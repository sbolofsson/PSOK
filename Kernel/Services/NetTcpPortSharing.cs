using System;
using System.Globalization;
using System.IO;
using Microsoft.Win32;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// A helper class for working with the NetTcpPortSharing service.
    /// </summary>
    public static class NetTcpPortSharing
    {
        /// <summary>
        /// Installs a NetTcpPortSharing config and backs up the original.
        /// </summary>
        /// <param name="newConfigFilePath"></param>
        public static void InstallNetTcpPortSharingConfig(string newConfigFilePath)
        {
            if (string.IsNullOrEmpty(newConfigFilePath))
                throw new ArgumentNullException("newConfigFilePath");

            string netTcpPortSharingServiceConfigPath = GetNetTcpPortSharingConfigFilePath();
            string netTcpPortSharingServiceConfigPathBackup = string.Format("{0}.bak",
                netTcpPortSharingServiceConfigPath);

            if (!File.Exists(netTcpPortSharingServiceConfigPathBackup))
                File.Copy(netTcpPortSharingServiceConfigPath, netTcpPortSharingServiceConfigPathBackup);

            string newConfigFileXml = File.ReadAllText(newConfigFilePath);

            newConfigFileXml = newConfigFileXml
                .Replace("#listenBacklog#", Binding.GetListenBacklog().ToString(CultureInfo.InvariantCulture))
                .Replace("#maxPendingConnections#",
                    Binding.GetMaxPendingConnections().ToString(CultureInfo.InvariantCulture))
                .Replace("#maxPendingAccepts#", Binding.GetMaxPendingAccepts().ToString(CultureInfo.InvariantCulture))
                .Replace("#receiveTimeout#", Binding.GetDefaultTimeout().ToString());

            File.WriteAllText(netTcpPortSharingServiceConfigPath, newConfigFileXml, System.Text.Encoding.UTF8);
        }

        /// <summary>
        /// Uninstalls a previously installed NetTcpPortSharing config file and restores the original.
        /// </summary>
        public static void UninstallNetTcpPortSharingConfig()
        {
            string netTcpPortSharingServiceConfigPath = GetNetTcpPortSharingConfigFilePath();
            string netTcpPortSharingServiceConfigPathBackup = string.Format("{0}.bak",
                netTcpPortSharingServiceConfigPath);

            if (File.Exists(netTcpPortSharingServiceConfigPathBackup))
            {
                File.Delete(netTcpPortSharingServiceConfigPath);
                File.Move(netTcpPortSharingServiceConfigPathBackup, netTcpPortSharingServiceConfigPath);
            }
        }

        /// <summary>
        /// Gets the config file path used by the NetTcpPortSharing service.
        /// </summary>
        /// <returns></returns>
        private static string GetNetTcpPortSharingConfigFilePath()
        {
            string netTcpPortSharingServiceExecutablePath = null;

            try
            {
                RegistryKey registryKey =
                    Registry.LocalMachine.OpenSubKey("System\\CurrentControlSet\\Services\\NetTcpPortSharing");
                if (registryKey != null)
                    netTcpPortSharingServiceExecutablePath = (string) registryKey.GetValue("ImagePath");
            }
            catch (Exception ex)
            {
                throw new Exceptions.BindingException("Could not read NetTcpPortSharing service information from the registry.", ex);
            }

            string netTcpPortSharingServiceConfigPath = string.Format("{0}.config",
                netTcpPortSharingServiceExecutablePath);

            if (!File.Exists(netTcpPortSharingServiceConfigPath))
                throw new FileNotFoundException("Could not find the NetTcpPortSharing service's configuration file.");

            return netTcpPortSharingServiceConfigPath;
        }
    }
}