using System;
using System.Runtime.Caching;

namespace PSOK.Kernel.Caching
{
    /// <summary>
    /// Options specifying how to cache objects
    /// </summary>
    public interface ICachingOptions
    {
        /// <summary>
        /// Specifies whether caching is enabled.
        /// </summary>
        bool EnableCaching { get; set; }

        /// <summary>
        /// The expiration of the cached item.
        /// </summary>
        TimeSpan Expiration { get; set; }

        /// <summary>
        /// Callback handler that will be invoked when the cached object is removed from the cache.
        /// </summary>
        Action<CacheEntryRemovedReason> OnRemoved { get; set; }

        /// <summary>
        /// Calculates a cache key for the given message.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        string GetCacheKey(object message);

        /// <summary>
        /// Indicates whether an extra cache key has been specified.
        /// </summary>
        bool HasCacheKey { get; }

        /// <summary>
        /// A unique key identifying the type of object being cached.
        /// </summary>
        string Key { get; }
    }
}