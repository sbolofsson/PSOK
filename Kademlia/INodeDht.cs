using System.Collections.Generic;
using PSOK.Kademlia.Reports;

namespace PSOK.Kademlia
{
    /// <summary>
    /// Defines a DHT.
    /// All methods are thread safe.
    /// </summary>
    internal interface INodeDht
    {
        /// <summary>
        /// Adds some data using the specified key to the DHT.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        void Add(string key, params IItem[] items);

        /// <summary>
        /// Retrieves some data identified by the specified key from the DHT.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<IItem> Get(string key);

        /// <summary>
        /// Replicates all items in the DHT to Kademlia.
        /// </summary>
        void Replicate();

        /// <summary>
        /// Replicates all items in the DHT to the specified <see cref="IContact"/> if it is closer
        /// to any items than the owner of the DHT.
        /// </summary>
        /// <param name="contact"></param>
        void Replicate(IContact contact);

        /// <summary>
        /// Ensures the integrity of all items in the DHT.
        /// </summary>
        void EnsureIntegrity();

        /// <summary>
        /// Indicates the status of the DHT.
        /// </summary>
        /// <returns></returns>
        INodeDhtStatus GetStatus();
    }
}