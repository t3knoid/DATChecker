using System;

namespace DATValidatorLib
{
    /// <summary>
    /// Arguments for the ValidationStatusEvent event handler
    /// </summary>
    public class ValidationStatusEventArgs : EventArgs
    {
        public string Message { get; set; }
        public ValidationStatusEventArgs(string message)
        {
            Message = message;
        }
    }
}