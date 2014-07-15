using System.IO;
using PSOK.Kernel.Environment;

namespace PSOK.Kernel.Log4net
{
    /// <summary>
    /// RollingFileAppender which makes sure to use the bae directory of the app domain as the base directory for log files.
    /// Can be used in a Windows service application.
    /// </summary>
    public class RollingFileAppender : log4net.Appender.RollingFileAppender
    {
        /// <summary>
        /// Indicates the path to the log file.
        /// </summary>
        public override string File
        {
            set { base.File = Path.Combine(EnvironmentHelper.GetEnvironmentPath(), value); }
        }
    }
}