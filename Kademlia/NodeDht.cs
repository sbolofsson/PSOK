using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;
using PSOK.Kademlia.Reports;
using PSOK.Kademlia.Services;
using PSOK.Kernel.Caching;
using log4net;

namespace PSOK.Kademlia
{
    /// <summary>
    /// Defines a DHT.
    /// </summary>
    internal class NodeDht : INodeDht
    {
        // Instance fields
        private readonly ObjectCache _cache = new Cache("dht");
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly INode _owner;

        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(NodeDht));

        /// <summary>
        /// Constructs a new <see cref="NodeDht"/> with the given <see cref="INode"/> as owner.
        /// </summary>
        /// <param name="owner"></param>
        public NodeDht(INode owner)
        {
            _owner = owner;
        }

        #region INodeDht methods

        /// <summary>
        /// Adds some data using the specified key to the DHT.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        public void Add(string key, params IItem[] items)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (items == null)
                throw new ArgumentNullException("items");

            try
            {
                _cacheLock.EnterWriteLock();

                ConcurrentDictionary<string, IItem> existingItems =
                    _cache.Contains(key)
                        ? (_cache[key] as ConcurrentDictionary<string, IItem> ?? new ConcurrentDictionary<string, IItem>())
                        : new ConcurrentDictionary<string, IItem>();

                foreach (IItem item in items)
                {
                    IItem existingItem;
                    if ((existingItems.TryGetValue(item.Contact.NodeId, out existingItem) &&
                        item.Expiration > existingItem.Expiration) ||
                        !existingItems.ContainsKey(item.Contact.NodeId)
                        )
                    {
                        existingItems[item.Contact.NodeId] = item;
                    }
                }

                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
                {
                    Priority = CacheItemPriority.Default,
                    AbsoluteExpiration = existingItems.Values.Max(x => x.Expiration)
                };

                _cache.Set(key, existingItems, cacheItemPolicy);
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Retrieves some data identified by the specified key from the DHT.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<IItem> Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            try
            {
                _cacheLock.EnterReadLock();

                if (!_cache.Contains(key))
                    return null;

                ConcurrentDictionary<string, IItem> items = _cache[key] as ConcurrentDictionary<string, IItem>;
                return items != null ? items.Values.ToList() : null;
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Ensures the integrity of all items in the DHT.
        /// </summary>
        public void EnsureIntegrity()
        {
            IEnumerable<Tuple<string, ConcurrentDictionary<string, IItem>>> items = GetItems();
            foreach (Tuple<string, ConcurrentDictionary<string, IItem>> item in items)
            {
                ConcurrentDictionary<string, IItem> contactItems = item.Item2;
                foreach (IItem contactItem in contactItems.Values.ToList())
                {
                    if (!contactItem.IsExpired)
                        continue;

                    IItem dummy;
                    contactItems.TryRemove(contactItem.Contact.NodeId, out dummy);
                }
            }
        }

        /// <summary>
        /// Replicates all items in the DHT to the specified <see cref="IContact"/> if it is closer
        /// to any items than the owner of the DHT.
        /// </summary>
        /// <param name="contact"></param>
        public void Replicate(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            // Retrieve all items where the specified contact is closer than the owner of the DHT
            IEnumerable<Tuple<string, ConcurrentDictionary<string, IItem>>> items = GetItems()
                .Where(x =>
                Node.Distance(contact.NodeId, x.Item1) <
                Node.Distance(_owner.Contact.NodeId, x.Item1))
                .ToList();

            foreach (Tuple<string, ConcurrentDictionary<string, IItem>> item in items)
            {
                string key = item.Item1;
                IEnumerable<IItem> values = item.Item2.Values.ToList();
                Replicate(key, values, () => new[] { contact });
            }
        }

        /// <summary>
        /// Replicates all items in the DHT to Kademlia.
        /// </summary>
        public void Replicate()
        {
            IEnumerable<Tuple<string, ConcurrentDictionary<string, IItem>>> items = GetItems();

            foreach (Tuple<string, ConcurrentDictionary<string, IItem>> item in items)
            {
                // Retrieve the (key, value) from the DHT
                string key = item.Item1;
                IEnumerable<IItem> values = item.Item2.Values.ToList();
                Replicate(key, values, () => _owner.IterativeFindNode(key));
            }
        }

        /// <summary>
        /// Indicates the status of the DHT.
        /// </summary>
        /// <returns></returns>
        public INodeDhtStatus GetStatus()
        {
            IEnumerable<IItemStatus> items =
                GetItems()
                .Select(x => new ItemStatus
                {
                    Key = x.Item1
                } as IItemStatus).ToList();

            return new DhtStatus
            {
                Size = items.Count(),
                Items = items
            };
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Replicates the given key and value at the closest nodes.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        /// <param name="closestNodesSelector"></param>
        private void Replicate(string key, IEnumerable<IItem> items, Func<IEnumerable<IContact>> closestNodesSelector)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if(items == null)
                throw new ArgumentNullException("items");

            if(closestNodesSelector == null)
                throw new ArgumentNullException("closestNodesSelector");

            List<IItem> itemsToReplicate = items.Where(x => !x.IsExpired).ToList();

            if (!itemsToReplicate.Any())
                return;

            IEnumerable<IContact> closestNodes = closestNodesSelector();
            if (closestNodes == null)
                return;

            foreach (IContact closestNode in closestNodes)
            {
                if (closestNode.Equals(_owner.Contact))
                    continue;

                try
                {
                    using (NodeProxy nodeProxy = new NodeProxy(_owner, closestNode, false))
                    {
                        nodeProxy.Context.Store(key, itemsToReplicate);
                    }
                }
                catch (Exception ex)
                {
                    _owner.PingContact(closestNode);
                    if (!Kernel.Exceptions.Exception.IsEndpointDown(ex))
                        Log.Error(ex);
                }
            }
        }

        /// <summary>
        /// Retrieves all the item in the DHT.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Tuple<string, ConcurrentDictionary<string, IItem>>> GetItems()
        {
            try
            {
                _cacheLock.EnterReadLock();
                return _cache
                    .Select(
                        x =>
                            new Tuple<string, ConcurrentDictionary<string, IItem>>(x.Key,
                                x.Value as ConcurrentDictionary<string, IItem>))
                                .Where(x => x != null)
                    .ToList();
            }
            finally
            {
                _cacheLock.ExitReadLock();
            }
        }

        #endregion
    }
}