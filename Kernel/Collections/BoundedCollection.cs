using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PSOK.Kernel.Exceptions;

namespace PSOK.Kernel.Collections
{
    /// <summary>
    /// A collection with bounds
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BoundedCollection<T> : Collection<T>
    {
        private readonly int _bound;

        /// <summary>
        /// The maximum number of elements the collection can contain
        /// </summary>
        public int Bound { get { return _bound; } }

        /// <summary>
        /// Instantiates a new collection with the given bound
        /// </summary>
        /// <param name="bound"></param>
        public BoundedCollection(int bound) : base(new List<T>(bound))
        {
            if(bound <= 0)
                throw new ArgumentException("Collection bound cannot be zero or lower.");

            _bound = bound;
        }

        /// <summary>
        /// Adds a new element to the collection
        /// </summary>
        /// <param name="element"></param>
        public new void Add(T element)
        {
            if (Bound < Count)
                throw new CollectionException("The bound has been exceeded.");
            base.Add(element);
        } 
    }
}
