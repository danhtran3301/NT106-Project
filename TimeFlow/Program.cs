using System;
using System.Windows.Forms;
using System.Globalization;
using System.Text.RegularExpressions;
using TimeFlow.UI;
<<<<<<< HEAD
using TimeFlow.Server;
=======
using TimeFlow.Tasks;
using TimeFlow.Server;
using TimeFlow.Authentication;
>>>>>>> 4b4f42b10bf4f52b062839463b4981cbca790977

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
<<<<<<< HEAD
            Application.Run(new FormTaskDetail());
           //Application.Run(new FormMenuTCP());
            //Application.Run(new FormDangNhap());
=======
<<<<<<< HEAD
            Application.Run(new FormMenuTCP());
=======
            Application.Run(new FormDangNhap());
>>>>>>> 4b4f42b10bf4f52b062839463b4981cbca790977
>>>>>>> 6fb3b932d2511cd06296fb71a104269de4194c30
        }
    }
}