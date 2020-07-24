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
    /// <summary>
    /// Defines a delegate that is used for a callback from within the TestData class
    /// </summary>
    /// <param name="message"></param>
    public delegate void CompleteDelegate(string message);

    public delegate void StatusDelegate(string message, MessageType messageType);

    public delegate void LoggingDelegate(string message, MessageType messageType);
    public class DatValidator : IDatValidator
    {
        public CompleteDelegate CompleteDelegateCallBack { get; set; }
        public StatusDelegate StatusDelegateCallBack { get; set; }
        public LoggingDelegate LoggingDelegateCallback { get; set; }
        public BackgroundWorker Worker { set { worker = value; } }
        public string Header { get { return header; } internal set { header = value; } }
        public string[] HeaderColumns { get { return headerColumns; } internal set { headerColumns = value; } }
        public int NumColumns { get { return numColumns; } internal set { numColumns = value; } }
        public string DatFile { 
            get { return datfilePath; }
            set { datfilePath = value; }
        }

        private BackgroundWorker worker;
        private string datfilePath;
        private string header;
        private string[] headerColumns;
        private int numColumns;

        const char delimiter = (char)Constants.ASCII020;
        const char text_qualifier = (char)Constants.ASCII254;

        public void Validate()
        {
            string outputFilename = Path.GetFileNameWithoutExtension(datfilePath);
            string outputDir = Path.GetDirectoryName(datfilePath);
            try
            {
                if (worker.CancellationPending)
                {
                    StatusDelegateCallBack("Cancel requested.", MessageType.INFO);
                    return;
                }

                var lineCount = File.ReadLines(datfilePath).Count();
                using (var sr = new StreamReader(datfilePath))
                {
                    if (!worker.CancellationPending)
                    {
                        // Read header and count
                        StatusDelegateCallBack("Starting process", MessageType.INFO);
                        header = sr.ReadLine();
                        headerColumns = header.Split(delimiter);
                        numColumns = headerColumns.Count();
                        //StatusDelegateCallBack("1 " + header, MessageType.INFO);
                        // Loop through rest of file
                        for (int i = 2; i <= lineCount; ++i)
                        {

                            if (worker.CancellationPending)
                            {
                                StatusDelegateCallBack("Cancel requested.", MessageType.INFO);
                                break;
                            }
                            string line = sr.ReadLine();
                            //StatusDelegateCallBack(i.ToString() + " " + line, MessageType.INFO);
                            var fields = line.Split((char)Constants.ASCII020);

                            // Check number of fields
                            if (numColumns != fields.Count())
                            {
                                LoggingDelegateCallback(line, MessageType.ERROR);
                                LoggingDelegateCallback("Line number " + i.ToString() + " has wrong field count", MessageType.ERROR);
                                StatusDelegateCallBack(line, MessageType.INFO);
                                StatusDelegateCallBack("Line number " + i.ToString() + " has wrong field count", MessageType.ERROR);
                            }

                            //Check delimiters                        
                            if (DelimitersOK(fields) == false)
                            {
                                LoggingDelegateCallback(line, MessageType.ERROR);
                                LoggingDelegateCallback("Line number " + i.ToString() + " has mismatch text qualifier", MessageType.ERROR);
                                StatusDelegateCallBack(line, MessageType.INFO);
                                StatusDelegateCallBack("Line number " + i.ToString() + " has mismatch text qualifier", MessageType.ERROR);
                            }

                        }
                    }
                    else
                    {
                        StatusDelegateCallBack("Cancel requested.", MessageType.INFO);
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingDelegateCallback(ex.Message, MessageType.ERROR);
                throw;
            }
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
    }


}
