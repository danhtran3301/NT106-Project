using System;

namespace TimeFlow.Data.Exceptions
{
    /// <summary>
    /// Exception nem khi data validation that bai
    /// </summary>
    public class ValidationException : Exception
    {
        public string Field { get; }
        public object? InvalidValue { get; }

        public ValidationException(string message) : base(message)
        {
            Field = string.Empty;
        }

        public ValidationException(string field, string message) : base(message)
        {
            Field = field;
        }

        public ValidationException(string field, object? invalidValue, string message) : base(message)
        {
            Field = field;
            InvalidValue = invalidValue;
        }

        public ValidationException(string message, Exception innerException) 
            : base(message, innerException)
        {
            Field = string.Empty;
        }
    }
}
