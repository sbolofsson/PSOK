using System.Collections.Generic;

namespace PSOK.Kademlia.Lookups
{
    /// <summary>
    /// A search result in Kademlia.
    /// It can either have a list of <see cref="IContact" />s or
    /// contact information on the <see cref="INode"/>s which have the value.
    /// </summary>
    internal interface IResult
    {
        /// <summary>
        /// The list of <see cref="IContact" /> which the <see cref="IResult" /> represents.
        /// </summary>
        IEnumerable<IContact> Contacts { get; }

        /// <summary>
        /// Contact information on the <see cref="INode"/>s which have the value.
        /// </summary>
        IEnumerable<IItem> Entries { get; }
    }
}