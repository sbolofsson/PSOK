using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Reports
{
    /// <summary>
    /// Indicates the status of an <see cref="Peer" />.
    /// </summary>
    [DataContract, Serializable]
    internal class BrokerStatus : IBrokerStatus
    {
        /// <summary>
        /// Indicates if the <see cref="IPeer" /> is fully initialized.
        /// </summary>
        [DataMember]
        public bool IsBrokerInitialized { get; set; }

        /// <summary>
        /// The number of <see cref="ISubscription" />s owned by the <see cref="IPeer" />.
        /// </summary>
        [DataMember]
        public int SubscriptionCount { get; set; }

        /// <summary>
        /// The status of the <see cref="Peer" />'s associated <see cref="ISubscription" />s.
        /// </summary>
        [DataMember]
        public IEnumerable<ISubscriptionStatus> SubscriptionStatuses { get; set; } 

        /// <summary>
        /// Resets the status object.
        /// </summary>
        public void Reset()
        {
            IsBrokerInitialized = false;
            SubscriptionCount = 0;
            SubscriptionStatuses = null;
        }
    }
}