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
using System.IO;
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
            var confirm = MessageBox.Show(
                "Bạn có chắc chắn muốn đăng xuất không?\n\nBạn sẽ cần đăng nhập lại để sử dụng ứng dụng.",
                "Xác nhận đăng xuất", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                PerformLogout();
            }
        }

        // ✅ Main logout logic
        private void PerformLogout()
        {
            try
            {
                // 1. ✅ Clear session data (UserId, Username, Email, Token, TcpClient)
                SessionManager.ClearSession();

                // 2. ✅ Delete token file
                DeleteTokenFile();

                // 3. ✅ Close all open forms except login
                CloseAllForms();

                // 4. ✅ Show login form
                ShowLoginForm();

                // 5. ✅ Log activity (optional - if needed)
                LogLogoutActivity();

                MessageBox.Show(
                    "Đăng xuất thành công!\n\nHẹn gặp lại bạn!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Đã xảy ra lỗi khi đăng xuất: {ex.Message}",
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ✅ Delete token file
        private void DeleteTokenFile()
        {
            try
            {
                string tokenPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory, 
                    @"..\..\..\token.jwt"
                );

                if (File.Exists(tokenPath))
                {
                    File.Delete(tokenPath);
                    Console.WriteLine($"[Logout] Token file deleted: {tokenPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logout] Failed to delete token file: {ex.Message}");
                // Don't throw - token deletion is not critical
            }
        }

        // ✅ Close all forms except login
        private void CloseAllForms()
        {
            try
            {
                // Get all open forms
                var openForms = Application.OpenForms.Cast<Form>().ToList();

                // Close all forms except FormDangNhap (if exists)
                foreach (var form in openForms)
                {
                    if (form is not FormDangNhap)
                    {
                        // Close form to free resources
                        if (!form.IsDisposed)
                        {
                            form.Close();
                        }
                    }
                }

                Console.WriteLine($"[Logout] Closed {openForms.Count - 1} forms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logout] Error closing forms: {ex.Message}");
            }
        }

        // ✅ Show login form
        private void ShowLoginForm()
        {
            try
            {
                // ✅ FIX: Tạo form login mới, không kiểm tra form cũ
                FormDangNhap loginForm = new FormDangNhap();
                loginForm.Show();

                Console.WriteLine("[Logout] Login form shown");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logout] Error showing login form: {ex.Message}");
                
                // ✅ Nếu lỗi, vẫn cố gắng tạo form login
                try
                {
                    FormDangNhap fallbackForm = new FormDangNhap();
                    fallbackForm.Show();
                }
                catch
                {
                    MessageBox.Show("Không thể mở form đăng nhập. Vui lòng khởi động lại ứng dụng.", 
                        "Lỗi nghiêm trọng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ✅ Log logout activity (optional)
        private void LogLogoutActivity()
        {
            try
            {
                string logMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] User logged out: {SessionManager.Username ?? "Unknown"}";
                Console.WriteLine(logMessage);

                // Optional: Write to log file
                // string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "activity.log");
                // File.AppendAllText(logPath, logMessage + Environment.NewLine);
            }
            catch
            {
                // Ignore logging errors
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
                // TODO: Implement delete account API call
                MessageBox.Show("Account deletion process initiated.", "Delete Account",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
