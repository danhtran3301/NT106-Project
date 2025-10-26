using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;



namespace BT3_LTMCB
{
    public partial class FormDangKi : Form
    {
        public FormDangKi()
        {
            InitializeComponent();
        }
        private string SendRegisterRequest(string username, string password, string email)
        {
            var registerData = new
            {
                type = "register",
                data = new
                {
                    username = username,
                    password = password,
                    email = email
                }
            };

            string json = System.Text.Json.JsonSerializer.Serialize(registerData);
            byte[] sendBytes = Encoding.UTF8.GetBytes(json);

            using (TcpClient client = new TcpClient())
            {
                client.ReceiveTimeout = 3000;
                client.SendTimeout = 3000;

                client.Connect("127.0.0.1", 1010);
                using NetworkStream stream = client.GetStream();

                stream.Write(sendBytes, 0, sendBytes.Length);
                stream.Flush();

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            }
        }

        private void buttonSignup_Click(object sender, EventArgs e)
        {
            string username = usernameTxtbox.Text.Trim();
            string password = passwordTxtbox.Text.Trim();
            string confirmPassword = confirmTxtbox.Text.Trim();
            string email = emailTextbox.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(email))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Regex.IsMatch(username, @"^[a-zA-Z0-9]{5,15}$"))
            {
                MessageBox.Show("Tên đăng nhập phải từ 5-15 ký tự và không chứa ký tự đặc biệt.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!Regex.IsMatch(email, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
            {
                MessageBox.Show("Email không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$"))
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (password != confirmPassword)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Task.Run(() =>
            {
                try
                {
                    string response = SendRegisterRequest(username, password, email);

                    this.Invoke((MethodInvoker)delegate
                    {
                        if (response == "registered")
                        {
                            MessageBox.Show("Đăng ký thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            usernameTxtbox.Clear();
                            emailTextbox.Clear();
                            passwordTxtbox.Clear();
                            confirmTxtbox.Clear();

                            FormDangNhap DN = new FormDangNhap();
                            DN.Show();
                            this.Hide();
                        }
                        else if (response == "exists")
                        {
                            MessageBox.Show("Tên đăng nhập hoặc email đã tồn tại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Đăng ký thất bại: " + response, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    });
                }
                catch (Exception ex)
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        MessageBox.Show("Lỗi khi gửi request đăng ký: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    });
                }
            });
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            FormDangNhap login = new FormDangNhap();
            login.Show();
            this.Hide();
        }

    }
}
