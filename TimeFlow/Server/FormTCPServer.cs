using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using TimeFlow.Data.Repositories;
using TimeFlow.Models;

namespace TimeFlow.Server
{
    public partial class FormTCPServer : Form
    {
        public FormTCPServer()
        {
            InitializeComponent();
        }

        // --- CAC BIEN TOAN CUC ---
        private TcpListener tcpListener;
        private Thread serverThread;

        // Chuỗi kết nối Database
        private string connectionString = "Data Source=localhost;Initial Catalog=UserDB;User ID=myuser;Password=YourStrong@Passw0rd;TrustServerCertificate=True";

        // DANH SÁCH USER ONLINE: Map từ Username -> Socket Client
        private static Dictionary<string, TcpClient> _onlineClients = new Dictionary<string, TcpClient>();
        private static object _lock = new object();

        // --- CAC CLASS DU LIEU (DTO) ---
        public class LoginRequest { public string username { get; set; } public string password { get; set; } }
        public class RegisterRequest { public string username { get; set; } public string password { get; set; } public string email { get; set; } }

        // --- SU KIEN UI ---
        private void buttonListen_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBoxPortNumber.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Port không hợp lệ."); return;
            }

            // Test database connection truoc khi start server
            try
            {
                AppendLog("Testing database connection...");
                var testDb = new Data.DatabaseHelper();
                if (testDb.TestConnection())
                {
                    AppendLog("✓ Database connection successful!");
                    AppendLog($"Connection string: {testDb.GetConnectionString()}");
                    
                    // Test query de kiem tra table Users ton tai
                    var testQuery = "SELECT COUNT(*) FROM Users";
                    var count = testDb.ExecuteScalar(testQuery, null);
                    AppendLog($"✓ Users table exists with {count} users");
                }
            }
            catch (Exception ex)
            {
                AppendLog($"✗ Database connection failed: {ex.Message}");
                if (ex.InnerException != null)
                    AppendLog($"  Inner error: {ex.InnerException.Message}");
                
                MessageBox.Show($"Database connection failed:\n{ex.Message}\n\nPlease check:\n1. SQL Server is running\n2. Database 'TimeFlowDB' exists\n3. Run TimeFlowDB_Schema.sql and TestData.sql", 
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            serverThread = new Thread(() => StartServer(port));
            serverThread.IsBackground = true;
            serverThread.Start();
            AppendLog($"Server đang chạy tại cổng {port}...");
        }

        // --- LOGIC SERVER CHINH ---
        private void StartServer(int port)
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();

                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                AppendLog("Lỗi Server: " + ex.Message);
            }
        }

        // --- HAM XU LY CLIENT ---
        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
            string currentUsername = null;

            try
            {
                while (client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    using JsonDocument doc = JsonDocument.Parse(message);
                    JsonElement root = doc.RootElement;
                    string type = root.GetProperty("type").GetString();

                    // --- XU LY CAC LOAI GOI TIN ---

                    if (type == "login")
                    {
                        var data = root.GetProperty("data").Deserialize<LoginRequest>();
                        var user = ValidateLogin(data.username, data.password);
                        
                        if (user != null)
                        {
                            currentUsername = user.Username;
                            string token = CreateJwtToken(currentUsername);

                            // Cap nhat last login
                            _userRepo.UpdateLastLogin(user.UserId);

                            // Luu vao danh sach Online
                            lock (_lock)
                            {
                                if (_onlineClients.ContainsKey(currentUsername))
                                    _onlineClients[currentUsername].Close();
                                _onlineClients[currentUsername] = client;
                            }

                            string res = JsonSerializer.Serialize(new
                            {
                                status = "success",
                                token = token,
                                user = new { 
                                    userId = user.UserId,
                                    username = user.Username, 
                                    email = user.Email,
                                    fullName = user.FullName
                                }
                            });
                            SendResponse(client, res);
                            AppendLog($"User '{currentUsername}' đã đăng nhập.");

                            // Log activity
                            _activityLogRepo.LogActivity(user.UserId, null, "Login", "User logged in");
                        }
                        else
                        {
                            SendResponse(client, JsonSerializer.Serialize(new { status = "fail" }));
                        }
                    }
                    else if (type == "register")
                    {
                        var data = root.GetProperty("data").Deserialize<RegisterRequest>();
                        var result = RegisterNewUser(data.username, data.password, data.email);
                        SendResponse(client, result ? "registered" : "exists");
                    }
                    else if (type == "autologin")
                    {
                        string token = root.GetProperty("token").GetString();
                        if (IsJwtTokenValid(token, out string username))
                        {
                            var user = _userRepo.GetByUsername(username);
                            if (user != null && user.IsActive)
                            {
                                currentUsername = username;

                                // Cap nhat last login
                                _userRepo.UpdateLastLogin(user.UserId);

                                lock (_lock)
                                {
                                    if (_onlineClients.ContainsKey(currentUsername))
                                        _onlineClients[currentUsername].Close();
                                    _onlineClients[currentUsername] = client;
                                }

                                string res = JsonSerializer.Serialize(new
                                {
                                    status = "autologin_success",
                                    user = new { 
                                        userId = user.UserId,
                                        username = user.Username, 
                                        email = user.Email,
                                        fullName = user.FullName
                                    }
                                });
                                SendResponse(client, res);
                                AppendLog($"User '{username}' Re-login thành công.");

                                // Log activity
                                _activityLogRepo.LogActivity(user.UserId, null, "AutoLogin", "User auto-logged in");
                            }
                            else
                            {
                                SendResponse(client, JsonSerializer.Serialize(new { status = "autologin_fail" }));
                            }
                        }
                        else
                        {
                            SendResponse(client, JsonSerializer.Serialize(new { status = "autologin_fail" }));
                        }
                    }
                    else if (type == "chat")
                    {
                        if (!string.IsNullOrEmpty(currentUsername))
                        {
                            string receiver = root.GetProperty("receiver").GetString();
                            string content = root.GetProperty("content").GetString();
                            RouteMessage(currentUsername, receiver, content);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppendLog($"Client lỗi/ngắt kết nối: {ex.Message}");
            }
            finally
            {
                if (!string.IsNullOrEmpty(currentUsername))
                {
                    lock (_lock)
                    {
                        if (_onlineClients.ContainsKey(currentUsername) && _onlineClients[currentUsername] == client)
                        {
                            _onlineClients.Remove(currentUsername);
                        }
                    }
                    AppendLog($"User '{currentUsername}' đã Offline.");

                    // Log logout
                    var user = _userRepo.GetByUsername(currentUsername);
                    if (user != null)
                    {
                        _activityLogRepo.LogActivity(user.UserId, null, "Logout", "User logged out");
                    }
                }
                client.Close();
            }
        }

        // --- HAM DIEU HUONG TIN NHAN (ROUTING) ---
        private void RouteMessage(string sender, string receiver, string content)
        {
            TcpClient receiverClient = null;

            lock (_lock)
            {
                if (_onlineClients.ContainsKey(receiver))
                {
                    receiverClient = _onlineClients[receiver];
                }
            }

            if (receiverClient != null && receiverClient.Connected)
            {
                try
                {
                    var msgObj = new
                    {
                        type = "receive_message",
                        sender = sender,
                        content = content,
                        timestamp = DateTime.Now.ToString("HH:mm")
                    };
                    string json = JsonSerializer.Serialize(msgObj);
                    SendResponse(receiverClient, json);
                    AppendLog($"[Chat] {sender} -> {receiver}: {content}");
                }
                catch
                {
                    AppendLog($"Gửi tin tới {receiver} thất bại.");
                }
            }
            else
            {
                // TODO: Luu vao Database bang 'Messages' neu user Offline
                AppendLog($"[Chat] {sender} -> {receiver} (Offline) - Cần lưu DB.");
            }
        }

        private void SendResponse(TcpClient client, string response)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(response);
            client.GetStream().Write(bytes, 0, bytes.Length);
        }

        private void AppendLog(string message)
        {
            if (richTextBoxMessage.InvokeRequired)
                richTextBoxMessage.Invoke(new MethodInvoker(() => richTextBoxMessage.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n")));
            else
                richTextBoxMessage.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
        }

        // --- KHU VUC CAC HAM HELPER & DATABASE ---

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Chuyển password nhập vào thành byte
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Chuyển byte thành chuỗi Hex (viết thường)
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // REFACTORED: Dung UserRepository thay vi raw SQL
        private User? ValidateLogin(string username, string password)
        {
            // 1. In ra thông tin đầu vào
            string inputHash = HashPassword(password);
            AppendLog($"[DEBUG] Đang check login: User={username}");
            AppendLog($"[DEBUG] Hash từ Client: {inputHash}");

            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                string query = "SELECT COUNT(*) FROM Users WHERE Username = @u AND Password = @p";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", HashPassword(password));
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        // REFACTORED: Dung UserRepository thay vi raw SQL
        private bool RegisterNewUser(string username, string password, string email)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                string check = "SELECT COUNT(*) FROM Users WHERE Username = @u OR Email = @e";
                using SqlCommand cmdCheck = new SqlCommand(check, conn);
                cmdCheck.Parameters.AddWithValue("@u", username);
                cmdCheck.Parameters.AddWithValue("@e", email);
                if ((int)cmdCheck.ExecuteScalar() > 0) return false;

                string query = "INSERT INTO Users (Username, Password, Email) VALUES (@u, @p, @e)";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", HashPassword(password));
                cmd.Parameters.AddWithValue("@e", email);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }

        private string GetEmailByUsername(string username)
        {
            try
            {
                AppendLog("Register Error: " + ex.Message);
                return false;
            }
        }

        // --- KHU VUC JWT (TOKEN) ---
        private string _secretKey = "your_super_secret_key_change_this_to_something_long";

        private string CreateJwtToken(string username)
        {
            var header = JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" });
            var payload = JsonSerializer.Serialize(new
            {
                username = username,
                exp = DateTimeOffset.UtcNow.AddMinutes(60).ToUnixTimeSeconds()
            });

            string encodedHeader = Base64UrlEncode(header);
            string encodedPayload = Base64UrlEncode(payload);
            string signature = HmacSha256($"{encodedHeader}.{encodedPayload}", _secretKey);

            return $"{encodedHeader}.{encodedPayload}.{signature}";
        }

        private bool IsJwtTokenValid(string token, out string username)
        {
            username = null;
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return false;

                string header = parts[0];
                string payload = parts[1];
                string signature = parts[2];

                string expectedSignature = HmacSha256($"{header}.{payload}", _secretKey);
                if (signature != expectedSignature) return false;

                string payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(payload)));
                using var doc = JsonDocument.Parse(payloadJson);

                long exp = doc.RootElement.GetProperty("exp").GetInt64();
                if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp) return false;

                username = doc.RootElement.GetProperty("username").GetString();
                return true;
            }
            catch { return false; }
        }

        private string PadBase64(string base64) => base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=').Replace('-', '+').Replace('_', '/');

        private string Base64UrlEncode(string input) => Convert.ToBase64String(Encoding.UTF8.GetBytes(input)).Replace("=", "").Replace('+', '-').Replace('/', '_');

        private string HmacSha256(string data, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("=", "").Replace('+', '-').Replace('/', '_');
            }
        }
    }
}