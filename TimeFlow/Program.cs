using System;
using System.Windows.Forms;
using TimeFlow.Authentication;
using TimeFlow.Tasks;

namespace TimeFlow
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. Chạy Form Đăng Nhập trước
            FormDangNhap loginForm = new FormDangNhap();

            // Nếu người dùng bấm Đăng nhập thành công (Code ở Bước 2 trả về DialogResult.OK)
            if (loginForm.ShowDialog() == DialogResult.OK)
            {
                // 2. Kiểm tra GlobalState (An toàn)
                if (GlobalState.CurrentUser != null)
                {
                    Application.Run(new FormGiaoDien());
                }
            }
            else
            {
                // Người dùng tắt form đăng nhập -> Thoát ứng dụng
                Application.Exit();
            }
        }
    }
}