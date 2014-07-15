using System;
using System.ComponentModel;
using System.Configuration.Install;
using PSOK.Kernel.Reflection;
using log4net;

namespace PSOK.Kernel.Environment
{
    /// <summary>
    /// Helper class for working with Windows services.
    /// </summary>
    public static class ServiceHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ServiceHelper));

        /// <summary>
        /// Installs the entry assembly of the current application as a Windows service.
        /// </summary>
        /// <returns></returns>
        public static bool InstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { AssemblyHelper.GetEntryAssembly().Location });
            }
            catch (Exception ex)
            {
                Win32Exception win32Exception = ex.InnerException as Win32Exception;
                if (win32Exception != null)
                {
                    Log.ErrorFormat("Error(0x{0:X}): Service is already installed.", win32Exception.ErrorCode);
                }

                Log.Error(ex);
                return false;
            }

            return false;
        }

        /// <summary>
        /// Uninstall the entry assembly of the current application as a Windows service.
        /// </summary>
        /// <returns></returns>
        public static bool UninstallService()
        {
            try
            {
                ManagedInstallerClass.InstallHelper(new[] { "/u", AssemblyHelper.GetEntryAssembly().Location });
            }
            catch (Exception ex)
            {
                Win32Exception win32Exception = ex.InnerException as Win32Exception;
                if (win32Exception != null)
                {
                    Log.ErrorFormat("Error(0x{0:X}): Service is not installed.", win32Exception.ErrorCode);
                }

                Log.Error(ex);
                return false;
            }

            return true;
        }
    }
}