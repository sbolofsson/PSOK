using System;
using System.Collections.Generic;
using PSOK.Kademlia;
using PSOK.Kademlia.Reports;
using PSOK.Kernel;
using PSOK.Kernel.Events;

namespace PSOK.PublishSubscribe.Reports
{
    /// <summary>
    /// Defines a status summary of an application in the P2P network.
    /// </summary>
    public interface IStatusReport
    {
        /// <summary>
        /// The unqiue id of the application.
        /// </summary>
        string AppId { get; }

        /// <summary>
        /// The name of the application.
        /// </summary>
        string AppName { get; }

        /// <summary>
        /// The overall status of the application.
        /// </summary>
        AppStatus AppStatus { get; set; }

        /// <summary>
        /// The amount of memory used by the application in bytes.
        /// </summary>
        long MemoryUsed { get; }

        /// <summary>
        /// The number of cache entries.
        /// </summary>
        int CacheEntries { get; }

        /// <summary>
        /// The last time the <see cref="IStatusReport" /> was updated.
        /// </summary>
        DateTime Updated { get; }

        /// <summary>
        /// A collection of <see cref="IQueueStatus" />es indicating the status of all of the event queues in the application.
        /// </summary>
        IEnumerable<IQueueStatus> QueueStatuses { get; }

        /// <summary>
        /// A collection of <see cref="IBrokerStatus" />es indicating the status of all of the <see cref="IPeer" />s in the
        /// application.
        /// </summary>
        IEnumerable<IBrokerStatus> BrokerStatuses { get; }

        /// <summary>
        /// A collection of <see cref="INodeStatus" />es indicating the status of all of the <see cref="INode" />s in the
        /// application.
        /// </summary>
        IEnumerable<INodeStatus> NodeStatuses { get; }

        /// <summary>
        /// Retrieves the header of the <see cref="IStatusReport" />.
        /// </summary>
        /// <returns></returns>
        string GetHeader();

        /// <summary>
        /// Retrieves all the information contained in the status report as a collection of <see cref="IStatusReportEntry" />
        /// objects.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IStatusReportEntry> GetEntries();

        /// <summary>
        /// Resolves the status of the service hosts in the remote application.
        /// </summary>
        void ResolveStatusRemotely();

        /// <summary>
        /// Stores an <see cref="IStatusReport" /> locally.
        /// </summary>
        void StoreLocally();

        /// <summary>
        /// Stores an <see cref="IStatusReport" /> remotely.
        /// </summary>
        void StoreRemotely();
    }
}