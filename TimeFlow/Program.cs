using System;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;
using TimeFlow.UI;

namespace TimeFlow
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Ch?y FormGiaoDien làm main dashboard
            Application.Run(new FormTaskDetail());
            
            // Other entry points (comment out):
            // Application.Run(new ChatForm());           // Chat interface
            // Application.Run(new FormDangNhap());       // Login
            // Application.Run(new FormMenuTCP());        // TCP Menu
        }
    }
}