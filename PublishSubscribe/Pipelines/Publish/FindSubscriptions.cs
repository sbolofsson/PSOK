using System.Collections.Generic;
using System.Linq;
using PSOK.Kademlia;
using PSOK.Kernel.Pipelines;
using PSOK.Kernel.Reflection;

namespace PSOK.PublishSubscribe.Pipelines.Publish
{
    /// <summary>
    /// Finds relevant <see cref="ISubscription" />s based on the specified <see cref="IPublish{T}" />.
    /// </summary>
    internal class FindSubscriptions : IProcessor<PublishArgs>
    {
        public void Execute(PublishArgs args)
        {
            IEnumerable<IContact> subscriptions = args.Peer.Node.Get(args.Publish.Key);

            if (subscriptions == null)
            {
                args.AbortPipeline(string.Format("Found no subscribers for '{0}'", args.Publish.Type.AssemblyQualifiedName()));
                return;
            }

            IEnumerable<IContact> existingSubscriptions = args.Subscribers;

            args.Subscribers = existingSubscriptions == null ? subscriptions : existingSubscriptions.Intersect(subscriptions);

            args.Subscribers = args.Subscribers.Where(x => x.ConditionHolds(args.Publish.Message)).ToList();

            if (!args.Subscribers.Any())
            {
                args.AbortPipeline(string.Format("Found no subscribers for '{0}'.", args.Publish.Type.AssemblyQualifiedName()));
            }
        }
    }
}