using System.Collections.Generic;

namespace PSOK.Kademlia.Reports
{
    /// <summary>
    /// Indicates the status of an <see cref="INodeDht" />.
    /// </summary>
    public interface INodeDhtStatus
    {
        /// <summary>
        /// Indicates the size of the <see cref="INode" />'s part of the DHT.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// A collection of statuses of the items in the <see cref="INodeDht"/>.
        /// </summary>
        IEnumerable<IItemStatus> Items { get; }
    }
}
