using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using PSOK.Kademlia;
using PSOK.Kademlia.Reports;
using PSOK.Kernel;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Environment;
using PSOK.Kernel.Events;
using PSOK.Kernel.Reflection;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Services;
using log4net;
using Formatter = PSOK.Kernel.Encoding.Formatter;

namespace PSOK.PublishSubscribe.Reports
{
    /// <summary>
    /// Defines a status summary of an application in the P2P network.
    /// </summary>
    [DataContract, Serializable]
    public class StatusReport : IStatusReport
    {
        // Static fields
        [IgnoreDataMember, NonSerialized]
        private static readonly ConcurrentDictionary<string, byte> ReportIds =
            new ConcurrentDictionary<string, byte>();

        [IgnoreDataMember, NonSerialized]
        private static readonly ILog Log = LogManager.GetLogger(typeof(StatusReport));

        #region IStatusReport properties

        /// <summary>
        /// The unqiue id of the application.
        /// </summary>
        [DataMember]
        public string AppId { get; set; }

        /// <summary>
        /// The name of the application.
        /// </summary>
        [DataMember]
        public string AppName { get; set; }

        /// <summary>
        /// The overall status of the application.
        /// </summary>
        [DataMember]
        public AppStatus AppStatus { get; set; }

        /// <summary>
        /// The amount of memory used by the application in bytes.
        /// </summary>
        [DataMember]
        public long MemoryUsed { get; set; }

        /// <summary>
        /// The number of cache entries.
        /// </summary>
        [DataMember]
        public int CacheEntries { get; set; }

        /// <summary>
        /// The last time the <see cref="StatusReport" /> was updated.
        /// </summary>
        [DataMember]
        public DateTime Updated { get; set; }

        /// <summary>
        /// A collection of <see cref="IQueueStatus" />es indicating the status of all of the event queues in the application.
        /// </summary>
        [DataMember]
        public IEnumerable<IQueueStatus> QueueStatuses { get; set; }

        /// <summary>
        /// A collection of <see cref="IBrokerStatus" />es indicating the status of all of the <see cref="IBroker" />s in the
        /// application.
        /// </summary>
        [DataMember]
        public IEnumerable<IBrokerStatus> BrokerStatuses { get; set; }

        /// <summary>
        /// A collection of <see cref="INodeStatus" />es indicating the status of all of the <see cref="INode" />s in the
        /// application.
        /// </summary>
        [DataMember]
        public IEnumerable<INodeStatus> NodeStatuses { get; set; }

        #endregion

        #region IStatusReport methods

        /// <summary>
        /// Retrieves the header of the <see cref="IStatusReport" />.
        /// </summary>
        /// <returns></returns>
        public string GetHeader()
        {
            return string.Format("App: {0}, Memory used: {1}, Total items queued: {2}",
                AppName,
                Formatter.FormatAsDataSize(MemoryUsed),
                QueueStatuses != null ? QueueStatuses.Sum(x => x.QueueSize) : 0);
        }

        /// <summary>
        /// Retrieves all the information contained in the status report as a collection of <see cref="IStatusReportEntry" />
        /// objects.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IStatusReportEntry> GetEntries()
        {
            List<StatusReportEntry> entries =
                ObjectHelper.TraverseObject<object, StatusReport, StatusReportEntry>(this,
                    (currentObject, currentPropertyInfo, currentPropertyValue) => new StatusReportEntry
                    {
                        Description = string.Format("{0}.{1}", currentObject.GetType().Name, currentPropertyInfo.Name),
                        Value = currentPropertyValue != null ? currentPropertyValue.ToString() : "-"
                    }).ToList();

            return entries;
        }

        /// <summary>
        /// Resolves the status of the service hosts in the remote application.
        /// </summary>
        public void ResolveStatusRemotely()
        {
            /*List<AppStatus> statuses = new List<AppStatus>();
            foreach (INodeStatus nodeStatus in NodeStatuses)
            {
                bool isNodeAlive = true;
                IContact contact = nodeStatus.GetContact();

                try
                {
                    using (
                        ServiceProxy<IKademlia> peerProxy = new ServiceProxy<IKademlia>(contact.NodeUrl))
                    {
                        peerProxy.Context.Ping();
                    }
                }
                catch (Exception)
                {
                    nodeStatus.Reset();
                    isNodeAlive = false;
                }
                statuses.Add(isNodeAlive ? AppStatus.Running : AppStatus.Down);
            }

            SetAppStatus(statuses);
            */
        }

