using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace PSOK.Kademlia.Services
{
    /// <summary>
    /// This custom behavior class is used to add both client and server inspectors to
    /// the corresponding WCF end points.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    internal class Behavior : Attribute, IServiceBehavior, IEndpointBehavior
    {
        private readonly INode _owner;

        public Behavior(INode owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint,
            BindingParameterCollection bindingParameterCollection)
        {
        }

        /// <summary>
        /// Applies client behaviour to the client runtime
        /// </summary>
        /// <param name="serviceEndpoint"></param>
        /// <param name="clientRuntime"></param>
        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new MessageInspector(_owner));
        }

        /// <summary>
        /// Applies dispatch behavior to the dispatch runtime
        /// </summary>
        /// <param name="serviceEndpoint"></param>
        /// <param name="endpointDispatcher"></param>
        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint,
            EndpointDispatcher endpointDispatcher)
        {
            ChannelDispatcher channelDispatcher = endpointDispatcher.ChannelDispatcher;

            if (channelDispatcher == null)
                return;

            foreach (EndpointDispatcher endPoint in channelDispatcher.Endpoints)
            {
                endPoint.DispatchRuntime.MessageInspectors.Add(new MessageInspector(_owner));
            }
        }

        void IEndpointBehavior.Validate(ServiceEndpoint endpoint)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints,
            BindingParameterCollection bindingParameterCollection)
        {
        }

        /// <summary>
        /// Applies dispatch behavior to the dispatch runtime
        /// </summary>
        /// <param name="serviceDescription"></param>
        /// <param name="serviceHostBase"></param>
        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription serviceDescription,
            ServiceHostBase serviceHostBase)
        {
            foreach (
                EndpointDispatcher endpointDispatcher in
                    serviceHostBase.ChannelDispatchers.Cast<ChannelDispatcher>().SelectMany(x => x.Endpoints))
            {
                endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new MessageInspector(_owner));
            }
        }

        void IServiceBehavior.Validate(ServiceDescription desc, ServiceHostBase host)
        {
        }
    }
}