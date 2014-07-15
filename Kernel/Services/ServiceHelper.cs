using System;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Helper class for working with services.
    /// </summary>
    public static class ServiceHelper
    {
        /// <summary>
        /// Closes a service host
        /// </summary>
        /// <param name="serviceHost"></param>
        public static void CloseServiceHost(System.ServiceModel.ServiceHostBase serviceHost)
        {
            if (serviceHost == null || !IsServiceHostOpen(serviceHost))
                return;

            try
            {
                serviceHost.Close();
            }
            catch (Exception)
            {
                {
                }
            }
        }

        /// <summary>
        /// Closes a channel factory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channelFactory"></param>
        public static void CloseChannelFactory<T>(ChannelFactory<T> channelFactory)
        {
            if (channelFactory == null)
                return;

            try
            {
                channelFactory.Close();
            }
            catch (Exception)
            {
                channelFactory.Abort();
            }
        }

        /// <summary>
        /// Closes a client channel
        /// </summary>
        /// <param name="clientChannel"></param>
        public static void CloseClientChannel(IClientChannel clientChannel)
        {
            if (clientChannel == null)
                return;

            try
            {
                clientChannel.Close();
            }
            catch (Exception)
            {
                clientChannel.Abort();
            }
            finally
            {
                clientChannel.Dispose();
            }
        }

        /// <summary>
        /// Determines whether a channel factory is open
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channelFactory"></param>
        /// <returns></returns>
        public static bool IsChannelFactoryOpen<T>(ChannelFactory<T> channelFactory)
        {
            return (channelFactory != null &&
                    (channelFactory.State == CommunicationState.Opened ||
                     channelFactory.State == CommunicationState.Opening));
        }

        /// <summary>
        /// Determines whether a client channel is open
        /// </summary>
        /// <param name="clientChannel"></param>
        /// <returns></returns>
        public static bool IsClientChannelOpen(IClientChannel clientChannel)
        {
            return (clientChannel != null &&
                    (clientChannel.State == CommunicationState.Opened ||
                     clientChannel.State == CommunicationState.Opening));
        }

        /// <summary>
        /// Determines whether a service host is open
        /// </summary>
        /// <param name="serviceHost"></param>
        /// <returns></returns>
        public static bool IsServiceHostOpen(System.ServiceModel.ServiceHostBase serviceHost)
        {
            return (serviceHost != null &&
                    (serviceHost.State == CommunicationState.Opened || serviceHost.State == CommunicationState.Opening));
        }

        /// <summary>
        /// Gets a free TCP port
        /// </summary>
        /// <returns></returns>
        public static int GetFreeTcpPort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint) tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }
    }
}