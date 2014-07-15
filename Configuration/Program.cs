using System;
using System.Collections.Generic;
using System.Linq;
using PSOK.Kernel.Exceptions;
using PSOK.Kernel.Services;
using PSOK.Kernel.Web;

namespace PSOK.Configuration
{
    /// <summary>
    /// The entry point of the program
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main method of the program.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            try
            {
                ProcessInput(args);
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex);
                PrintUsage();
            }
        }

        /// <summary>
        /// Processes the input parameters.
        /// </summary>
        /// <param name="args"></param>
        private static void ProcessInput(IList<string> args)
        {
            if (args == null || !args.Any())
                throw new ConfigurationException("No input parameters were specified.");

            IDictionary<string, string> param = args
                .Where(x => x.Contains("="))
                .Select(arg => arg.Split('='))
                .ToDictionary(x => x[0].Replace("-", string.Empty).ToLower(), x => x[1]);

            SiteInformation siteInformation = SiteInformation.CreateFromParams(param);

            switch (args[0])
            {
                case "-AssignCertificateToSite":
                    IisHelper.AssignCertificateToSite(siteInformation);
                    break;
                case "-UnassignCertificateFromSite":
                    IisHelper.UnassignCertificateFromSite(siteInformation);
                    break;
                case "-InstallNetTcpPortSharingConfig":
                    NetTcpPortSharing.InstallNetTcpPortSharingConfig(param["path"]);
                    break;
                case "-UninstallNetTcpPortSharingConfig":
                    NetTcpPortSharing.UninstallNetTcpPortSharingConfig();
                    break;
                default:
                    throw new ConfigurationException("Could not determine action based on input parameters.");
            }
        }

        /// <summary>
        /// Prints the usage of the <see cref="Program" />
        /// </summary>
        private static void PrintUsage()
        {
            string siteInformationParameters = SiteInformation.GetPropertiesAsArgumentList();

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine();

            Console.WriteLine("-AssignCertificateToSite {0}", siteInformationParameters);
            Console.WriteLine();

            Console.WriteLine("-UnassignCertificateFromSite {0}", siteInformationParameters);
            Console.WriteLine();

            Console.WriteLine("-InstallNetTcpPortSharingConfig -Path=[Path]");
            Console.WriteLine();

            Console.WriteLine("-UninstallNetTcpPortSharingConfig");
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Examples:");
            Console.WriteLine();

            Console.WriteLine(
                "Kademlia.Configuration.exe -AssignCertificateToSite -SiteName=Kademlia.Web -Protocol=HTTPS -Dns=*:443:debug.kademlia -CertificateHash=e9d6e97167c01c5caf55bf81fe68dc1c80fb573a -CertificateStore=My");
            Console.WriteLine();

            Console.WriteLine(
                "Kademlia.Configuration.exe -UnassignCertificateFromSite -SiteName=Kademlia.Web -Protocol=HTTPS -Dns=*:443:debug.kademlia");
            Console.WriteLine();

            Console.WriteLine("Kademlia.Configuration.exe -Path=\"C:\\MyFolder\\SMSvcHost.exe.config\"");
            Console.WriteLine();
            Console.WriteLine();
        }
    }
}