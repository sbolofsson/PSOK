using System;
using System.ServiceModel;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Exceptions;
using PSOK.Kernel.Reflection;
using Exception = System.Exception;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// A generic service proxy.
    /// </summary>
    /// <typeparam name="T">The service interface to connect to.</typeparam>
    public class ServiceProxy<T> : IDisposable where T : class
    {
        // Instance fields
        
        /// <summary>
        /// A proxy object representing the service.
        /// </summary>
        protected T Proxy;

        /// <summary>
        /// The <see cref="System.ServiceModel.ChannelFactory{TChannel}"/> used to create the <see cref="Proxy"/>.
        /// </summary>
        protected ChannelFactory<T> ChannelFactory;
        
        /// <summary>
        /// The url of the service, which is to be communicated with.
        /// </summary>
        protected readonly string Url;

        /// <summary>
        /// Constructs a new <see cref="ServiceProxy{T}"/> with the specified url.
        /// </summary>
        /// <param name="url"></param>
        public ServiceProxy(string url)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            Url = url;
        }

        /// <summary>
        /// Initializes a new channel factory.
        /// </summary>
        /// <param name="modifyChannelFactory"></param>
        /// <param name="modifyBinding"></param>
        protected virtual void Initialize(Action<ChannelFactory<T>> modifyChannelFactory = null, Action<System.ServiceModel.Channels.Binding> modifyBinding = null)
        {
            string cacheKey = string.Format("serviceproxy#channelfactory#{0}", typeof (T).Key());
            ICachingOptions cachingOptions = new CachingOptions<ChannelFactory<T>>
            {
                EnableCaching = true,
                Expiration = new TimeSpan(24, 0, 0)
            };
            ChannelFactory = CacheManager.GetOrAdd(cacheKey, () => GetChannelFactory(modifyChannelFactory, modifyBinding), cachingOptions);
            if (ChannelFactory.State == CommunicationState.Faulted)
            {
                ChannelFactory = GetChannelFactory(modifyChannelFactory, modifyBinding);
                CacheManager.Add(cacheKey, ChannelFactory, cachingOptions);
            }
        }

        /// <summary>
        /// Gets a reference to the service interface of type T represented by the current instance.
        /// </summary>
        public T Context
        {
            get
            {
                if (Proxy != null)
                    return Proxy;

                Proxy = GetProxy();

                return Proxy;
            }
        }

        /// <summary>
        /// Indicates whether logging is enabled for this <see cref="ServiceProxy{T}"/>.
        /// </summary>
        public bool EnableLogging { get; set; }

        /// <summary>
        /// Disposes the <see cref="Proxy" />.
        /// </summary>
        public void Dispose()
        {
            ServiceHelper.CloseClientChannel(Proxy as IClientChannel);
        }

        private ChannelFactory<T> GetChannelFactory(Action<ChannelFactory<T>> modifyChannelFactory = null, Action<System.ServiceModel.Channels.Binding> modifyBinding = null)
        {
            System.ServiceModel.Channels.Binding binding = Binding.GetDefaultBinding();

            if (modifyBinding != null)
                modifyBinding(binding);

            ChannelFactory<T> channelFactory = new ChannelFactory<T>(binding);
            ServiceBehavior behavior = new ServiceBehavior(Transport.Protocol, Transport.EnableSsl);
            behavior.SetBehavior(channelFactory);

            if (modifyChannelFactory != null)
                modifyChannelFactory(channelFactory);

            if (EnableLogging)
                channelFactory.Endpoint.Behaviors.Add(new LogMessageBehavior());

            return channelFactory;
        }

        private T GetProxy()
        {
            try
            {
                Initialize();

                EndpointIdentity endpointIdentity =
                    EndpointIdentity.CreateDnsIdentity(Transport.ServiceDomainName);
                T proxy = ChannelFactory.CreateChannel(new EndpointAddress(new Uri(Url), endpointIdentity));

                if (proxy == null)
                    throw new ServiceException("Could not create a service proxy context.");

                ((IClientChannel)proxy).Open();
                return proxy;
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }
    }
}