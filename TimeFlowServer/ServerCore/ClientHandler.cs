using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Serilog;
using TimeFlow.Data.Repositories;
using TimeFlow.Models;
using TimeFlowServer.Security;

namespace TimeFlowServer.ServerCore
{
    // Xu ly tung ket noi client va xu ly cac message
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly UserRepository _userRepo;
        private readonly ActivityLogRepository _activityLogRepo;
        private readonly JwtManager _jwtManager;
        private readonly Dictionary<string, TcpClient> _onlineClients;
        private readonly object _clientsLock;
        
        private string? _currentUsername;
        private readonly string _clientId;

        public ClientHandler(
            TcpClient client,
            UserRepository userRepo,
            ActivityLogRepository activityLogRepo,
            JwtManager jwtManager,
            Dictionary<string, TcpClient> onlineClients,
            object clientsLock)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = client.GetStream();
            _userRepo = userRepo;
            _activityLogRepo = activityLogRepo;
            _jwtManager = jwtManager;
            _onlineClients = onlineClients;
            _clientsLock = clientsLock;
            _clientId = client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (_client.Connected && !cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    
                    if (bytesRead == 0)
                    {
                        Log.Information($"[{_clientId}] Client disconnected (zero bytes)");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await ProcessMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Connection error");
            }
            finally
            {
                Cleanup();
            }
        }

        private async Task ProcessMessageAsync(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("type", out JsonElement typeElem))
                {
                    Log.Warning($"[{_clientId}] Message without 'type' field");
                    return;
                }

                string type = typeElem.GetString() ?? "";

