/*using System;
using System.ServiceModel;
using System.Threading;
using EPIC.P2P.Kernel.Exceptions;

namespace EPIC.P2P.Kernel.Services
{
    public class Pool<T> where T : class
    {
        private readonly T[] items;
        private int _count;
        private readonly SemaphoreSlim _poolLock = new SemaphoreSlim(1, 1);
        private readonly Action<ChannelFactory<T>> _modifyChannelFactory;
        private readonly Action<System.ServiceModel.Channels.Binding> _modifyBinding;
        private ChannelFactory<T> _channelFactory;

        public Pool(int maxCount, Action<ChannelFactory<T>> modifyChannelFactory = null, Action<System.ServiceModel.Channels.Binding> modifyBinding = null)
        {
            _modifyChannelFactory = modifyChannelFactory;
            _modifyBinding = modifyBinding;
            items = new T[maxCount];
        }

        public int Count
        {
            get { return _count; }
        }

        public T Take(string url)
        {
            try
            {
                _poolLock.Wait();

                if (_count > 0)
                {
                    T item = items[--_count];
                    items[_count] = null;

                    if (item != null && (item as IClientChannel).State != CommunicationState.Opened)
                    {
                        ServiceHelper.CloseClientChannel((item as IClientChannel));
                        item = null;
                    }

                    if (item == null)
                        item = CreateProxy(url);

                    return item;
                }

                return CreateProxy(url);
            }
            finally
            {
                _poolLock.Release();
            }
        }

        public bool Return(T item)
        {
            try
            {
                _poolLock.Wait();
                if (_count < items.Length)
                {
                    items[_count++] = item;
                    return true;
                }
                ServiceHelper.CloseClientChannel((item as IClientChannel));
                return false;
            }
            finally
            {
                _poolLock.Release();
            }
        }

        public void Clear()
        {
            try
            {
                _poolLock.Wait();
                for (int i = 0; i < _count; i++)
                    items[i] = null;
                _count = 0;
            }
            finally
            {
                _poolLock.Release();
            }
        }

        private ChannelFactory<T> ChannelFactory
        {
            get
            {
                if (_channelFactory != null && _channelFactory.State == CommunicationState.Opened)
                    return _channelFactory;

                System.ServiceModel.Channels.Binding binding = Binding.GetDefaultBinding();

                if (_modifyBinding != null)
                    _modifyBinding(binding);

                ChannelFactory<T> channelFactory = new ChannelFactory<T>(binding);
                ServiceBehavior behavior = new ServiceBehavior(Transport.Protocol, Transport.EnableSsl);
                behavior.SetBehavior(channelFactory);

                if (_modifyChannelFactory != null)
                    _modifyChannelFactory(channelFactory);

                return (_channelFactory = channelFactory);
            }
        }

        private T CreateProxy(string url)
        {
            EndpointIdentity endpointIdentity = EndpointIdentity.CreateDnsIdentity(Transport.ServiceDomainName);
            T proxy = ChannelFactory.CreateChannel(new EndpointAddress(new Uri(url), endpointIdentity));

            if (proxy == null)
                throw new ServiceException("Could not create a service proxy context.");

            ((IClientChannel)proxy).Open();
            return proxy;
        }
    }
}
*/