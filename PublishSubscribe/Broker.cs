using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kernel;
using PSOK.Kernel.Collections;
using PSOK.Kernel.Reflection;
using PSOK.PublishSubscribe.Messages;
using PSOK.PublishSubscribe.Services;
using log4net;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// A class representing a peer in the P2P network.
    /// </summary>
    public class Broker : IBroker
    {
        // Static fields

        /// <summary>
        /// Indicates the instance mode used for all <see cref="Broker"/>s in the current application.
        /// </summary>
        public static InstanceMode InstanceMode
        {
            get { return AppSettings.InstanceMode; }
            set { AppSettings.InstanceMode = value; }
        }

        private static readonly ILog Log = LogManager.GetLogger(typeof(Broker));

        private static readonly ReaderWriterLockSlim InitializeDisposeLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private static IPeer _sharedPeer;

        // Instance fields
        private readonly IPeer _peer;
        private bool _disposed;

        /// <summary>
        /// Constructs a new <see cref="Broker" />.
        /// </summary>
        public Broker()
        {
            // If a shared peer exists, then abort
            if (_sharedPeer != null)
                return;

            Application.Start();

            try
            {
                InitializeDisposeLock.EnterWriteLock();

                if (InstanceMode == InstanceMode.Single && _sharedPeer == null)
                {
                    Peer peer = new Peer();
                    _sharedPeer = peer;
                    peer.Start();
                }
                else if (InstanceMode == InstanceMode.Multiple)
                {
                    Peer peer = new Peer();
                    _peer = peer;
                    peer.Start();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
            finally
            {
                InitializeDisposeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Force initializes the <see cref="Broker" />.
        /// Only use this method if you are dispoing the <see cref="Broker" /> manually.
        /// </summary>
        public void Initialize()
        {
            Peer.Initialize();
        }

        /// <summary>
        /// The <see cref="IPeer" /> associated to the <see cref="Broker" />.
        /// </summary>
        private IPeer Peer
        {
            get
            {
                try
                {
                    InitializeDisposeLock.EnterReadLock();
                    return InstanceMode == InstanceMode.Single ? _sharedPeer : _peer;
                }
                finally
                {
                    InitializeDisposeLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// The <see cref="IDataContext" /> associated to the <see cref="Broker" />.
        /// </summary>
        public IDataContext DataContext
        {
            get { return Peer.DataContext; }
        }

        /// <summary>
        /// Publishes an <see cref="IPublish{T}" /> to all subscribed <see cref="IBroker" />s.
        /// </summary>
        /// <param name="publish"></param>
        public void Publish(IPublish<Message> publish)
        {
            if (publish == null)
                throw new ArgumentNullException("publish");
            
            try
            {
                Peer.Publish(publish);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Publishes an <see cref="IPublish{T}" /> to all subscribed <see cref="IBroker" />s and waits for a response.
        /// Blocks until a <see cref="Message" /> is returned or the specified timeout occurs.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="publish">The <see cref="IPublish{T}" /> to publish.</param>
        /// <param name="timeout">The amount of time to wait for a response.</param>
        /// <returns>
        /// A <see cref="Message" /> of type T if it was published within the specified timeout. Otherwise null is
        /// returned.
        /// </returns>
        public T Publish<T>(IPublish<Message> publish, TimeSpan timeout) where T : Message
        {
            if (publish == null)
                throw new ArgumentNullException("publish");

            if (timeout == null)
                throw new ArgumentNullException("timeout");

            try
            {
                return Peer.Publish<T>(publish, timeout);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Subscribes to a certain <see cref="Message" /> type.
        /// </summary>
        /// <param name="subscription">Information about what should be subscribed to.</param>
        public void Subscribe(ISubscription subscription)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            try
            {
                if (subscription.IncludeSubclasses)
                {
                    Type subscriptionBaseType = subscription.Type;
                    IEnumerable<Type> subscriptionTypes = TypeHelper.GetSubclasses(
                        subscriptionBaseType.IsGenericType ?
                        subscriptionBaseType.GetGenericTypeDefinition() : subscriptionBaseType
                        ).Append(subscriptionBaseType).SelectMany(TypeHelper.CreateTypes).ToList();

                    foreach (Type subscriptionType in subscriptionTypes)
                    {
                        try
                        {
                            ISubscription subscriptionCopy = subscription.MakeCopy(subscriptionType);
                            Peer.Subscribe(subscriptionCopy);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(string.Format("Could not subscribe to messages of type '{0}'.",
                                subscriptionType.AssemblyQualifiedName()), ex);
                        }
                    }
                }
                else
                {
                    Peer.Subscribe(subscription);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a cached <see cref="Message" /> of type TMessage that the <see cref="Broker" /> is subscribed to.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <returns></returns>
        public TMessage GetCachedMessage<TMessage>() where TMessage : Message
        {
            try
            {
                return Peer.GetCachedMessage<TMessage>();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Retrieves a cached <see cref="Message" /> of type TMessage based on the specified <see cref="Message" />.
        /// Use this method for request/response schemes.
        /// </summary>
        /// <typeparam name="TMessage"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        public TMessage GetCachedMessage<TMessage>(Message message) where TMessage : Message
        {
            if (message == null)
                throw new ArgumentNullException("message");

            try
            {
                return Peer.GetCachedMessage<TMessage>(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Disposes the <see cref="Broker" /> and releases all resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                InitializeDisposeLock.EnterWriteLock();

                if (_disposed)
                    return;

                if (_sharedPeer != null)
                    _sharedPeer.Dispose();

                if (_peer != null)
                    _peer.Dispose();

                _disposed = true;
            }
            finally
            {
                InitializeDisposeLock.ExitWriteLock();
            }
        }
    }
}