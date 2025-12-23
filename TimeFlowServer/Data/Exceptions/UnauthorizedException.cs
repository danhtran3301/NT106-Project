using System;

namespace TimeFlow.Data.Exceptions
{
    /// <summary>
    /// Exception nem khi user khong co quyen thuc hien hanh dong
    /// </summary>
    public class UnauthorizedException : Exception
    {
        public int UserId { get; }
        public string Action { get; }
        public int? ResourceId { get; }

        public UnauthorizedException(string message) : base(message)
        {
            Action = string.Empty;
        }

        public UnauthorizedException(int userId, string action, string message) : base(message)
        {
            UserId = userId;
            Action = action;
        }

        public UnauthorizedException(int userId, string action, int resourceId, string message) : base(message)
        {
            UserId = userId;
            Action = action;
            ResourceId = resourceId;
        }

        public UnauthorizedException(string message, Exception innerException) 
            : base(message, innerException)
        {
            Action = string.Empty;
        }
    }
}
