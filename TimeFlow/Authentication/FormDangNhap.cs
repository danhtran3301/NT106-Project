using Microsoft.Data.SqlClient;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using TimeFlow.UI;
using TimeFlow.Configuration;

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
        }

        private void FormDangNhap_Load(object sender, EventArgs e)
        {
            string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\token.jwt");

            if (File.Exists(tokenPath))
            {
                string token = File.ReadAllText(tokenPath);

                Task.Run(() =>
                {
                    try
                    {
                        var result = SendAutoLoginRequest(token);

                        this.Invoke((MethodInvoker)(() =>
                        {
                            if (result.Success)
                            {
                                MessageBox.Show("Tự động đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.Hide();
                                FormGiaoDien mainForm = new FormGiaoDien();
                                mainForm.FormClosed += (s, args) => this.Close();
                                mainForm.Show();
                            }
                            else
                            {
                                File.Delete(tokenPath); // Token sai/hết hạn → xoá đi
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        // Không cần show lỗi lớn nếu token fail
                        this.Invoke((MethodInvoker)(() =>
                        {
                            MessageBox.Show("Tự đăng nhập thất bại: " + ex.Message);
                        }));
                    }
                });
            }
        }

        private void CheckBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            passwordTxtbox.PasswordChar = checkBoxShowPassword.Checked ? '\0' : '*';
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string username = usernameTxtbox.Text.Trim();
            string password = passwordTxtbox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Username và Password!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Chạy trong thread nền
            Task.Run(() =>
            {
                try
                {
                    var result = SendLoginRequest(username, password);

                    this.Invoke((MethodInvoker)delegate
                    {
                        if (result.Success)
                        {
                            MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.Hide();
                            FormGiaoDien mainForm = new FormGiaoDien();
                            mainForm.FormClosed += (s, args) => this.Close();
                            mainForm.Show();
                        }
                        else if (!result.Success)
                        {
                            MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            usernameTxtbox.Focus();
                        }
                    });
                }
                catch (TimeoutException)
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        MessageBox.Show("Timeout khi chờ phản hồi từ server.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        MessageBox.Show("Lỗi kết nối đến server: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        private void buttonSignup_Click(object sender, EventArgs e)
        {
            FormDangKi dkForm = new FormDangKi();
            dkForm.Show();
            this.Hide();
        }

        private LoginResult SendLoginRequest(string username, string password)
        {
            var loginData = new
            {
                type = "login",
                data = new
                {
                    username = username,
                    password = password
                }
            };

            string json = System.Text.Json.JsonSerializer.Serialize(loginData);
            byte[] sendBytes = Encoding.UTF8.GetBytes(json);

            using (TcpClient client = new TcpClient())
            {
                client.ReceiveTimeout = ServerConfig.Timeout;
                client.SendTimeout = ServerConfig.Timeout;
                client.Connect(ServerConfig.Host, ServerConfig.Port);
                
                using NetworkStream stream = client.GetStream();
                stream.Write(sendBytes, 0, sendBytes.Length);
                stream.Flush();

                byte[] buffer = new byte[2048];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                // Parse JSON
                using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();

                if (status == "success")
                {
                    string token = root.GetProperty("token").GetString();
                    int userId = root.GetProperty("user").GetProperty("userId").GetInt32();
                    string usernameResp = root.GetProperty("user").GetProperty("username").GetString();
                    string email = root.GetProperty("user").GetProperty("email").GetString();

                    // Lưu token ra file
                    string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\token.jwt");
                    File.WriteAllText(tokenPath, token);

                    // Lưu session
                    SessionManager.SetUserSession(userId, usernameResp, email, token);

                    return new LoginResult
                    {
                        Success = true,
                        Token = token,
                        Username = usernameResp,
                        Email = email
                    };
                }
                else
                {
                    return new LoginResult { Success = false };
                }
            }
        }
        private LoginResult SendAutoLoginRequest(string token)
        {
            var request = new
            {
                type = "autologin",
                token = token
            };

            string json = System.Text.Json.JsonSerializer.Serialize(request);
            byte[] sendBytes = Encoding.UTF8.GetBytes(json);

            using (TcpClient client = new TcpClient())
            {
                client.ReceiveTimeout = ServerConfig.Timeout;
                client.SendTimeout = ServerConfig.Timeout;
                client.Connect(ServerConfig.Host, ServerConfig.Port);

                using NetworkStream stream = client.GetStream();
                stream.Write(sendBytes, 0, sendBytes.Length);
                stream.Flush();

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                using var doc = System.Text.Json.JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                string status = root.GetProperty("status").GetString();

                if (status == "autologin_success")
                {
                    int userId = root.GetProperty("user").GetProperty("userId").GetInt32();
                    string username = root.GetProperty("user").GetProperty("username").GetString();
                    string email = root.GetProperty("user").GetProperty("email").GetString();

                    // Lưu session
                    SessionManager.SetUserSession(userId, username, email, token);

                    return new LoginResult
                    {
                        Success = true,
                        Username = username,
                        Email = email
                    };
                }
                else
                {
                    return new LoginResult { Success = false };
                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("...", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}