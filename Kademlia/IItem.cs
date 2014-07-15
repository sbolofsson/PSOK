using System;

namespace PSOK.Kademlia
{
    /// <summary>
    /// An entry in the DHT.
    /// </summary>
    internal interface IItem
    {
        /// <summary>
        /// The time to live of the contact information.
        /// </summary>
        int TimeToLive { get; }

        /// <summary>
        /// Indicates the expiration time of the contact information.
        /// </summary>
        DateTime Expiration { get; }

        /// <summary>
        /// Indicates whether the contact information has expired.
        /// </summary>
        bool IsExpired { get; }

        /// <summary>
        /// The <see cref="IContact"/> of the <see cref="IItem"/>.
        /// Contains information about the <see cref="INode"/> which published the <see cref="IItem"/>.
        /// </summary>
        IContact Contact { get; }
    }
}
