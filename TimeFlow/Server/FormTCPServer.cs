using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic; // Cần thiết cho Dictionary
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace TimeFlow.Server
{
    public partial class FormTCPServer : Form
    {
        public FormTCPServer()
        {
            InitializeComponent();
        }

        // --- CÁC BIẾN TOÀN CỤC ---
        private TcpListener tcpListener;
        private Thread serverThread;

        // Chuỗi kết nối Database
        private string connectionString = "Server=localhost,1433;Database=TimeFlowDB;User Id=myuser;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Integrated Security=False;";
        // DANH SÁCH USER ONLINE: Map từ Username -> Socket Client
        private static Dictionary<string, TcpClient> _onlineClients = new Dictionary<string, TcpClient>();
        private static object _lock = new object(); // Khóa an toàn cho luồng

        // --- CÁC CLASS DỮ LIỆU (DTO) ---
        public class LoginRequest { public string username { get; set; } public string password { get; set; } }
        public class RegisterRequest { public string username { get; set; } public string password { get; set; } public string email { get; set; } }

        // --- SỰ KIỆN UI ---
        private void buttonListen_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBoxPortNumber.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Port không hợp lệ."); return;
            }

            serverThread = new Thread(() => StartServer(port));
            serverThread.IsBackground = true;
            serverThread.Start();
            AppendLog($"Server đang chạy tại cổng {port}...");
        }

        // --- LOGIC SERVER CHÍNH ---
        private void StartServer(int port)
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                while (true)
                {
                    // Chấp nhận kết nối mới
                    TcpClient client = tcpListener.AcceptTcpClient();

                    // Tạo luồng riêng để phục vụ client này (Chat + Auth)
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

        // --- HÀM XỬ LÝ CLIENT ---
        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[4096];
            string currentUsername = null; // Định danh user của kết nối này

            try
            {
                // Vòng lặp giữ kết nối (Persistent Connection)
                while (client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Client ngắt kết nối

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    // Phân tích JSON
                    using JsonDocument doc = JsonDocument.Parse(message);
                    JsonElement root = doc.RootElement;
                    string type = root.GetProperty("type").GetString();

                    // --- XỬ LÝ CÁC LOẠI GÓI TIN ---

                    if (type == "login")
                    {
                        var data = root.GetProperty("data").Deserialize<LoginRequest>();
                        if (ValidateLogin(data.username, data.password))
                        {
                            currentUsername = data.username;
                            string email = GetEmailByUsername(currentUsername);
                            string token = CreateJwtToken(currentUsername);

                            // Lưu vào danh sách Online
                            lock (_lock)
                            {
                                if (_onlineClients.ContainsKey(currentUsername))
                                    _onlineClients[currentUsername].Close(); // Kick phiên cũ
                                _onlineClients[currentUsername] = client;
                            }

                            string res = JsonSerializer.Serialize(new
                            {
                                status = "success",
                                token = token,
                                user = new { username = currentUsername, email = email }
                            });
                            SendResponse(client, res);
                            AppendLog($"User '{currentUsername}' đã đăng nhập.");
                        }
                        else
                        {
                            SendResponse(client, JsonSerializer.Serialize(new { status = "fail" }));
                        }
                    }
                    else if (type == "register")
                    {
                        var data = root.GetProperty("data").Deserialize<RegisterRequest>();
                        bool success = RegisterNewUser(data.username, data.password, data.email);
                        SendResponse(client, success ? "registered" : "exists");
                    }
                    else if (type == "autologin") // Tính năng cũ đã được khôi phục
                    {
                        string token = root.GetProperty("token").GetString();
                        if (IsJwtTokenValid(token, out string username))
                        {
                            currentUsername = username;
                            string email = GetEmailByUsername(username);

                            // Cập nhật trạng thái Online
                            lock (_lock)
                            {
                                if (_onlineClients.ContainsKey(currentUsername))
                                    _onlineClients[currentUsername].Close();
                                _onlineClients[currentUsername] = client;
                            }

                            string res = JsonSerializer.Serialize(new
                            {
                                status = "autologin_success",
                                user = new { username = username, email = email }
                            });
                            SendResponse(client, res);
                            AppendLog($"User '{username}' Re-login thành công.");
                        }
                        else
                        {
                            SendResponse(client, JsonSerializer.Serialize(new { status = "autologin_fail" }));
                        }
                    }
                    else if (type == "chat") // TÍNH NĂNG CHAT MỚI
                    {
                        // JSON mẫu: { "type": "chat", "receiver": "UserB", "content": "Hello" }
                        if (!string.IsNullOrEmpty(currentUsername)) // Chỉ cho phép chat nếu đã Login
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
                // Dọn dẹp khi User thoát
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
                }
                client.Close();
            }
        }

        // --- HÀM ĐIỀU HƯỚNG TIN NHẮN (ROUTING) ---
        private void RouteMessage(string sender, string receiver, string content)
        {
            TcpClient receiverClient = null;

            // 1. Tìm socket người nhận
            lock (_lock)
            {
                if (_onlineClients.ContainsKey(receiver))
                {
                    receiverClient = _onlineClients[receiver];
                }
            }

            // 2. Gửi tin nếu Online
            if (receiverClient != null && receiverClient.Connected)
            {
                try
                {
                    var msgObj = new
                    {
                        type = "receive_message", // Client cần bắt type này
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
                // TODO: Lưu vào Database bảng 'Messages' nếu user Offline
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

        // --- KHU VỰC CÁC HÀM HELPER & DATABASE ---

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

        private bool ValidateLogin(string username, string password)
        {
            // 1. In ra thông tin đầu vào
            string inputHash = HashPassword(password);
            AppendLog($"[DEBUG] Đang check login: User={username}");
            AppendLog($"[DEBUG] Hash từ Client: {inputHash}");

            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                AppendLog("[DEBUG] Kết nối Database: Thành công!");

                // 2. Kiểm tra xem User có tồn tại không trước
                string queryUser = "SELECT PasswordHash FROM Users WHERE Username = @u";
                using SqlCommand cmdUser = new SqlCommand(queryUser, conn);
                cmdUser.Parameters.AddWithValue("@u", username);

                object result = cmdUser.ExecuteScalar();

                if (result == null)
                {
                    AppendLog($"[LỖI] Không tìm thấy user '{username}' trong bảng Users.");
                    return false;
                }

                string dbHash = result.ToString();
                AppendLog($"[DEBUG] Hash trong DB:   {dbHash}");

                // 3. So sánh trực tiếp
                if (inputHash == dbHash)
                {
                    AppendLog("[SUCCESS] Mật khẩu khớp! Đăng nhập thành công.");
                    return true;
                }
                else
                {
                    AppendLog("[LỖI] Mật khẩu KHÔNG khớp.");
                    // So sánh độ dài để xem có bị thừa khoảng trắng không
                    AppendLog($"Độ dài Client: {inputHash.Length} - Độ dài DB: {dbHash.Length}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // QUAN TRỌNG: Đây là chỗ nó in ra lỗi SQL (VD: sai tên cột, sai user db...)
                AppendLog($"[EXCEPTION] Lỗi nghiêm trọng khi Login: {ex.Message}");
                return false;
            }
        }

        private bool RegisterNewUser(string username, string password, string email)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                // 1. Kiểm tra tồn tại
                string check = "SELECT COUNT(*) FROM Users WHERE Username = @u OR Email = @e";
                using SqlCommand cmdCheck = new SqlCommand(check, conn);
                cmdCheck.Parameters.AddWithValue("@u", username);
                cmdCheck.Parameters.AddWithValue("@e", email);

                int count = Convert.ToInt32(cmdCheck.ExecuteScalar());
                if (count > 0) return false;

                // 2. Thêm mới
                // SỬA: Đổi 'Password' thành 'PasswordHash'
                string query = "INSERT INTO Users (Username, PasswordHash, Email, CreatedAt, IsActive) VALUES (@u, @p, @e, GETDATE(), 1)";

                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", HashPassword(password));
                cmd.Parameters.AddWithValue("@e", email);

                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                AppendLog("Lỗi Register DB: " + ex.Message);
                return false;
            }
        }

        private string GetEmailByUsername(string username)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();
                string query = "SELECT Email FROM Users WHERE Username = @u";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", username);
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
            catch { return ""; }
        }

        // --- KHU VỰC JWT (TOKEN) ---
        private string _secretKey = "your_super_secret_key_change_this_to_something_long"; // Key dùng chung

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