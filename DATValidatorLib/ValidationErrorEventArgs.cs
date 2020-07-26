using System;

namespace DATValidatorLib
{
    /// <summary>
    /// Arguments for the ValidationErrorEvent event handler
    /// </summary>
    public class ValidationErrorEventArgs
    {
        public string Message { get; set; }
        public Exception Ex { get; set; }
        public ValidationErrorEventArgs(string message, Exception ex)
        {
            Message = message;
            Ex = ex;
        }

        public ValidationErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}