                switch (type)
                {
                    case "login":
                        await HandleLoginAsync(root);
                        break;

                    case "register":
                        await HandleRegisterAsync(root);
                        break;

                    case "autologin":
                        await HandleAutoLoginAsync(root);
                        break;

                    case "chat":
                        await HandleChatAsync(root);
                        break;

                    default:
                        Log.Warning($"[{_clientId}] Unknown message type: {type}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                Log.Warning(ex, $"[{_clientId}] Invalid JSON received");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error processing message");
            }
        }

        private async Task HandleLoginAsync(JsonElement root)
        {
            try
            {
                var data = root.GetProperty("data");
                string username = data.GetProperty("username").GetString() ?? "";
                string password = data.GetProperty("password").GetString() ?? "";

                Log.Information($"[{_clientId}] Login attempt for user: {username}");

                string hashedPassword = PasswordHasher.HashPassword(password);
                var user = _userRepo.ValidateLogin(username, hashedPassword);

                if (user != null && user.IsActive)
                {
                    _currentUsername = user.Username;

                    // Cap nhat last login
                    _userRepo.UpdateLastLogin(user.UserId);

                    // Them vao danh sach online clients
                    lock (_clientsLock)
                    {
                        if (_onlineClients.ContainsKey(_currentUsername))
                        {
                            Log.Information($"[{_clientId}] User {_currentUsername} already online, replacing connection");
                            try { _onlineClients[_currentUsername].Close(); } catch { }
                        }
                        _onlineClients[_currentUsername] = _client;
                    }

                    // Tao JWT token
                    string token = _jwtManager.CreateToken(_currentUsername);

                    // Gui response thanh cong
                    var response = new
                    {
                        status = "success",
                        token = token,
                        user = new
                        {
                            userId = user.UserId,
                            username = user.Username,
                            email = user.Email,
                            fullName = user.FullName
                        }
                    };

                    await SendResponseAsync(JsonSerializer.Serialize(response));

                    Log.Information($"[{_clientId}] ✓ User '{_currentUsername}' logged in successfully");

                    // Log activity
                    _activityLogRepo.LogActivity(user.UserId, null, "Login", "User logged in via TCP");
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "fail" }));
                    Log.Warning($"[{_clientId}] ✗ Login failed for user: {username}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Login error");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error" }));
            }
        }

        private async Task HandleRegisterAsync(JsonElement root)
        {
            try
            {
                var data = root.GetProperty("data");
                string username = data.GetProperty("username").GetString() ?? "";
                string password = data.GetProperty("password").GetString() ?? "";
                string email = data.GetProperty("email").GetString() ?? "";

                Log.Information($"[{_clientId}] Register attempt for user: {username}");

                // Kiem tra username hoac email da ton tai
                if (_userRepo.UsernameOrEmailExists(username, email))
                {
                    await SendResponseAsync("exists");
                    Log.Warning($"[{_clientId}] ✗ Registration failed: Username or email already exists");
                    return;
                }

                // Tao user moi
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = PasswordHasher.HashPassword(password),
                    IsActive = true
                };

                int userId = _userRepo.Create(newUser);

                if (userId > 0)
                {
                    await SendResponseAsync("registered");
                    Log.Information($"[{_clientId}] ✓ User '{username}' registered successfully (ID: {userId})");

                    // Log activity
                    _activityLogRepo.LogActivity(userId, null, "Register", "New user registered via TCP");
                }
                else
                {
                    await SendResponseAsync("error");
                    Log.Error($"[{_clientId}] ✗ Registration failed for user: {username}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Register error");
                await SendResponseAsync("error");
            }
        }

        private async Task HandleAutoLoginAsync(JsonElement root)
        {
            try
            {
                string token = root.GetProperty("token").GetString() ?? "";

                if (_jwtManager.ValidateToken(token, out string? username) && !string.IsNullOrEmpty(username))
                {
                    var user = _userRepo.GetByUsername(username);

                    if (user != null && user.IsActive)
                    {
                        _currentUsername = username;

                        // Cap nhat last login
                        _userRepo.UpdateLastLogin(user.UserId);

                        // Them vao danh sach online clients
                        lock (_clientsLock)
                        {
                            if (_onlineClients.ContainsKey(_currentUsername))
                            {
                                try { _onlineClients[_currentUsername].Close(); } catch { }
                            }
                            _onlineClients[_currentUsername] = _client;
                        }

                        var response = new
                        {
                            status = "autologin_success",
                            user = new
                            {
                                userId = user.UserId,
                                username = user.Username,
                                email = user.Email,
                                fullName = user.FullName
                            }
                        };

                        await SendResponseAsync(JsonSerializer.Serialize(response));

                        Log.Information($"[{_clientId}] ✓ User '{username}' auto-logged in successfully");

                        // Log activity
                        _activityLogRepo.LogActivity(user.UserId, null, "AutoLogin", "User auto-logged in via token");
                    }
                    else
                    {
                        await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
                        Log.Warning($"[{_clientId}] ✗ Auto-login failed: User not found or inactive");
                    }
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
                    Log.Warning($"[{_clientId}] ✗ Auto-login failed: Invalid token");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Auto-login error");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
            }
        }

        private async Task HandleChatAsync(JsonElement root)
        {
            if (string.IsNullOrEmpty(_currentUsername))
            {
                Log.Warning($"[{_clientId}] Chat message from unauthenticated client");
                return;
            }

            try
            {
                string receiver = root.GetProperty("receiver").GetString() ?? "";
                string content = root.GetProperty("content").GetString() ?? "";

                Log.Information($"[CHAT] {_currentUsername} → {receiver}: {content}");

                // Tim receiver client
                TcpClient? receiverClient = null;
                lock (_clientsLock)
                {
                    if (_onlineClients.TryGetValue(receiver, out var client))
                    {
                        receiverClient = client;
                    }
                }

                if (receiverClient != null && receiverClient.Connected)
                {
                    try
                    {
                        var message = new
                        {
                            type = "receive_message",
                            sender = _currentUsername,
                            content = content,
                            timestamp = DateTime.Now.ToString("HH:mm")
                        };

                        string json = JsonSerializer.Serialize(message);
                        byte[] bytes = Encoding.UTF8.GetBytes(json);
                        
                        var stream = receiverClient.GetStream();
                        await stream.WriteAsync(bytes, 0, bytes.Length);

                        Log.Information($"[CHAT] ✓ Message delivered to {receiver}");
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, $"[CHAT] ✗ Failed to deliver message to {receiver}");
                    }
                }
                else
                {
                    Log.Warning($"[CHAT] ✗ User {receiver} is offline - message not delivered");
                    // TODO: Luu vao database de gui lai sau
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Chat error");
            }
        }

        private async Task SendResponseAsync(string response)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(response);
                await _stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Failed to send response");
            }
        }

        private void Cleanup()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentUsername))
                {
                    lock (_clientsLock)
                    {
                        if (_onlineClients.TryGetValue(_currentUsername, out var client) && client == _client)
                        {
                            _onlineClients.Remove(_currentUsername);
                            Log.Information($"[{_clientId}] User '{_currentUsername}' removed from online list");
                        }
                    }

                    // Log logout activity
                    try
                    {
                        var user = _userRepo.GetByUsername(_currentUsername);
                        if (user != null)
                        {
                            _activityLogRepo.LogActivity(user.UserId, null, "Logout", "User disconnected from TCP");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to log logout activity");
                    }
                }

                _client.Close();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Cleanup error");
            }
        }
    }
}
