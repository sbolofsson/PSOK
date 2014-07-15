using System.Collections.Generic;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// Represents meta data that can be associated to an <see cref="IPublish{T}" />.
    /// </summary>
    public interface IHeaders
    {
        /// <summary>
        /// Adds an object value with the specified key to the <see cref="IHeaders" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key to store the object under.</param>
        /// <param name="obj">The object to add to the <see cref="IHeaders" />.</param>
        void Add<T>(string key, T obj);

        /// <summary>
        /// Adds a string value with the specified key to the <see cref="IHeaders" />.
        /// </summary>
        /// <param name="key">The key to store the object under.</param>
        /// <param name="value">The string to add to the <see cref="IHeaders" />.</param>
        void Add(string key, string value);

        /// <summary>
        /// Retrieves an object value of type T based on the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key to retrieve the object from.</param>
        /// <returns>The object value stored under the specified key.</returns>
        T Get<T>(string key);

        /// <summary>
        /// Retrieves a string value based on the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The string value stored under the specified key.</returns>
        string Get(string key);

        /// <summary>
        /// Retrieves all the keys from the <see cref="IHeaders" />.
        /// </summary>
        /// <returns>All keys contained in the <see cref="IHeaders" />.</returns>
        IEnumerable<string> GetKeys();

        /// <summary>
        /// Creates a copy of the <see cref="IHeaders" /> into an <see cref="IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <returns>A copy of the <see cref="IHeaders" />.</returns>
        IDictionary<string, string> AsDictionary();
    }
}