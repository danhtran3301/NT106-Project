using System.Net.Sockets;
using TimeFlow.Models;

namespace TimeFlow
{
    public static class GlobalState
    {
        public static TcpClient Client { get; set; }
        public static User CurrentUser { get; set; }
    }
}