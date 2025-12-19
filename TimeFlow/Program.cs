using System;
using System.Windows.Forms;
using TimeFlow.UI;
using TimeFlow.Server;
using TimeFlow.Tasks;
using TimeFlow.Server;
using TimeFlow.Authentication;

namespace TimeFlow
{
    internal static class Program
    {
        private static ApplicationContext _appContext;
        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // ✅ FIX: Sử dụng ApplicationContext để quản lý app lifecycle
            _appContext = new ApplicationContext();
            ShowLoginForm();
            Application.Run(_appContext);
        }

        // ✅ Show login form và set làm main form
        public static void ShowLoginForm()
        {
            var loginForm = new FormDangNhap();
            
            // ✅ Khi login thành công, form sẽ tự đóng và mở FormGiaoDien
            loginForm.FormClosed += (s, e) =>
            {
                // Nếu không có form nào khác đang mở, thoát app
                if (Application.OpenForms.Count == 0)
                {
                    Application.Exit();
                }
            };
            
            loginForm.Show();
        }
    }
}
