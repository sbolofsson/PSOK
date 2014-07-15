using System;
using System.Collections.Generic;
using PSOK.Kernel;

namespace PSOK.Kademlia
{
    /// <summary>
    /// A container of <see cref="IBucket" />s.
    /// </summary>
    internal interface IBucketContainer
    {
        /// <summary>
        /// Indicates the amount of <see cref="IContact"/>s in the <see cref="IBucketContainer"/>.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Initializes the <see cref="IBucketContainer" />.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Adds an <see cref="IContact" /> to the appropriate <see cref="IBucket" />.
        /// </summary>
        /// <param name="contact"></param>
        void Add(IContact contact);

        /// <summary>
        /// Removes an <see cref="IContact" /> from the appropriate <see cref="IBucket" />.
        /// </summary>
        /// <param name="contact"></param>
        void Remove(IContact contact);

        /// <summary>
        /// Indicates if the <see cref="IBucketContainer"/> contains the specified <see cref="IContact"/>.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        bool Contains(IContact contact);

        /// <summary>
        /// Event which is raised when the <see cref="IBucketContainer"/> is changed.
        /// </summary>
        event Action<IContact, BucketOperation> OnChanged;

        /// <summary>
        /// Finds the closest contacts for the specified key.
        /// </summary>
        /// <param name="key">The key to retrieve the <see cref="IContact" />s based on.</param>
        /// <param name="count">The maximum number of <see cref="IContact" />s to retrieve.</param>
        /// <returns></returns>
        List<IContact> GetClosestContacts(string key, int count);

        /// <summary>
        /// Refreshes all the <see cref="IBucket" />s.
        /// </summary>
        void RefreshBuckets();

        /// <summary>
        /// Refreshes all <see cref="IBucket"/>s further away than the closest neighbour.
        /// </summary>
        void RefreshNeighbours();

        /// <summary>
        /// Updates the timestamp for the appropriate <see cref="IBucket"/>.
        /// </summary>
        /// <param name="key"></param>
        void SetTimestamp(string key);
    }
}