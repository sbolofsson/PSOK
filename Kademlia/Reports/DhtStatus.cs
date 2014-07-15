using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PSOK.Kademlia.Reports
{
    /// <summary>
    /// Indicates the status of an <see cref="NodeDht" />.
    /// </summary>
    [DataContract, Serializable]
    internal class DhtStatus : INodeDhtStatus
    {
        /// <summary>
        /// Indicates the size of the <see cref="Node" />'s part of the <see cref="NodeDht"/>.
        /// </summary>
        [DataMember]
        public int Size { get; set; }

        /// <summary>
        /// A collection of statuses of the items in the <see cref="NodeDht"/>.
        /// </summary>
        [DataMember]
        public IEnumerable<IItemStatus> Items { get; set; }
    }
}
