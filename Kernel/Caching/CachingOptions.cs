using System;
using System.Runtime.Caching;
using PSOK.Kernel.Reflection;

namespace PSOK.Kernel.Caching
{
    /// <summary>
    /// Options specifying how to cache objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachingOptions<T> : ICachingOptions where T : class
    {
        /// <summary>
        /// Specifies whether caching is enabled.
        /// </summary>
        public bool EnableCaching { get; set; }

        /// <summary>
        /// The expiration of the cached item.
        /// </summary>
        public TimeSpan Expiration { get; set; }

        /// <summary>
        /// Callback handler that will be invoked when the cached object is removed from the cache.
        /// </summary>
        public Action<CacheEntryRemovedReason> OnRemoved { get; set; }

        /// <summary>
        /// A function used to calculate a cache key based on the instance of type T that is being cached.
        /// </summary>
        public Func<T, string> CacheKey { get; set; }

        /// <summary>
        /// Indicates whether an extra cache key has been specified.
        /// </summary>
        bool ICachingOptions.HasCacheKey
        {
            get { return CacheKey != null; }
        }

        /// <summary>
        /// A unique key identifying the type of object being cached.
        /// </summary>
        string ICachingOptions.Key
        {
            get { return typeof(T).Key(); }
        }

        /// <summary>
        /// Calculates a cache key for the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string ICachingOptions.GetCacheKey(object message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            return CacheKey != null ? CacheKey(message as T) : null;
        }
    }
}