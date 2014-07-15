using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

namespace PSOK.Kernel.Caching
{
    /// <summary>
    /// Thread safe cache manager
    /// </summary>
    public static class CacheManager
    {
        private static readonly ObjectCache Cache = new Cache("kademlia");
        private static readonly ReaderWriterLockSlim CacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Caches an object
        /// </summary>
        /// <param name="key">A unique key to cache the object under</param>
        /// <param name="obj">The object to cache</param>
        /// <param name="cachingOptions">Options on how to cache</param>
        public static void Add(string key, object obj, ICachingOptions cachingOptions)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (obj == null)
                throw new ArgumentNullException("obj");

            if (cachingOptions == null)
                throw new ArgumentNullException("cachingOptions");

            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = DateTime.Now.AddMilliseconds(cachingOptions.Expiration.TotalMilliseconds)
            };

            if (cachingOptions.OnRemoved != null)
                cacheItemPolicy.RemovedCallback += x => cachingOptions.OnRemoved(x.RemovedReason);

            CacheLock.EnterWriteLock();

            Cache.Set(key, obj, cacheItemPolicy);

            CacheLock.ExitWriteLock();
        }

        /// <summary>
        /// Gets an object from the cache based on the given key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T Get<T>(string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            try
            {
                CacheLock.EnterReadLock();
                return Cache.Contains(key) ? Cache[key] as T : null;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                CacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Retrieves an item from the cache and adds it if is not there
        /// </summary>
        /// <param name="key"></param>
        /// <param name="selector"></param>
        /// <param name="cachingOptions"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T GetOrAdd<T>(string key, Func<T> selector, ICachingOptions cachingOptions) where T : class
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (selector == null)
                throw new ArgumentNullException("selector");

            if (cachingOptions == null)
                throw new ArgumentNullException("cachingOptions");

            T value = Get<T>(key);

            if (value != null)
                return value;

            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
            {
                Priority = CacheItemPriority.Default,
                AbsoluteExpiration = DateTime.Now.AddMilliseconds(cachingOptions.Expiration.TotalMilliseconds)
            };

            if (cachingOptions.OnRemoved != null)
                cacheItemPolicy.RemovedCallback += x => cachingOptions.OnRemoved(x.RemovedReason);

            try
            {
                CacheLock.EnterWriteLock();

                value = Cache[key] as T;

                if (value != null)
                    return value;

                value = selector();

                Cache.Set(key, value, cacheItemPolicy);

                return value;
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes an object from the cache based on the given key.
        /// </summary>
        /// <param name="key"></param>
        public static void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            try
            {
                CacheLock.EnterWriteLock();

                if (Cache.Contains(key))
                {
                    Cache.Remove(key);
                }
            }
            finally
            {
                CacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Gets all the cache keys.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<string> GetAllKeys()
        {
            try
            {
                CacheLock.EnterReadLock();
                return Cache.Select(x => x.Key).ToList();
            }
            finally
            {
                CacheLock.ExitReadLock();
            }
        }
    }
}