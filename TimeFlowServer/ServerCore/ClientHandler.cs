using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Serilog;
using TimeFlow.Data.Repositories;
using TimeFlow.Models;
using TimeFlowServer.Security;

namespace TimeFlowServer.ServerCore
{
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly UserRepository _userRepo;
        private readonly ActivityLogRepository _activityLogRepo;
        private readonly MessageRepository _messageRepo;
        private readonly GroupMemberRepository _groupMemberRepo;
        private readonly GroupRepository _groupRepo;
        private readonly JwtManager _jwtManager;
        private readonly Dictionary<string, TcpClient> _onlineClients;
        private readonly object _clientsLock;
        private string? _currentUsername;
        private readonly string _clientId;

        public ClientHandler(TcpClient client, UserRepository userRepo, ActivityLogRepository activityLogRepo, MessageRepository messageRepo, GroupMemberRepository groupMemberRepo, GroupRepository groupRepo, JwtManager jwtManager, Dictionary<string, TcpClient> onlineClients, object clientsLock)
        {
            _client = client;
            _stream = client.GetStream();
            _userRepo = userRepo;
            _activityLogRepo = activityLogRepo;
            _messageRepo = messageRepo;
            _groupMemberRepo = groupMemberRepo;
            _groupRepo = groupRepo;
            _jwtManager = jwtManager;
            _onlineClients = onlineClients;
            _clientsLock = clientsLock;
            _clientId = client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[8192];
            try
            {
                while (_client.Connected && !cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead == 0) break;
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await ProcessMessageAsync(message);
                }
            }
            catch (Exception ex) { Log.Warning($"[{_clientId}] Connection error: {ex.Message}"); }
            finally { Cleanup(); }
        }

        private async Task ProcessMessageAsync(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;
                string type = root.GetProperty("type").GetString() ?? "";

                switch (type)
                {
                    case "login": await HandleLoginAsync(root); break;
                    case "register": await HandleRegisterAsync(root); break;
                    case "autologin": await HandleAutoLoginAsync(root); break;
                    case "chat": await HandleChatAsync(root); break;
                    case "get_history": await HandleGetHistoryAsync(root); break;
                    case "get_group_history":
                        int gId = root.GetProperty("groupId").GetInt32();
                        var history = _messageRepo.GetGroupHistory(gId);
                        await SendResponseAsync(JsonSerializer.Serialize(new { type = "group_history_response", groupId = gId, messages = history }));
                        break;
                }
            }
            catch (Exception ex) { Log.Error(ex, $"[{_clientId}] Error processing message"); }
        }

        private async Task HandleLoginAsync(JsonElement root)
        {
            var data = root.GetProperty("data");
            string username = data.GetProperty("username").GetString() ?? "";
            string password = data.GetProperty("password").GetString() ?? "";
            string hashedPassword = PasswordHasher.HashPassword(password);
            var user = _userRepo.ValidateLogin(username, hashedPassword);

            if (user != null && user.IsActive)
            {
                _currentUsername = user.Username;
                _userRepo.UpdateLastLogin(user.UserId);
                lock (_clientsLock) { _onlineClients[_currentUsername] = _client; }

                var userGroups = _groupRepo.GetByUserId(user.UserId);
                string token = _jwtManager.CreateToken(_currentUsername);

                var response = new
                {
                    status = "success",
                    token = token,
                    user = new { userId = user.UserId, username = user.Username, email = user.Email },
                    groups = userGroups
                };
                await SendResponseAsync(JsonSerializer.Serialize(response));
                await BroadcastOnlineUsers();
            }
            else { await SendResponseAsync(JsonSerializer.Serialize(new { status = "fail" })); }
        }

        private async Task HandleRegisterAsync(JsonElement root)
        {
            var data = root.GetProperty("data");
            string username = data.GetProperty("username").GetString() ?? "";
            string email = data.GetProperty("email").GetString() ?? "";
            if (_userRepo.UsernameOrEmailExists(username, email)) { await SendResponseAsync("exists"); return; }

            var newUser = new User { Username = username, Email = email, PasswordHash = PasswordHasher.HashPassword(data.GetProperty("password").GetString()), IsActive = true };
            int userId = _userRepo.Create(newUser);
            await SendResponseAsync(userId > 0 ? "registered" : "error");
        }

        private async Task HandleAutoLoginAsync(JsonElement root)
        {
            string token = root.GetProperty("token").GetString() ?? "";
            if (_jwtManager.ValidateToken(token, out string? username) && !string.IsNullOrEmpty(username))
            {
                var user = _userRepo.GetByUsername(username);
                if (user != null && user.IsActive)
                {
                    _currentUsername = username;
                    lock (_clientsLock) { _onlineClients[_currentUsername] = _client; }
                    var userGroups = _groupRepo.GetByUserId(user.UserId);
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_success", user = new { userId = user.UserId, username = user.Username }, groups = userGroups }));
                    await BroadcastOnlineUsers();
                    return;
                }
            }
            await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
        }

        private async Task HandleChatAsync(JsonElement root)
        {
            string content = root.GetProperty("content").GetString() ?? "";
            if (root.TryGetProperty("groupId", out JsonElement gElem))
            {
                int gId = gElem.GetInt32();
                _messageRepo.AddGroupMessage(_currentUsername!, gId, content);
                var members = _groupMemberRepo.GetByGroupId(gId);
                foreach (var m in members)
                {
                    var uName = _userRepo.GetById(m.UserId)?.Username;
                    if (uName != null && uName != _currentUsername && _onlineClients.TryGetValue(uName, out var target))
                    {
                        var msg = new { type = "receive_group_message", groupId = gId, sender = _currentUsername, content = content, timestamp = DateTime.Now.ToString("HH:mm") };
                        byte[] b = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
                        await target.GetStream().WriteAsync(b, 0, b.Length);
                    }
                }
            }
            else
            {
                string receiver = root.GetProperty("receiver").GetString() ?? "";
                if (_currentUsername != null)
                {
                    _messageRepo.AddMessage(_currentUsername, receiver, content);
                }
                if (_onlineClients.TryGetValue(receiver, out var target))
                {
                    var msg = new { type = "receive_message", sender = _currentUsername, content = content, timestamp = DateTime.Now.ToString("HH:mm") };
                    byte[] b = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(msg));
                    await target.GetStream().WriteAsync(b, 0, b.Length);
                }
            }
        }

        private async Task HandleGetHistoryAsync(JsonElement root)
        {
            string targetUser = root.GetProperty("target_user").GetString() ?? "";
            var history = _messageRepo.GetHistory(_currentUsername!, targetUser);
            await SendResponseAsync(JsonSerializer.Serialize(new { type = "history_data", data = history }));
        }

        private async Task BroadcastOnlineUsers()
        {
            List<string> userList;
            lock (_clientsLock) { userList = _onlineClients.Keys.ToList(); }
            byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { type = "user_list", users = userList }));
            foreach (var client in _onlineClients.Values) { if (client.Connected) await client.GetStream().WriteAsync(bytes, 0, bytes.Length); }
        }

        private async Task SendResponseAsync(string response) { byte[] bytes = Encoding.UTF8.GetBytes(response); await _stream.WriteAsync(bytes, 0, bytes.Length); }

        private void Cleanup() { lock (_clientsLock) { if (_currentUsername != null) _onlineClients.Remove(_currentUsername); } _client.Close(); }
    }
}