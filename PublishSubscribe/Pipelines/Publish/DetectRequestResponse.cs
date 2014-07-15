using System.Collections.Generic;
using System.Linq;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Pipelines;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe.Pipelines.Publish
{
    /// <summary>
    /// Detects request response schemes and caches the <see cref="IPublish{T}" /> for the subscriber.
    /// </summary>
    internal class DetectRequestResponse : IProcessor<PublishArgs>
    {
        public void Execute(PublishArgs args)
        {
            IEnumerable<ISubscription> subscriptions = args.Peer.Subscriptions
                .Where(x => x.CachingOptions != null && x.CachingOptions.EnableCaching &&
                            x.CachingOptions.HasCacheKey && string.Equals(x.CachingOptions.Key, args.Publish.Key));

            IPublish<Message> request = args.Publish;

            string key = string.Format("publish#{0}#{1}",
                args.Peer.Node.Contact.NodeId,
                request.Key);

            foreach (ISubscription subscription in subscriptions)
            {
                CacheManager.Add(key, request, subscription.CachingOptions);
            }
        }
    }
}