using System.IO;

namespace DATValidatorLib
{
    /// <summary>
    /// Arguments for the ValidationProgressEvent event handler
    /// </summary>
    public class ValidationProgressEventArgs
    {
        public double CurrentFileProgress { get; set; }
        public ValidationProgressEventArgs(double fileprogress)
        {
            CurrentFileProgress = fileprogress;
        }
    }
}