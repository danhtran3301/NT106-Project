using System;
using System.Windows.Forms;
using TimeFlow.UI;
using TimeFlow.Server;
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

            Application.Run(new FormDangNhap());
        }
    }
}
