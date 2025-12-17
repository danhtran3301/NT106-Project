using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeFlow.Authentication;
using TimeFlow.UI.Components;

namespace TimeFlow.UI
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent(); // Phải gọi trước tiên

            // Sau đó mới set các properties sử dụng AppColors
            this.Text = "Application Settings";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppColors.Gray50;
            this.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);
            this.WindowState = FormWindowState.Maximized;
        }
        private void BtnLogout_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất không?",
                                         "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                // 1. Xóa thông tin Session
                //Models.UserToken.Clear();

                // 2. Đóng tất cả các form và quay lại Form Đăng nhập
                this.Hide();
                FormDangNhap loginForm = new FormDangNhap();
                loginForm.Show();

                
            }
        }

        // Thêm event handlers cho các buttons
        private void OnBackButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnSaveButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show("Settings saved successfully!", "Settings",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            this.Close();
        }

        private void OnDeleteAccountClick(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Are you sure you want to delete your account? This action cannot be undone.",
                "Delete Account",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                MessageBox.Show("Account deletion process initiated.", "Delete Account",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
