using System;
using System.Collections.Generic;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Reports;
using log4net;

namespace PSOK.PublishSubscribe.Services
{
    /// <summary>
    /// Service host for debugging operations.
    /// </summary>
    public class DebugServiceHost : ServiceHostBase, IDebugServiceHost
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(DebugServiceHost));
        private static string _url;

        /// <summary>
        /// Creates a new <see cref="DebugServiceHost"/> at the specified url.
        /// </summary>
        /// <param name="url"></param>
        public DebugServiceHost(string url)
        {
            _url = url;
        }
        
        /// <summary>
        /// Initializes the <see cref="DebugServiceHost" />.
        /// </summary>
        public override void Initialize()
        {
            Initialize("debug", _url, typeof(IDebugServiceHost));
        }

        /// <summary>
        /// Delivers an <see cref="IStatusReport" />.
        /// </summary>
        /// <param name="statusReport"></param>
        public void Deliver(IStatusReport statusReport)
        {
            if (statusReport == null)
                throw new ArgumentNullException("statusReport");

            try
            {
                statusReport.StoreLocally();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Retrives all the <see cref="IStatusReport" />s delivered to this service.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IStatusReport> GetReports()
        {
            try
            {
                return StatusReport.GetLocalReports();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return null;
        }
    }
}