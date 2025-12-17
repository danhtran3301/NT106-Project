using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace TimeFlow
{
    class SessionManager
    {
        public static string Username { get; set; }
        public static string Email { get; set; }
        public static string Token { get; set; }

        public static void SetUserSession(string username, string email, string token)
        {
            Username = username;
            Email = email;
            Token = token;
        }

        public static void ClearSession()
        {
            Username = null;
            Email = null;
            Token = null;
        }
    }
}
