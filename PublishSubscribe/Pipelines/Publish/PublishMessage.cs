using System;
using System.Collections.Generic;
using System.Linq;
using PSOK.Kademlia;
using PSOK.Kernel.Exceptions;
using PSOK.Kernel.Pipelines;
using PSOK.Kernel.Reflection;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Events;
using PSOK.PublishSubscribe.Messages;
using PSOK.PublishSubscribe.Messages.Requests.DataRequest;
using PSOK.PublishSubscribe.Services;
using PSOK.PublishSubscribe.Tasks;
using log4net;
using Exception = System.Exception;

// ReSharper disable LoopCanBeConvertedToQuery

namespace PSOK.PublishSubscribe.Pipelines.Publish
{
    /// <summary>
    /// Publishes a <see cref="Message" />.
    /// </summary>
    internal class PublishMessage : IProcessor<PublishArgs>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (PublishMessage));

        public void Execute(PublishArgs args)
        {
            List<IContact> undeliveredSubscriptions = new List<IContact>();
            foreach (IContact subscriber in args.Subscribers)
            {
                bool success = TypeHelper.IsSubclassOf(args.Publish.Type, typeof(DataRequest))
                    ? ExecuteDataRequest(args, subscriber)
                    : ExecuteCallback(args, subscriber);

                if (!success)
                    undeliveredSubscriptions.Add(subscriber);
            }

            int failureCount = undeliveredSubscriptions.Count();
            if (failureCount == 0)
                return;

            args.Subscribers = undeliveredSubscriptions;
            Republish.Enqueue(new PublishEvent
            {
                PipelineArgs = args
            });
        }

        /// <summary>
        /// Executes a normal callback.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        private bool ExecuteCallback(PublishArgs args, IContact subscriber)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (subscriber == null)
                throw new ArgumentNullException("subscriber");

            try
            {
                using (ServiceProxy<IPeerServiceHost> proxy = new ServiceProxy<IPeerServiceHost>(subscriber.PeerUrl()))
                {
                    proxy.Context.Callback(args.Publish);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(string.Format("Could not deliver message of type '{0}' to '{1}'. The subscriber application may be temporarily down.",
                    args.Publish.Type.AssemblyQualifiedName(),
                    subscriber.NodeId));
                if (!Kernel.Exceptions.Exception.IsEndpointDown(ex))
                    Log.Error(ex);
            }

            return false;
        }

        /// <summary>
        /// Executes a database request.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        private bool ExecuteDataRequest(PublishArgs args, IContact subscriber)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (subscriber == null)
                throw new ArgumentNullException("subscriber");

            try
            {
                DataRequest request = args.Publish.Message as DataRequest;

                if (request == null)
                    throw new ProcessorException(
                        string.Format(
                            "Cannot execute data request because the published message is not of type '{0}'.",
                            typeof(DataRequest).AssemblyQualifiedName()));

                ISubscription subscription = subscriber.ToSubscription();

                Message response = request.Execute(new DataRequestParameters
                {
                    BaseUrl = subscriber.DataContextUrl(),
                    EntitySet = subscription.EntitySet,
                    Headers = args.Publish.Headers.AsDictionary()
                });

                if (response != null)
                {
                    args.Peer.Callback(new Publish<Message>
                    {
                        Message = response
                    });
                }

                return true;
            }
            catch (Exception ex)
            {
                Log.Warn(string.Format("Could not deliver message to: '{0}'", subscriber.NodeId));
                if (!Kernel.Exceptions.Exception.IsEndpointDown(ex))
                    Log.Error(ex);
            }

            return false;
        }
    }
}

// ReSharper restore LoopCanBeConvertedToQuery