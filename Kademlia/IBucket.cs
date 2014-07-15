using System;
using System.Collections.Generic;
using PSOK.Kernel;

namespace PSOK.Kademlia
{
    /// <summary>
    /// A container of <see cref="IContact" />s.
    /// </summary>
    internal interface IBucket
    {
        /// <summary>
        /// The list of <see cref="IContact" />s contained in the <see cref="IBucket" /> sorted by last contact time.
        /// </summary>
        IEnumerable<IContact> Contacts { get; }

        /// <summary>
        /// Event which is raised when the <see cref="IBucket"/> is changed.
        /// </summary>
        event Action<IContact, BucketOperation> OnChanged;

        /// <summary>
        /// Adds an <see cref="IContact" /> to the <see cref="IBucket" /> and maintains the sorting.
        /// </summary>
        /// <param name="contact"></param>
        void Add(IContact contact);

        /// <summary>
        /// Removes an <see cref="IContact" /> from the <see cref="IBucket" /> and maintains the sorting.
        /// </summary>
        /// <param name="contact"></param>
        void Remove(IContact contact);

        /// <summary>
        /// Indicates if the <see cref="IBucket"/> contains the specified <see cref="IContact"/>.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        bool Contains(IContact contact);
        
        /// <summary>
        /// Refreshes the <see cref="IBucket"/>.
        /// </summary>
        void Refresh(bool force = false);

        /// <summary>
        /// Updates the lookup timestamp.
        /// </summary>
        void SetTimestamp();
    }
}