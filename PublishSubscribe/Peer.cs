using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kademlia;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Events;
using PSOK.Kernel.Reflection;
using PSOK.PublishSubscribe.Events;
using PSOK.PublishSubscribe.Messages;
using PSOK.PublishSubscribe.Pipelines;
using PSOK.PublishSubscribe.Reports;
using PSOK.PublishSubscribe.Services;
using log4net;
using Thread = PSOK.Kernel.Threads.Thread;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// Provides a layer between the consuming application and the Kademlia network.
    /// </summary>
    internal class Peer : Thread, IPeer
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(Peer));

        private static readonly ConcurrentDictionary<IPeer, byte> AllPeers =
            new ConcurrentDictionary<IPeer, byte>();

        // Instance fields
        private readonly ReaderWriterLockSlim _initializeDisposeLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly ConcurrentDictionary<string, ISubscription> _subscriptions =
            new ConcurrentDictionary<string, ISubscription>();

        private readonly IPeerServiceHost _serviceHost;
        private readonly Lazy<IDataContext> _dataContext;
        private readonly Lazy<IDht> _node;

        private bool _disposed;

        /// <summary>
        /// Constructs a new <see cref="Peer" />.
        /// </summary>
        public Peer()
        {
            AllPeers[this] = new byte();
            _dataContext = new Lazy<IDataContext>(() => new DataServiceHost(this),
                LazyThreadSafetyMode.ExecutionAndPublication);
            _node = new Lazy<IDht>(() => new Node(), LazyThreadSafetyMode.ExecutionAndPublication);
            _serviceHost = new PeerServiceHost(this);
        }

        #region Fields and properties

        /// <summary>
        /// Provides access to the underlying DHT.
        /// </summary>
        public IDht Node
        {
            get { return _node.Value; }
        }

        /// <summary>
        /// The <see cref="ISubscription" />s of the <see cref="IPeer" />.
        /// </summary>
        public IEnumerable<ISubscription> Subscriptions
        {
            get { return _subscriptions.Values.ToList(); }
        }

        /// <summary>
        /// The <see cref="IDataContext" /> associated to the <see cref="Peer" />.
        /// </summary>
        public IDataContext DataContext
        {
            get { return _dataContext.Value; }
        }

        /// <summary>
        /// Indicates whether the <see cref="Peer" /> is initialized.
        /// This property is not thread safe.
        /// </summary>
        private bool CheckIsInitialized
        {
            get { return _disposed || (Node.IsInitialized && _serviceHost.IsInitialized && DataContext.IsInitialized); }
        }

        /// <summary>
        /// Indicates whether the <see cref="Peer" /> is initialized.
        /// </summary>
        public bool IsInitialized
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
        /// All <see cref="IPeer" />s created in the application.
        /// </summary>
        public static IEnumerable<IPeer> Peers
        {
            get { return AllPeers.Keys.ToList().AsReadOnly(); }
        }

        #endregion

        #region Thread methods

        /// <summary>
        /// Initializes the <see cref="Peer" />.
        /// </summary>
        protected override void Run()
        {
            Initialize();
        }

        #endregion

        #region IServiceHost methods

        /// <summary>
        /// Force initializes the <see cref="Peer" />.
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

                if (!Node.IsInitialized)
                {
                    Node.Initialize();
                }

                if (!_serviceHost.IsInitialized)
                {
                    _serviceHost.Initialize();
                }

                if (!DataContext.IsInitialized)
                {
                    DataContext.Initialize();
                }

                if (CheckIsInitialized)
                    Log.Info("Initialized successfully.");
                else
                    Log.Error("Initialization failed.");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
            finally
            {
                _initializeDisposeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Disposes the <see cref="Peer" /> and releases all resources held by this instance.
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

                if (_serviceHost != null)
                    _serviceHost.Dispose();

                Node.Dispose();

                byte b;
                AllPeers.TryRemove(this, out b);

                Log.Info("A Peer instance has been disposed.");

                _disposed = true;
            }
            finally
            {
                _initializeDisposeLock.ExitWriteLock();
            }
        }

        #endregion

        #region IPeerServiceHost methods

        /// <summary>
        /// Is invoked when a relevant <see cref="Message" /> is published.
        /// </summary>
        public void Callback(IPublish<Message> publish)
        {
            if (publish == null)
                throw new ArgumentNullException("publish");

            EventQueue.RaiseEvent(new CallbackEvent
            {
                PipelineArgs = new CallbackArgs { Peer = this, Publish = publish }
            });
        }

        /// <summary>
        /// Checks if the <see cref="Peer" /> is alive.
        /// </summary>
        /// <returns></returns>
        public bool Ping()
        {
            return true;
        }

        #endregion

        #region IBroker methods

        /// <summary>
        /// Publishes an <see cref="IPublish{T}" /> to all subscribed <see cref="IPeer" />s.
        /// </summary>
        public void Publish(IPublish<Message> publish)
        {
            if (publish == null)
                throw new ArgumentNullException("publish");

            // If the publish is done inside a callback handler we know it's a request/response scheme
            // and therefore the PubSubContext will be initialized.
            if (PubSubContext.IsInitialized)
            {
                string callerId = PubSubContext.Current.Publish.Headers.Get("callerId");
                if (!string.IsNullOrEmpty(callerId))
                    publish.Headers.Add("callerId", callerId);
            }

            EventQueue.RaiseEvent(new PublishEvent
            {
                PipelineArgs = new PublishArgs { Peer = this, Publish = publish }
            });
        }

        /// <summary>
        /// Publishes an <see cref="IPublish{T}" /> to all subscribed <see cref="IPeer" />s and waits for a response.
        /// </summary>
        public T Publish<T>(IPublish<Message> publish, TimeSpan timeout) where T : Message
        {
            if (publish == null)
                throw new ArgumentNullException("publish");

            if (timeout == null)
                throw new ArgumentNullException("timeout");

            if (GetSubscription(publish.Key) == null)
            {
                Subscribe(new Subscription<T>());
            }

            string callerId = Guid.NewGuid().ToString();
            string responseKey = typeof(T).Key();
            SemaphoreSlim handle = new SemaphoreSlim(0);

            string cacheKeyHandle = string.Format("publish#{0}#{1}#handle", responseKey, callerId);
            string cacheKeyMessage = string.Format("publish#{0}#{1}#message", responseKey, callerId);

            CacheManager.Add(cacheKeyHandle, handle, new CachingOptions<SemaphoreSlim>
            {
                EnableCaching = true,
                Expiration = timeout + timeout
            });

            publish.Headers.Add("callerId", callerId);

            ISubscription subscription = new Subscription<T>();
            if (!_subscriptions.ContainsKey(subscription.Key))
                Subscribe(subscription);

            Publish(publish);

            handle.Wait(timeout);

            T message = CacheManager.Get<T>(cacheKeyMessage);

            CacheManager.Remove(cacheKeyMessage);

            return message;
        }

        /// <summary>
        /// Subscribes to a certain <see cref="Message" /> type.
        /// </summary>
        /// <param name="subscription"></param>
        public void Subscribe(ISubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            _subscriptions[subscription.Key] = subscription;

            EventQueue.RaiseEvent(new SubscribeEvent
            {
                PipelineArgs = new SubscribeArgs { Peer = this, Subscription = subscription }
            });
        }

        /// <summary>
        /// Retrieves a cached <see cref="Message" /> of type T that the <see cref="Peer" /> is subscribed to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetCachedMessage<T>() where T : Message
        {
            string callbackKey = string.Format("callback#{0}#{1}", Node.Contact.NodeId,
                typeof(T).AssemblyQualifiedName());
            return CacheManager.Get<T>(callbackKey);
        }

        /// <summary>
        /// Retrieves a cached <see cref="Message" /> of type T based on the specified <see cref="Message" />.
        /// Use this method for request/response schemes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public T GetCachedMessage<T>(Message message) where T : Message
        {
            if (message == null)
                throw new ArgumentNullException("message");

            string subscriptionKey = typeof(T).Key();
            string callbackKey = string.Format("callback#{0}#{1}", Node.Contact.NodeId, subscriptionKey);
            ISubscription subscription = GetSubscription(subscriptionKey);
            IPublish<Message> request = CacheManager.Get<IPublish<Message>>(string.Format("publish#{0}#{1}",
                Node.Contact.NodeId,
                message.GetType().Key()));

            if (request == null)
                return null;

            string extraKey = subscription.CachingOptions.GetCacheKey(request.Message);
            string cacheKey = string.Format("{0}#{1}", callbackKey, extraKey);
            return CacheManager.Get<T>(cacheKey);
        }

        #endregion

        #region IPeer

        /// <summary>
        /// Retrieves an <see cref="ISubscription" /> based on the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ISubscription GetSubscription(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            ISubscription subscription;
            _subscriptions.TryGetValue(key, out subscription);
            return subscription;
        }

        /// <summary>
        /// Indicates the status of the <see cref="Peer" />.
        /// </summary>
        /// <returns></returns>
        public IBrokerStatus GetStatus()
        {
            IEnumerable<ISubscriptionStatus> statuses = _subscriptions.Values.Select(x =>
                new SubscriptionStatus
                {
                    Type = x.Type.AssemblyQualifiedName(),
                    Key = x.Type.Key()
                } as ISubscriptionStatus
            ).ToList();

            return new BrokerStatus
            {
                IsBrokerInitialized = IsInitialized,
                SubscriptionCount = _subscriptions.Count,
                SubscriptionStatuses = statuses
            };
        }

        #endregion
    }
}