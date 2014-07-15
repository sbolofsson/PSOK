using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using PSOK.Kernel.Serialization;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// Represents meta data that can be associated to an <see cref="IPublish{T}" />.
    /// All members of this class are thread safe.
    /// </summary>
    [Serializable, DataContract]
    internal class Headers : IHeaders
    {
        public Headers()
        {
            _innerHeaders = new ConcurrentDictionary<string, string>();
        }

        [DataMember]
        private ConcurrentDictionary<string, string> _innerHeaders;

        /// <summary>
        /// Adds an object value with the specified key to the <see cref="Headers" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key to store the object under.</param>
        /// <param name="obj">The object to add to the <see cref="Headers" />.</param>
        public void Add<T>(string key, T obj)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (ReferenceEquals(obj, null))
                throw new ArgumentNullException("obj");

            string value = Serializer.Serialize(obj);
            Add(key, value);
        }

        /// <summary>
        /// Adds a string value with the specified key to the <see cref="Headers" />.
        /// </summary>
        /// <param name="key">The key to store the object under.</param>
        /// <param name="value">The string to add to the <see cref="Headers" />.</param>
        public void Add(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("value");

            _innerHeaders[key] = value;
        }

        /// <summary>
        /// Retrieves an object value of type T based on the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key to retrieve the object from.</param>
        /// <returns>The object value stored under the specified key.</returns>
        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            string value = Get(key);
            return Serializer.Deserialize<T>(value);
        }

        /// <summary>
        /// Retrieves a string value based on the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The string value stored under the specified key.</returns>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            string value;
            _innerHeaders.TryGetValue(key, out value);
            return value;
        }

        /// <summary>
        /// Retrieves all the keys from the <see cref="Headers" />.
        /// </summary>
        /// <returns>All keys contained in the <see cref="Headers" />.</returns>
        IEnumerable<string> IHeaders.GetKeys()
        {
            return _innerHeaders.Keys.ToList();
        }

        /// <summary>
        /// Creates a copy of the <see cref="Headers" /> into an <see cref="IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <returns>A copy of the <see cref="Headers" />.</returns>
        IDictionary<string, string> IHeaders.AsDictionary()
        {
            return _innerHeaders.Keys.ToDictionary(x => x, x => _innerHeaders[x]);
        }
    }
}