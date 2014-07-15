using System;
using System.Diagnostics;
using log4net;

namespace PSOK.Kernel.Environment
{
    /// <summary>
    /// Helper class for determining the performance of the current process
    /// </summary>
    public static class ProcessPerformance
    {
        // Performance counter cache
        private static PerformanceCounter _performanceCounter;

        private static readonly ILog Log = LogManager.GetLogger(typeof (ProcessPerformance));

        /// <summary>
        /// Gets the amount of bytes used by the current process
        /// </summary>
        /// <returns></returns>
        public static long GetBytesUsed()
        {
            if (_performanceCounter != null)
                return Convert.ToInt64(_performanceCounter.NextValue());

            try
            {
                Process currentProcess = Process.GetCurrentProcess();
                _performanceCounter = new PerformanceCounter("Process", "Working Set - Private",
                    currentProcess.ProcessName);

                return Convert.ToInt64(_performanceCounter.NextValue());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return 0;
        }
    }
}