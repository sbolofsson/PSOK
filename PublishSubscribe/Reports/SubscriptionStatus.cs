using System;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Reports
{
    /// <summary>
    /// Indicates the status of a <see cref="Subscription{T}"/>.
    /// </summary>
    [DataContract, Serializable]
    internal class SubscriptionStatus : ISubscriptionStatus
    {
        /// <summary>
        /// The <see cref="Subscription{T}"/> key.
        /// </summary>
        [DataMember]
        public string Key { get; set; }

        /// <summary>
        /// The <see cref="Subscription{T}"/> type.
        /// </summary>
        [DataMember]
        public string Type { get; set; }
    }
}
