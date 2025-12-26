using System;
using System.Windows.Forms;
using TimeFlow.UI;
using TimeFlow.Server;
using TimeFlow.Tasks;
using TimeFlow.Server;
using TimeFlow.Authentication;
using TimeFlow.Configuration;

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
            ServerConfig.Load();
            _appContext = new ApplicationContext();
            ShowLoginForm();
            Application.Run(_appContext);
        }
        public static void ShowLoginForm()
        {
            var loginForm = new FormDangNhap();
            
            loginForm.FormClosed += (s, e) =>
            {
                if (Application.OpenForms.Count == 0)
                {
                    Application.Exit();
                }
            };
            
            loginForm.Show();
        }
    }
}
