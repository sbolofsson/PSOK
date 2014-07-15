using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using PSOK.Kademlia.Lookups;
using PSOK.Kademlia.Reports;
using PSOK.Kademlia.Services;
using PSOK.Kernel;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Configuration;
using PSOK.Kernel.Encoding;
using PSOK.Kernel.Security;
using PSOK.Kernel.Services;
using log4net;

// ReSharper disable FunctionNeverReturns
namespace PSOK.Kademlia
{
    /// <summary>
    /// Represents a node in Kademlia.
    /// </summary>
    public class Node : Kernel.Threads.Thread, IDht, INode
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(Node));
        private static readonly ConcurrentDictionary<INode, byte> AllNodes =
            new ConcurrentDictionary<INode, byte>();

        // Instance fields
        private readonly Lazy<INodeDht> _dht;
        private readonly Lazy<IContact> _contact;
        private IKademlia _serviceHost;
        private bool? _isBootstrapper;
        private readonly Lazy<IBucketContainer> _bucketContainer;
        private readonly ConcurrentDictionary<string, IContact> _publishes =
            new ConcurrentDictionary<string, IContact>();

        private readonly ReaderWriterLockSlim _initializeDisposeLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly BlockingCollection<IContact> _informQueue =
            new BlockingCollection<IContact>(new ConcurrentQueue<IContact>());

        private bool _disposed;

        /// <summary>
        /// Construcs a new <see cref="Node"/>.
        /// </summary>
        public Node()
        {
            _dht = new Lazy<INodeDht>(() => new NodeDht(this), LazyThreadSafetyMode.ExecutionAndPublication);
            _contact = new Lazy<IContact>(KeyProvider.GetContact,
                LazyThreadSafetyMode.ExecutionAndPublication);

            _bucketContainer = new Lazy<IBucketContainer>(() =>
            {
                IBucketContainer bucketContainer = new BucketContainer(this);
                bucketContainer.OnChanged += (contact, operation) =>
                {
                    if (operation == BucketOperation.Add)
                        (this as INode).Dht.Replicate(contact);
                };
                bucketContainer.Initialize();
                return bucketContainer;
            }, LazyThreadSafetyMode.ExecutionAndPublication);

            AllNodes[this] = new byte();
        }

        #region Properties

        /// <summary>
        /// Container of all the <see cref="IContact" />s which the <see cref="Node" /> knows.
        /// </summary>
        IBucketContainer INode.BucketContainer
        {
            get { return _bucketContainer.Value; }
        }

        /// <summary>
        /// The local part of the DHT.
        /// </summary>
        INodeDht INode.Dht
        {
            get { return _dht.Value; }
        }

        /// <summary>
        /// The <see cref="Node" />'s contact information
        /// </summary>
        public IContact Contact
        {
            get { return _contact.Value; }
        }

        /// <summary>
        /// Indicates whether the <see cref="Node" /> is initialized.
        /// This property is not thread safe.
        /// </summary>
        private bool CheckIsInitialized
        {
            get
            {
                return _disposed ||
                       (_serviceHost != null && _serviceHost.IsInitialized &&
                        (IsBootstrapper || KnowsAboutBootstrapper));
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="Node" /> is initialized.
        /// </summary>
        bool IServiceHost.IsInitialized
        {
            get
            {
                try
                {
                    _initializeDisposeLock.EnterReadLock();
                    return CheckIsInitialized;
                }
                finally
                {
                    _initializeDisposeLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="Node" /> knows about the bootstrapper.
        /// </summary>
        private bool KnowsAboutBootstrapper
        {
            get { return BootstrapperNode.GetContacts().Any((this as INode).BucketContainer.Contains); }
        }

        private bool IsBootstrapper
        {
            get
            {
                return _isBootstrapper != null
                    ? _isBootstrapper.Value
                    : (_isBootstrapper = !string.IsNullOrEmpty(Config.ReadConfig().Kademlia.NodeId)).Value;
            }
        }

        /// <summary>
        /// All <see cref="INode" />s created in the application.
        /// </summary>
        internal static IEnumerable<INode> Nodes
        {
            get { return AllNodes.Keys.ToList().AsReadOnly(); }
        }

        /// <summary>
        /// Retrieves status information for all <see cref="Node"/>s in the current application.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<INodeStatus> GetNodeStatuses()
        {
            return Nodes.Select(x => x.GetStatus()).ToList();
        }

        #endregion

        #region IServiceHost methods

        /// <summary>
        /// Force initializes the <see cref="Node" />.
        /// </summary>
        public void Initialize()
        {
            if (CheckIsInitialized)
                return;

            try
            {
                _initializeDisposeLock.EnterWriteLock();

                if (CheckIsInitialized)
                    return;

                if (!Started)
                    Start();

                if (_serviceHost == null)
                {
                    _serviceHost = new NodeServiceHost(this);
                }

                if (!_serviceHost.IsInitialized)
                {
                    _serviceHost.Initialize();
                }

                if (!IsBootstrapper && !KnowsAboutBootstrapper)
                {
                    INode node = this;

                    foreach (IContact contact in BootstrapperNode.GetContacts())
                    {
                        node.PingContact(contact);
                        node.BucketContainer.Add(contact);
                    }

                    node.IterativeFindNode(node.Contact.NodeId);
                    node.BucketContainer.RefreshNeighbours();
                }
            }
            finally
            {
                _initializeDisposeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Disposes the <see cref="Node" /> and releases all resources held by this instance.
        /// </summary>
        public override void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _initializeDisposeLock.EnterWriteLock();

                if (_disposed)
                    return;

                _serviceHost.Dispose();
                base.Dispose();

                _disposed = true;
            }
            finally
            {
                _initializeDisposeLock.ExitWriteLock();
            }
        }

        #endregion

        #region INode methods

        /// <summary>
        /// Republishes all key value pairs.
        /// </summary>
        void INode.Republish()
        {
            foreach (KeyValuePair<string, IContact> publish in _publishes)
            {
                IterativeStore(publish.Key, publish.Value);
            }
        }

        /// <summary>
        /// Retrieves the status of the <see cref="Node" />.
        /// </summary>
        /// <returns></returns>
        INodeStatus INode.GetStatus()
        {
            INode node = this;
            return new NodeStatus
            {
                DhtStatus = node.Dht.GetStatus(),
                IsNodeInitialized = (this as IServiceHost).IsInitialized,
                BucketContainerSize = node.BucketContainer.Size,
                BaseUrl = node.Contact.BaseUrl,
                NodeId = node.Contact.NodeId
            };
        }

        /// <summary>
        /// Iteratively tries to store in the DHT that the <see cref="Node"/>
        /// has the data for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        void INode.IterativeStore(string key, object data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            IContact contact = (this as INode).Contact.Copy();
            contact.Data = data;
            _publishes[key] = contact;
            IterativeStore(key, contact);
        }

        /// <summary>
        /// Iteratively tries to store in the DHT that the <see cref="Node"/>
        /// has the data for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="contact"></param>
        private void IterativeStore(string key, IContact contact)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (contact == null)
                throw new ArgumentNullException("contact");

            IEnumerable<IContact> closestNodes = (this as INode).IterativeFindNode(key);

            if (closestNodes == null)
                return;

            IItem item = new Item(contact);

            foreach (IContact closestNode in closestNodes)
            {
                try
                {
                    using (NodeProxy nodeProxy = new NodeProxy(this, closestNode, false))
                    {
                        nodeProxy.Context.Store(key, new List<IItem> {item});
                    }
                }
                catch (Exception ex)
                {
                    if (!Kernel.Exceptions.Exception.IsEndpointDown(ex))
                        Log.Error(ex);
                    (this as INode).PingContact(closestNode);
                }
            }
        }

        /// <summary>
        /// Iteratively tries to find a list of <see cref="IContact" />s in the DHT.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<IContact> INode.IterativeFindNode(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            LookupBase lookup = LookupFactory.GetLookup(this, key, true);
            IResult result = lookup.LookupAsync();
            return result != null ? result.Contacts : null;
        }

        /// <summary>
        /// Iteratively tries to find a value in the DHT.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<IContact> INode.IterativeFindValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            IResult result = (this as IKademlia).FindValue(key);

            if (result != null && result.Entries != null)
                return result.Entries.Select(x => x.Contact).ToList();

            LookupBase lookup = LookupFactory.GetLookup(this, key, false);
            result = lookup.LookupAsync();
            return result != null && result.Entries != null ? result.Entries.Select(x => x.Contact).ToList() : null;
        }

        /// <summary>
        /// Informs the <see cref="INode" /> that an <see cref="IContact" /> is still active.
        /// </summary>
        /// <param name="caller"></param>
        void INode.Inform(IContact caller)
        {
            if (caller == null)
                throw new ArgumentNullException("caller");

            _informQueue.Add(caller);
        }

        /// <summary>
        /// Pings the specified <see cref="IContact" />.
        /// </summary>
        /// <param name="contact"></param>
        void INode.PingContact(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            try
            {
                using (NodeProxy nodeProxy = new NodeProxy(this, contact, false))
                {
                    nodeProxy.Context.Ping();
                }
            }
            catch (Exception)
            {
                (this as INode).BucketContainer.Remove(contact);
            }
        }

        #endregion

        #region IKademlia methods

        /// <summary>
        /// Pings the <see cref="Node" />.
        /// </summary>
        /// <returns></returns>
        bool IKademlia.Ping()
        {
            return true;
        }

        /// <summary>
        /// Instructs the <see cref="Node" /> to store the specified data for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        void IKademlia.Store(string key, IEnumerable<IItem> items)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            if (items == null)
                throw new ArgumentNullException("items");

            (this as INode).Dht.Add(key, items.ToArray());
        }

        /// <summary>
        /// Ask a contact for a list of closest nodes (contacts) which know about the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IEnumerable<IContact> IKademlia.FindNode(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            return (this as INode).BucketContainer.GetClosestContacts(key, Constants.K);
        }

        /// <summary>
        /// Ask a contact for a value, if it is found the value is returned, otherwise it acts like <see cref="IKademlia.FindNode" />.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        IResult IKademlia.FindValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            IEnumerable<IItem> contacts = (this as INode).Dht.Get(key);
            // ReSharper disable PossibleMultipleEnumeration
            return contacts != null && contacts.Any() ? new Result(contacts) : new Result((this as IKademlia).FindNode(key));
            // ReSharper restore PossibleMultipleEnumeration
        }

        #endregion

        #region IDht methods

        /// <summary>
        /// Stores in the DHT that the application
        /// has the data for the specified key. 
        /// </summary>
        /// <param name="key">A unique key identifying an associated value.</param>
        /// <param name="data">Extra information to store in the DHT. Should be not be the actual data associated to the key.</param>
        public void Put(string key, object data)
        {
            (this as INode).IterativeStore(key, data);
        }

        /// <summary>
        /// Conducts a search in the DHT for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A collection of contact information about the applications' who posses the data stored under the specified key.</returns>
        public IEnumerable<IContact> Get(string key)
        {
            return (this as INode).IterativeFindValue(key);
        }

        #endregion

        #region Thread methods

        /// <summary>
        /// Starts the asynchronous processing of inform requests.
        /// </summary>
        protected override void Run()
        {
            while (true)
            {
                IContact caller = _informQueue.Take();
                (this as INode).BucketContainer.Add(caller);
            }
        }

        #endregion

        #region Static methods

        /// <summary>
        /// Computes the distance between the specified node ids.
        /// </summary>
        /// <param name="nodeId1"></param>
        /// <param name="nodeId2"></param>
        /// <returns></returns>
        internal static byte[] DistanceBytes(string nodeId1, string nodeId2)
        {
            if (string.IsNullOrEmpty(nodeId1))
                throw new ArgumentNullException("nodeId1");

            if (string.IsNullOrEmpty(nodeId2))
                throw new ArgumentNullException("nodeId2");

            if (nodeId1.Length != nodeId2.Length)
                throw new ArgumentException("The specified node ids must be of equal length");

            byte[] nodeId1Bytes = EncryptionProvider.GetBytes(nodeId1);
            byte[] nodeId2Bytes = EncryptionProvider.GetBytes(nodeId2);

            // Compute XOR
            byte[] distance = new byte[nodeId1Bytes.Length];
            for (int i = distance.Length - 1; i >= 0; i--)
            {
                distance[i] = (byte)(nodeId1Bytes[i] ^ nodeId2Bytes[i]);
            }

            return distance;
        }

        /// <summary>
        /// Computes the distance between two node ids.
        /// </summary>
        /// <param name="nodeId1"></param>
        /// <param name="nodeId2"></param>
        /// <returns></returns>
        internal static double Distance(string nodeId1, string nodeId2)
        {
            if (string.IsNullOrEmpty(nodeId1))
                throw new ArgumentNullException("nodeId1");

            if (string.IsNullOrEmpty(nodeId2))
                throw new ArgumentNullException("nodeId2");

            string cacheKey = string.Format("node#{0}#{1}", nodeId1, nodeId2);
            object distance = CacheManager.GetOrAdd<object>(cacheKey,
                () => new BigInteger(BitOperations.EnsurePositive(DistanceBytes(nodeId1, nodeId2))),
                new CachingOptions<object>
                {
                    EnableCaching = true,
                    Expiration = new TimeSpan(24, 0, 0)
                });
            return (double)((BigInteger)distance);
        }

        #endregion

    }
}
// ReSharper restore FunctionNeverReturns