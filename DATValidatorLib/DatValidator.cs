using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DATValidatorLib
{
    public class DatValidator : IDatValidator
    {
        /// <summary>
        /// Subscribe to this event to get a progress of each line in a given DAT file.
        /// </summary>
        public event ValidationProgressEventHandler OnValidateProgress;
        /// <summary>
        /// Subscribe to this event to get a status.
        /// </summary>
        public event ValidationStatusEventHandler OnValidateStatus;
        /// <summary>
        /// Subscribe to this event to get a completion message and overall status of the process.
        /// </summary>
        public event ValidationCompleteEventHandler OnValidateComplete;
        /// <summary>
        /// Subscribe to this event to get notified of any errors.
        /// </summary>
        public event ValidationErrorEventHandler OnValidateError;

        // Delegates for event handlers
        public delegate void ValidationProgressEventHandler(object sender, ValidationProgressEventArgs e);
        public delegate void ValidationStatusEventHandler(object sender, ValidationStatusEventArgs e);
        public delegate void ValidationCompleteEventHandler(object sender, ValidationCompleteEventArgs e);
        public delegate void ValidationErrorEventHandler(object sender, ValidationErrorEventArgs e);
        /// <summary>
        /// Set this property to the background worker object if a background worker is used
        /// to run the validation process.
        /// </summary>
        public BackgroundWorker Worker { set { worker = value; } }
        /// <summary>
        /// The Header property contains the header of a given DAT file.
        /// </summary>
        public string Header { get { return header; } internal set { header = value; } }
        /// <summary>
        /// The HeaderColumns property contains a string array containing each header.
        /// </summary>
        public string[] HeaderColumns { get { return headerColumns; } internal set { headerColumns = value; } }
        /// <summary>
        /// The NumColumns property contains the number of columns as dictated by the header row.
        /// </summary>
        public int NumColumns { get { return numColumns; } internal set { numColumns = value; } }
        /// <summary>
        /// The ValidationStatus property contains the overall status of the process. See the ValidationStatus 
        /// enum for the different statuses.
        /// </summary>
        public ValidationStatus ValidationStatus { get { return status; } internal set { status = value; } }
        /// <summary>
        /// The DatFile property contains the fully qualified path to the DAT file to be validated.
        /// </summary>
        public string DatFile
        {
            get { return datfilePath; }
            set { datfilePath = value; }
        }
        private BackgroundWorker worker;
        private string datfilePath;
        private string header;
        private string[] headerColumns;
        private int numColumns;
        private ValidationStatus status;

        const char delimiter = (char)Constants.ASCII020;
        const char text_qualifier = (char)Constants.ASCII254;

        #region Methods
        /// <summary>
        /// Call the Validate method to start the validation process. Ensure that the proper event
        /// handlers are set to get feedback on the process.
        /// </summary>
        public void Validate()
        {
            status = ValidationStatus.PASSED;
            string outputFilename = Path.GetFileNameWithoutExtension(datfilePath);
            string outputDir = Path.GetDirectoryName(datfilePath);
            try
            {
                if (worker.CancellationPending)
                {
                    status = ValidationStatus.CANCELLED;
                    OnRaiseValidationStatusEvent(new ValidationStatusEventArgs("Cancel requested."));
                    return;
                }
                var lineCount = File.ReadLines(datfilePath).Count();
                using (var sr = new StreamReader(datfilePath))
                {
                    if (!worker.CancellationPending)
                    {
                        // Read header and count
                        OnRaiseValidationStatusEvent(new ValidationStatusEventArgs("Starting process"));
                        header = sr.ReadLine();
                        headerColumns = header.Split(delimiter);
                        numColumns = headerColumns.Count();
                        //StatusDelegateCallBack("1 " + header, MessageType.INFO);
                        // Loop through rest of file
                        for (int i = 2; i <= lineCount; ++i)
                        {
                            OnRaiseProgressEvent(new ValidationProgressEventArgs(i));
                            if (worker.CancellationPending)
                            {
                                status = ValidationStatus.CANCELLED;
                                OnRaiseValidationStatusEvent(new ValidationStatusEventArgs("Cancel requested."));
                                break;
                            }
                            string line = sr.ReadLine();
                            //StatusDelegateCallBack(i.ToString() + " " + line, MessageType.INFO);
                            var fields = line.Split((char)Constants.ASCII020);

                            // Check number of fields
                            if (numColumns != fields.Count())
                            {
                                status = ValidationStatus.FAILED;
                                OnRaiseValidationErrorEvent(new ValidationErrorEventArgs(line));
                                OnRaiseValidationErrorEvent(new ValidationErrorEventArgs("Line number " + i.ToString() + " has wrong field count"));
                            }
                            //Check delimiters                        
                            if (DelimitersOK(fields) == false)
                            {
                                status = ValidationStatus.FAILED;
                                OnRaiseValidationErrorEvent(new ValidationErrorEventArgs(line));
                                OnRaiseValidationErrorEvent(new ValidationErrorEventArgs("Line number " + i.ToString() + " has mismatch text qualifier"));
                            }
                        }
                    }
                    else
                    {
                        status = ValidationStatus.CANCELLED;
                        OnRaiseValidationStatusEvent(new ValidationStatusEventArgs("Cancel requested."));
                    }
                }
            }
            catch (Exception ex)
            {
                status = ValidationStatus.FAILED;
                OnRaiseValidationErrorEvent(new ValidationErrorEventArgs(ex.Message, ex));
                throw;
            }

            OnRaiseValidationCompleteEvent(new ValidationCompleteEventArgs("Validation Complete",ValidationStatus.PASSED));
        }

        /// <summary>
        /// Checks if each value in a given array are correctly delimited. 
        /// </summary>
        /// <param name="fields">An array of qualified text values</param>
        /// <returns>Returns true if every value is correctly delimited. Otherwise, false is returned.</returns>
        private bool DelimitersOK(string[] values)
        {
            string regex_pattern = @"^" + text_qualifier + ".*" + text_qualifier + "$";
            Regex rg = new Regex(regex_pattern);
            bool match = false;
            foreach (var value in values)
            {
                match = rg.IsMatch(value);
                if (!match)
                {
                    continue;
                }
            }
            if (match)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region Raise Events
        private void OnRaiseProgressEvent(ValidationProgressEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            ValidationProgressEventHandler raiseEvent = OnValidateProgress;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                // e.Message += $" at {DateTime.Now}";

                // Call to raise the event.
                raiseEvent(this, e);
            }
        }

        private void OnRaiseValidationCompleteEvent(ValidationCompleteEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            ValidationCompleteEventHandler raiseEvent = OnValidateComplete;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                // e.Message += $" at {DateTime.Now}";

                // Call to raise the event.
                raiseEvent(this, e);
            }
        }

        private void OnRaiseValidationErrorEvent(ValidationErrorEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            ValidationErrorEventHandler raiseEvent = OnValidateError;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                // e.Message += $" at {DateTime.Now}";

                // Call to raise the event.
                raiseEvent(this, e);
            }
        }

        private void OnRaiseValidationStatusEvent(ValidationStatusEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            ValidationStatusEventHandler raiseEvent = OnValidateStatus;

            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Format the string to send inside the CustomEventArgs parameter
                // e.Message += $" at {DateTime.Now}";

                // Call to raise the event.
                raiseEvent(this, e);
            }
        }
        #endregion
    }
}
