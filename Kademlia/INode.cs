using System.Collections.Generic;
using PSOK.Kademlia.Reports;

namespace PSOK.Kademlia
{
    /// <summary>
    /// Represents a node in Kademlia.
    /// </summary>
    internal interface INode : IKademlia
    {
        /// <summary>
        /// Container of all the <see cref="IContact" />s which the <see cref="INode" /> knows.
        /// </summary>
        IBucketContainer BucketContainer { get; }

        /// <summary>
        /// The local part of the DHT.
        /// </summary>
        INodeDht Dht { get; }

        /// <summary>
        /// The <see cref="INode" />'s contact information.
        /// </summary>
        IContact Contact { get; }

        /// <summary>
        /// Iteratively stores in the DHT that the <see cref="INode"/>
        /// has the data for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        void IterativeStore(string key, object data);

        /// <summary>
        /// Iteratively tries to find a list of <see cref="IContact" />s in the DHT.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<IContact> IterativeFindNode(string key);

        /// <summary>
        /// Iteratively tries to find a value in the DHT.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<IContact> IterativeFindValue(string key);

        /// <summary>
        /// Informs the <see cref="INode" /> that an <see cref="IContact" /> is still active.
        /// </summary>
        /// <param name="contact"></param>
        void Inform(IContact contact);

        /// <summary>
        /// Pings the specified <see cref="IContact" />.
        /// </summary>
        /// <param name="contact"></param>
        void PingContact(IContact contact);

        /// <summary>
        /// Republishes all key value pairs.
        /// </summary>
        void Republish();

        /// <summary>
        /// Retrieves the status of the <see cref="INode" />.
        /// </summary>
        /// <returns></returns>
        INodeStatus GetStatus();
    }
}