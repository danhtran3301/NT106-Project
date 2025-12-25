using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TimeFlow
{
    public class SessionManager
    {
        
        public static int? CurrentUserId { get; set; }
        public static string Username { get; set; }
        public static string Email { get; set; }
        public static string Token { get; set; }
        public static TcpClient TcpClient { get; set; }

        public static void SetUserSession(int userId, string username, string email, string token)
        {
            CurrentUserId = userId;
            Username = username;
            Email = email;
            Token = token;
        }

        public static void SetUserSession(string username, string email, string token)
        {
            Username = username;
            Email = email;
            Token = token;
        }

        public static void SetTcpClient(TcpClient client)
        {
            TcpClient = client;
        }

        public static void ClearSession()
        {
            CurrentUserId = 0; // Reset về 0
            Username = null;
            Email = null;
            Token = null;

            if (TcpClient != null)
            {
                try
                {
                    TcpClient.Close();
                }
                catch { }
                TcpClient = null;
            }
        }

        public static bool IsAuthenticated => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Token);
    }
}