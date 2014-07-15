using System;

namespace PSOK.Kademlia
{
    /// <summary>
    /// A set of contact information.
    /// </summary>
    public interface IContact : ICloneable
    {
        /// <summary>
        /// The unique id of the <see cref="IContact" />.
        /// </summary>
        string NodeId { get; }

        /// <summary>
        /// The base URL of the <see cref="IContact" />.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// The public node URL of the <see cref="IContact" />.
        /// </summary>
        string NodeUrl { get; }

        /// <summary>
        /// Extra information associated with the <see cref="IContact"/>.
        /// </summary>
        object Data { get; set; }

        /// <summary>
        /// Clones the <see cref="IContact"/>.
        /// </summary>
        /// <returns></returns>
        IContact Copy();
    }
}