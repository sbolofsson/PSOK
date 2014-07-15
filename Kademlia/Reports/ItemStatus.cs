using System;
using System.Runtime.Serialization;

namespace PSOK.Kademlia.Reports
{
    /// <summary>
    /// Indicates the status of an entry in the <see cref="NodeDht"/>.
    /// </summary>
    [DataContract, Serializable]
    internal class ItemStatus : IItemStatus
    {
        /// <summary>
        /// The key of an entry in the <see cref="NodeDht"/>.
        /// </summary>
        [DataMember]
        public string Key { get; set; }
    }
}
