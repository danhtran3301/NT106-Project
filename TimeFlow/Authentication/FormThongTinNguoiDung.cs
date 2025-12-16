using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TimeFlow.Authentication
{
    public partial class FormThongTinNguoiDung : Form
    {
        private readonly string username;
        private readonly string email;
        public FormThongTinNguoiDung(string username, string email)
        {
            InitializeComponent();
            this.username = username;
            this.email = email;
        }
        private void FormThongTinNguoiDung_Load(object sender, EventArgs e)
        {
            labelUsernameInfo.Text = "Username: " + username;
            labelEmailInfo.Text = "Email: " + email;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                // Xóa token nếu tồn tại
                string tokenPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\token.jwt");
                if (File.Exists(tokenPath))
                {
                    File.Delete(tokenPath);
                }

                this.Hide();
                FormDangNhap loginForm = new FormDangNhap();
                loginForm.FormClosed += (s, args) => this.Close();
                loginForm.Show();
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Bạn có chắc chắn muốn thoát ứng dụng?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}
