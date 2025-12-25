using System;
using System.Windows.Forms;
using TimeFlow.Authentication;
using TimeFlow.Configuration;

namespace TimeFlow
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // ✅ Load cấu hình Server từ appsettings.json
            ServerConfig.Load();
            
            // ✅ FIX: Sử dụng ApplicationContext để quản lý app lifecycle
            _appContext = new ApplicationContext();
            ShowLoginForm();
            Application.Run(_appContext);
        }

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