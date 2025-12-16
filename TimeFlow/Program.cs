using System;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;
using TimeFlow.UI;
using TimeFlow.Server;
using TimeFlow.Tasks;
using TimeFlow.Server;
using TimeFlow.Authentication;

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
            Application.Run(new FormMenuTCP());

            Application.Run(new FormDangNhap());
        }
    }
}