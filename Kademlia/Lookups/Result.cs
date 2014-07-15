using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PSOK.Kademlia.Lookups
{
    /// <summary>
    /// A search result in Kademlia.
    /// It can either have a list of <see cref="IContact" />s or
    /// contact information on the <see cref="INode"/>s which have the value.
    /// </summary>
    [DataContract, Serializable]
    internal class Result : IResult
    {
        /// <summary>
        /// Constructs a new <see cref="Result" />.
        /// </summary>
        public Result()
        {
        }

        /// <summary>
        /// Constructor specifying that the <see cref="Result" /> should represent a list of <see cref="IItem" />s.
        /// </summary>
        /// <param name="entries"></param>
        public Result(IEnumerable<IItem> entries)
        {
            Entries = entries;
        }

        /// <summary>
        /// Constructor specifying that the <see cref="Result" /> should represent a list of <see cref="IContact" />s.
        /// </summary>
        /// <param name="contacts">The contact information of <see cref="INode"/>s.</param>
        public Result(IEnumerable<IContact> contacts)
        {
            if (contacts == null)
                throw new ArgumentNullException("contacts");

            Contacts = contacts;
        }

        /// <summary>
        /// The list of <see cref="IContact" /> which the <see cref="Result" /> represents.
        /// </summary>
        [DataMember]
        public IEnumerable<IContact> Contacts { get; set; }

        /// <summary>
        /// Contact information on the <see cref="INode"/>s which have the value.
        /// </summary>
        [DataMember]
        public IEnumerable<IItem> Entries { get; set; }
    }
}