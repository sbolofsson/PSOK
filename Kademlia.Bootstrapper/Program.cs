using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using PSOK.Kademlia.Bootstrapper.Exceptions;
using PSOK.Kernel.Environment;
using log4net;

namespace PSOK.Kademlia.Bootstrapper
{
    internal static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static int Main(string[] args)
        {
            bool success = false;
            try
            {
                success = ProcessInput(args);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                PrintUsage();
                Log.Error(ex);
            }
            return success ? 0 : 1;
        }

        /// <summary>
        /// Processes the input parameters.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static bool ProcessInput(IList<string> args)
        {
            bool success = true;

            if (Environment.UserInteractive)
            {
                if (args == null || !args.Any())
                    throw new BootstrapperException("No input parameters were specified.");

                string arg = args.First().ToLower();

                switch (arg)
                {
                    case "/i":
                        success = ServiceHelper.InstallService();
                        break;

                    case "/u":
                        success = ServiceHelper.UninstallService();
                        break;

                    default:
                        throw new BootstrapperException("Could not determine action based on input parameters.");
                }
            }
            else
            {
                ServiceBase.Run(new BootstrapperService());
            }

            return success;
        }

        /// <summary>
        /// Prints the usage of the <see cref="Program" />
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();

            Console.WriteLine("/i \t Installs the Kademlia.Bootstrapper service.");
            Console.WriteLine();

            Console.WriteLine("/u \t Uninstalls the Kademlia.Bootstrapper service.");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}