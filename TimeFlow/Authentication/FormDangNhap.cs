using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeFlow.Models;

namespace TimeFlow.Authentication
{
    public partial class FormDangNhap : Form
    {
        public FormDangNhap()
        {
            InitializeComponent();
            passwordTxtbox.PasswordChar = '*';
        }

        public class LoginResult
        {
            public bool Success { get; set; }
            public string Token { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public TcpClient ConnectedClient { get; set; }
        }

        private void FormDangNhap_Load(object sender, EventArgs e)
        {

        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string username = usernameTxtbox.Text.Trim();
            string password = passwordTxtbox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    var result = SendLoginRequest(username, password);
                    this.Invoke((MethodInvoker)delegate
                    {
                        if (result.Success)
                        {
                            SetupGlobalState(result);
                            MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // ✅ SỬA: Chỉ báo OK và đóng form login
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)(() => MessageBox.Show("Lỗi kết nối: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                }
            });
        }

        // Hàm phụ trợ để set GlobalState (tránh lặp code)
        private void SetupGlobalState(LoginResult result)
        {
            GlobalState.Client = result.ConnectedClient;

            // Xử lý an toàn cho UserId
            int userId = 0;
            try { userId = Convert.ToInt32(SessionManager.CurrentUserId); } catch { }

            GlobalState.CurrentUser = new User
            {
                UserId = userId,
                Username = result.Username,
                Email = result.Email
            };
        }

        private void buttonSignup_Click(object sender, EventArgs e)
        {
            FormDangKi dkForm = new FormDangKi();
            dkForm.Show();
            this.Hide();
        }

        private void CheckBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            passwordTxtbox.PasswordChar = checkBoxShowPassword.Checked ? '\0' : '*';
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Chức năng đang phát triển.", "Thông báo");
        }
        private LoginResult SendLoginRequest(string username, string password)
        {
            var loginData = new
            {
                type = "login",
                data = new { username = username, password = password }
            };

            string json = System.Text.Json.JsonSerializer.Serialize(loginData);
            byte[] sendBytes = Encoding.UTF8.GetBytes(json);

            TcpClient client = new TcpClient();
            try
            {
                client.ReceiveTimeout = 3000;
                client.SendTimeout = 3000;
                client.Connect("127.0.0.1", 1010);

                NetworkStream stream = client.GetStream();
                stream.Write(sendBytes, 0, sendBytes.Length);
                stream.Flush();

                byte[] buffer = new byte[4096];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                string status = root.GetProperty("status").GetString();

                if (status == "success")
                {
                    string token = root.GetProperty("token").GetString();
                    int userId = root.GetProperty("user").GetProperty("userId").GetInt32();
                    string usernameResp = root.GetProperty("user").GetProperty("username").GetString();
                    string email = root.GetProperty("user").GetProperty("email").GetString();

                    string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "token.jwt");
                    File.WriteAllText(tokenPath, token);

                    SessionManager.SetUserSession(userId, usernameResp, email, token);

                    return new LoginResult
                    {
                        Success = true,
                        Token = token,
                        Username = usernameResp,
                        Email = email,
                        ConnectedClient = client
                    };
                }
                else
                {
                    client.Close();
                    return new LoginResult { Success = false };
                }
            }
            catch
            {
                client.Close();
                throw;
            }
        }
    }
}