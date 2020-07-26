namespace DATValidatorLib
{
    /// <summary>
    /// Arguments for the ValidationCompleteEvent event handler
    /// </summary>
    public class ValidationCompleteEventArgs
    {
        public string Message { get; set; }
        public ValidationStatus Status { get; set; }
        public ValidationCompleteEventArgs(string message, ValidationStatus status)
        {
            Message = message;
            Status = status;
        }
    }
}