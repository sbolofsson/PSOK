using System.Collections.Generic;

namespace PSOK.PublishSubscribe.Reports
{
    /// <summary>
    /// Indicates the status of an <see cref="IPeer" />.
    /// </summary>
    public interface IBrokerStatus
    {
        /// <summary>
        /// Indicates if the <see cref="IPeer" /> is fully initialized.
        /// </summary>
        bool IsBrokerInitialized { get; }

        /// <summary>
        /// The number of <see cref="ISubscription" />s owned by the <see cref="IPeer" />.
        /// </summary>
        int SubscriptionCount { get; }

        /// <summary>
        /// The status of the <see cref="IPeer" />'s associated <see cref="ISubscription" />s.
        /// </summary>
        IEnumerable<ISubscriptionStatus> SubscriptionStatuses { get; } 

        /// <summary>
        /// Resets the status object.
        /// </summary>
        void Reset();
    }
}