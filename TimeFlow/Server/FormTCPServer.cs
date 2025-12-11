using Microsoft.Data.SqlClient;
using System;
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

        private TcpListener tcpListener;
        private Thread serverThread;
        private string connectionString = "Data Source=localhost;Initial Catalog=UserDB;User ID=myuser;Password=YourStrong@Passw0rd;TrustServerCertificate=True";

        public class LoginRequest
        {
            public string username { get; set; }
            public string password { get; set; }
        }
        public class RegisterRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string email { get; set; }
        }

        private void buttonListen_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBoxPortNumber.Text, out int port) || port < 1 || port > 65535)
            {
                MessageBox.Show("Vui lòng nhập port hợp lệ (1–65535).", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            serverThread = new Thread(() => StartServer(port));
            serverThread.IsBackground = true;
            serverThread.Start();
            AppendLog("Đang lắng nghe tại cổng " + port);
        }

        private void StartServer(int port)
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();

                while (true)
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    AppendLog("Đã chấp nhận kết nối mới từ client!");

                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.IsBackground = true;
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                AppendLog("Lỗi server: " + ex.Message);
            }
        }

        private void HandleClient(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[2048];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                AppendLog("Nhận từ client: " + receivedMessage);

                using JsonDocument doc = JsonDocument.Parse(receivedMessage);
                string type = doc.RootElement.GetProperty("type").GetString();

                string response = "error";

                if (type == "login")
                {
                    var loginData = doc.RootElement.GetProperty("data").Deserialize<LoginRequest>();
                    bool isValid = ValidateLogin(loginData.username, loginData.password);
                    if (isValid)
                    {
                        string email = GetEmailByUsername(loginData.username);  // Lấy email từ DB
                        string token = CreateJwtToken(loginData.username);      // Tạo token JWT

                        // Đóng gói JSON trả về
                        var jsonResponse = new
                        {
                            status = "success",
                            token = token,
                            user = new
                            {
                                username = loginData.username,
                                email = email
                            }
                        };
                        response = JsonSerializer.Serialize(jsonResponse);
                    }
                    else
                    {
                        var jsonResponse = new
                        {
                            status = "fail"
                        };
                        response = JsonSerializer.Serialize(jsonResponse);
                    }
                }
                else if (type == "register")
                {
                    var registerData = doc.RootElement.GetProperty("data").Deserialize<RegisterRequest>();
                    bool success = RegisterNewUser(registerData.username, registerData.password, registerData.email);
                    response = success ? "registered" : "exists";
                }
                else if (type == "autologin")
                {
                    string token = doc.RootElement.GetProperty("token").GetString();
                    if (IsJwtTokenValid(token, out string username))
                    {
                        string email = GetEmailByUsername(username);
                        var jsonResponse = new
                        {
                            status = "autologin_success",
                            user = new
                            {
                                username = username,
                                email = email
                            }
                        };
                        response = JsonSerializer.Serialize(jsonResponse);
                    }
                    else
                    {
                        var jsonResponse = new
                        {
                            status = "autologin_fail"
                        };
                        response = JsonSerializer.Serialize(jsonResponse);
                    }
                }

                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                stream.Write(responseBytes, 0, responseBytes.Length);
                stream.Flush();
                Thread.Sleep(100);
                AppendLog("Đã gửi phản hồi: " + response);
            }
            catch (JsonException jsonEx)
            {
                AppendLog("Lỗi JSON: " + jsonEx.Message);
            }
            catch (Exception ex)
            {
                AppendLog("Lỗi xử lý client: " + ex.Message);
            }
            finally
            {
                client.Close();
                AppendLog("Kết thúc kết nối với client.");
            }
        }

        private void AppendLog(string message)
        {
            if (richTextBoxMessage.InvokeRequired)
            {
                richTextBoxMessage.Invoke(new MethodInvoker(() =>
                    richTextBoxMessage.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n")));
            }
            else
            {
                richTextBoxMessage.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
            }
        }

        private string HashPassword(string password) 
        { 
            using (SHA256 sha256 = SHA256.Create()) 
            { 
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password)); 
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
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();
            string query = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";
            using SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@Password", HashPassword(password));
            int count = (int)cmd.ExecuteScalar();
            return count > 0;
        }
        private bool RegisterNewUser(string username, string password, string email)
        {
            using SqlConnection conn = new SqlConnection(connectionString);
            conn.Open();

            // Kiểm tra username/email đã tồn tại chưa
            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username OR Email = @Email";
            using SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@Username", username);
            checkCmd.Parameters.AddWithValue("@Email", email);
            int existing = (int)checkCmd.ExecuteScalar();
            if (existing > 0) return false;

            // Nếu chưa tồn tại thì thêm mới
            string insertQuery = "INSERT INTO Users (Username, Password, Email) VALUES (@Username, @Password, @Email)";
            using SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@Username", username);
            insertCmd.Parameters.AddWithValue("@Password", HashPassword(password));
            insertCmd.Parameters.AddWithValue("@Email", email);
            int rows = insertCmd.ExecuteNonQuery();
            return rows > 0;
        }

        private string GetEmailByUsername(string username)
        {
            try
            {
                using SqlConnection conn = new SqlConnection(connectionString);
                conn.Open();

                string query = "SELECT Email FROM Users WHERE Username = @Username";
                using SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", username);

                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
            catch (Exception ex)
            {
                AppendLog("Lỗi lấy email: " + ex.Message);
                return "";
            }
        }

        private string CreateJwtToken(string username)
        {
            var header = new { alg = "HS256", typ = "JWT" };
            var payload = new
            {
                username = username,
                exp = DateTimeOffset.UtcNow.AddMinutes(30).ToUnixTimeSeconds()
            };

            string headerJson = JsonSerializer.Serialize(header);
            string payloadJson = JsonSerializer.Serialize(payload);

            string encodedHeader = Base64UrlEncode(headerJson);
            string encodedPayload = Base64UrlEncode(payloadJson);

            string secretKey = "your_super_secret_key";
            string signature = HmacSha256($"{encodedHeader}.{encodedPayload}", secretKey);

            return $"{encodedHeader}.{encodedPayload}.{signature}";
        }
        private bool IsJwtTokenValid(string token, out string username)
        {
            username = null;

            try
            {
                string secretKey = "your_super_secret_key";

                // Tách 3 phần: header.payload.signature
                var parts = token.Split('.');
                if (parts.Length != 3) return false;

                string header = parts[0];
                string payload = parts[1];
                string signature = parts[2];

                // Tính lại chữ ký từ header + payload
                string dataToSign = $"{header}.{payload}";
                string expectedSignature = HmacSha256(dataToSign, secretKey);

                // So sánh chữ ký
                if (signature != expectedSignature)
                    return false;

                // Giải mã payload từ base64 để lấy username và thời gian hết hạn
                string payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(payload)));
                using var doc = JsonDocument.Parse(payloadJson);
                var root = doc.RootElement;

                username = root.GetProperty("username").GetString();
                long exp = root.GetProperty("exp").GetInt64();
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                // Kiểm tra hạn token
                return now <= exp;
            }
            catch
            {
                return false;
            }
        }
        private string PadBase64(string base64)
        {
            return base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=')
                 .Replace('-', '+')
                 .Replace('_', '/');
        }
        private string Base64UrlEncode(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            return Convert.ToBase64String(bytes)
                .Replace("=", "")
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private string HmacSha256(string data, string secret)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hash)
                    .Replace("=", "")
                    .Replace('+', '-')
                    .Replace('/', '_');
            }
        }
    }
}
