using System;
using System.Windows.Forms;
using BT3_LTMCB;
using Exercise3; // Ensure this is present and correct
using TimeFlow;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BT3_LTMCB
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
            Application.Run(new ChatForm());
        }
    }
}