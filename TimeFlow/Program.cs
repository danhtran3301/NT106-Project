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
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);      
            Application.Run(new FormDangNhap());

        }
    }
}
