using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DATValidatorLib;
using LoggerLib;

namespace DATCheckerCMD
{
    public class Datfile
    {
        Logger Log = new Logger();
        public string FilePath { set { filePath = value; } get { return filePath; } }
        private string filePath;
        public Datfile()
        { }

        public void Check()
        {
            Log.Info("Validating " + filePath);
            DatValidator datValidator = new DatValidator
            {
                DatFile = filePath,
            };
            datValidator.OnValidateStatus += OnValidateStatus;
            datValidator.OnValidateError += OnValidateError;
            datValidator.OnValidateComplete += OnValidateComplete;
            datValidator.Validate();
        }

        private void OnValidateComplete(object sender, ValidationCompleteEventArgs e)
        {
            switch (e.Status)
            {
                case ValidationStatus.FAILED:
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(e.Message + " One or more errors detected." + Environment.NewLine);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    break;
                case ValidationStatus.CANCELLED:
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(e.Message + Environment.NewLine);
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }
                case ValidationStatus.PASSED:
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(e.Message + Environment.NewLine);
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    }
            }
        }
        private void OnValidateError(object sender, ValidationErrorEventArgs e)
        {  
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(e.Message);
            Console.WriteLine(e.Ex.StackTrace);
            Console.ForegroundColor = ConsoleColor.White;
        }

        private void OnValidateStatus(object sender, ValidationStatusEventArgs e)
        {
            Console.WriteLine(e.Message);
        }
    }
}
