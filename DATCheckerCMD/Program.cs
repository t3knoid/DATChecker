using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoggerLib;

namespace DATCheckerCMD
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger Log = new Logger();
            Log.Info("Starting");

            try
            {
                var parser = new CommandLine();
                parser.Parse(args);

                var datfile = new Datfile();

                if (parser.Arguments.Count > 0)
                {
                    // get test data
                    if (parser.Arguments.ContainsKey("f"))
                    {
                        datfile.FilePath = parser.Arguments["f"][0];
                    };

                    // If any of the parameter is not specified exit
                    if (String.IsNullOrWhiteSpace(datfile.FilePath))
                    {
                        Usage();
                        Environment.Exit(1);
                    };

                    // Start checking here
                    try
                    {
                        datfile.Check();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
                else
                {
                    Usage();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error checking file.", ex);
            }
        }

        /// <summary>
        /// Show usage message in the console
        /// </summary>
        static void Usage()
        {
            Console.WriteLine("usage: DATCheckerCMD -f datfile");
        }
    }
}