        /// <summary>
        /// Stores an <see cref="StatusReport" /> locally.
        /// </summary>
        public void StoreLocally()
        {
            Updated = DateTime.Now;

            string reportId = string.Format("statusreport#{0}", AppId);

            ReportIds[reportId] = new byte();

            CacheManager.Add(reportId, this, new CachingOptions<IStatusReport>
            {
                EnableCaching = true,
                Expiration = new TimeSpan(1, 0, 0),
                OnRemoved = x =>
                {
                    // Ignore callbacks caused by Remove and Set - we know that we only use Set
                    if (x == CacheEntryRemovedReason.Removed)
                        return;
                    byte b;
                    ReportIds.TryRemove(reportId, out b);
                }
            });
        }

        /// <summary>
        /// Stores an <see cref="StatusReport" /> remotely.
        /// </summary>
        public void StoreRemotely()
        {
            foreach (IContact bootstrapper in BootstrapperNode.GetContacts())
            {
                try
                {
                    using (
                        ServiceProxy<IDebugServiceHost> proxy =
                            new ServiceProxy<IDebugServiceHost>(bootstrapper.DebugUrl()))
                    {
                        proxy.Context.Deliver(this);
                    }
                }
                catch (Exception ex)
                {
                    if (!Kernel.Exceptions.Exception.IsEndpointDown(ex))
                        Log.Error(ex);
                }
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Sets the <see cref="StatusReport" />'s <see cref="AppStatus" /> based on a list of <see cref="AppStatus" />es
        /// representing
        /// the remote status of the associated service hosts.
        /// </summary>
        /// <param name="statuses"></param>
        private void SetAppStatus(List<AppStatus> statuses)
        {
            if (statuses == null)
                throw new ArgumentNullException("statuses");

            if (!statuses.Any() || statuses.All(x => x == AppStatus.Running))
            {
                AppStatus = AppStatus.Running;
            }
            else if (statuses.All(x => x == AppStatus.Down))
            {
                AppStatus = AppStatus.Down;
                QueueStatuses = null;
                CacheEntries = 0;
            }
            else
            {
                AppStatus = AppStatus.PartiallyRunning;
            }
        }

        /// <summary>
        /// Prints all entries of the <see cref="StatusReport" />.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetEntries().
                Aggregate(string.Empty,
                    (acc, entry) => string.Format("{0}{1}\n", acc, entry.ToString())).Trim();
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Retrieves all <see cref="IStatusReport" />s created by all applications currently active in the P2P network.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IStatusReport> GetRemoteReports()
        {
            IContact bootstrapper = BootstrapperNode.GetContacts().FirstOrDefault();
            if (bootstrapper == null)
                return new List<IStatusReport>();

            try
            {
                using (
                    ServiceProxy<IDebugServiceHost> proxy =
                        new ServiceProxy<IDebugServiceHost>(bootstrapper.DebugUrl()))
                {
                    return proxy.Context.GetReports();
                }
            }
            catch (Exception ex)
            {
                if (!Kernel.Exceptions.Exception.IsEndpointDown(ex))
                    Log.Error(ex);
            }
            return new List<IStatusReport>();
        }

        /// <summary>
        /// Retrieves all <see cref="IStatusReport" />s stored locally.
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<IStatusReport> GetLocalReports()
        {
            try
            {
                return ReportIds.Keys
                    .Select(x => CacheManager.Get<StatusReport>(x) as IStatusReport)
                    .Where(x => x != null).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return new List<IStatusReport>();
        }

        /// <summary>
        /// Creates an <see cref="IStatusReport" /> for the current application.
        /// </summary>
        /// <returns></returns>
        internal static IStatusReport Create()
        {
            StatusReport statusReport = new StatusReport
            {
                AppId = AssemblyHelper.GetEntryAssemblyId(),
                //AppId = KeyProvider.GetContact().NodeId,
                AppName = AssemblyHelper.GetEntryAssemblyName(),
                MemoryUsed = ProcessPerformance.GetBytesUsed(),
                BrokerStatuses = Peer.Peers.Select(x => x.GetStatus()).ToList(),
                NodeStatuses = Node.GetNodeStatuses(),
                CacheEntries = CacheManager.GetAllKeys().Count(),
                QueueStatuses = EventQueue.GetStatuses().ToList()
            };

            List<AppStatus> statuses = (statusReport.BrokerStatuses ?? new List<IBrokerStatus>())
                .Select(x => x.IsBrokerInitialized ? AppStatus.Running : AppStatus.Down).ToList()
                .Union((statusReport.NodeStatuses ?? new List<INodeStatus>())
                .Select(x => x.IsNodeInitialized ? AppStatus.Running : AppStatus.Down).ToList())
                .ToList();

            statusReport.SetAppStatus(statuses);

            return statusReport;
        }

        #endregion
    }
}