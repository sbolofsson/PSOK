using System;
using System.Runtime.Caching;

// ReSharper disable BaseMethodCallWithDefaultParameter
namespace PSOK.Kernel.Caching
{
    /// <summary>
    /// A cache mechanism.
    /// </summary>
    public class Cache : MemoryCache
    {
        /// <summary>
        /// Constructs a new <see cref="Cache"/> with the specified name.
        /// </summary>
        /// <param name="name"></param>
        public Cache(string name) : base(name) { }
        
        /// <summary>
        /// Sets a <see cref="System.Runtime.Caching.CacheItem"/> using the specified <see cref="System.Runtime.Caching.CacheItemPolicy"/>.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="policy"></param>
        public override void Set(CacheItem item, CacheItemPolicy policy)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (policy == null)
                throw new ArgumentNullException("policy");

            Set(item.Key, item.Value, policy, item.RegionName);
        }

        /// <summary>
        /// Sets an object value using the specified key, expiration and region name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="absoluteExpiration"></param>
        /// <param name="regionName"></param>
        public override void Set(string key, object value, DateTimeOffset absoluteExpiration, string regionName = null)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (absoluteExpiration == null)
                throw new ArgumentNullException("absoluteExpiration");

            Set(key, value, new CacheItemPolicy { AbsoluteExpiration = absoluteExpiration }, regionName);
        }

        /// <summary>
        /// Sets an object value using the specified key, <see cref="System.Runtime.Caching.CacheItemPolicy"/> and region name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="policy"></param>
        /// <param name="regionName"></param>
        public override void Set(string key, object value, CacheItemPolicy policy, string regionName = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (value == null)
                throw new ArgumentNullException("value");

            if (policy == null)
                throw new ArgumentNullException("policy");

            base.Set(CreateKeyWithRegion(key, regionName), value, policy);
        }

        /// <summary>
        /// Retrieves a <see cref="System.Runtime.Caching.CacheItem"/> using the specified key and region name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public override CacheItem GetCacheItem(string key, string regionName = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            CacheItem temporary = base.GetCacheItem(CreateKeyWithRegion(key, regionName));

            if (temporary == null)
                throw new ArgumentException(string.Format("Could not retrieve CacheItem based on the specified key ('{0}') and region name ('{1}').",
                    key, regionName));

            return new CacheItem(key, temporary.Value, regionName);
        }

        /// <summary>
        /// Retrieves an object value using the specified key and region name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="regionName"></param>
        /// <returns></returns>
        public override object Get(string key, string regionName = null)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            return base.Get(CreateKeyWithRegion(key, regionName));
        }

        /// <summary>
        /// Indicates the capabilities of the <see cref="Cache"/>.
        /// </summary>
        public override DefaultCacheCapabilities DefaultCacheCapabilities
        {
            get
            {
                return (base.DefaultCacheCapabilities | DefaultCacheCapabilities.CacheRegions);
            }
        }

        /// <summary>
        /// Constructs a cache key using the specified key and region name.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        private string CreateKeyWithRegion(string key, string region)
        {
            return region != null
                ? string.Format("region#{0}#{1}", region, key)
                : key;
        }
    }
}
// ReSharper restore BaseMethodCallWithDefaultParameter