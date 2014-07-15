using System;
using System.Collections.Generic;
using System.Linq;

namespace PSOK.Kernel.Collections
{
    /// <summary>
    /// Defines extension methods for collections.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Computes the Cartesian product of n sequences.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequences"></param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            // The implementation of this method has been taken from
            // http://blogs.msdn.com/b/ericlippert/archive/2010/06/28/computing-a-cartesian-product-with-linq.aspx
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
              emptyProduct,
              (acc, cur) =>
                from sequence in acc
                from item in cur
                select sequence.Concat(new[] { item }));
        }

        /// <summary>
        /// Appends an item of type T to the specified enumerable and returns the resulting collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            return ConcatIterator(item, enumerable, false);
        }


        /// <summary>
        /// Prepends an item of type T to the specified enumerable and returns the resulting collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> enumerable, T item)
        {
            if (enumerable == null)
                throw new ArgumentNullException("enumerable");

            return ConcatIterator(item, enumerable, true);
        }

        private static IEnumerable<T> ConcatIterator<T>(T item, IEnumerable<T> enumerable, bool head)
        {
            if (head)
                yield return item;
            foreach (var element in enumerable)
                yield return element;
            if (!head)
                yield return item;
        }
    }
}
