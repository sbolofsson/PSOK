using PSOK.Kernel.Caching;
using PSOK.Kernel.Pipelines;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe.Pipelines.Callback
{
    /// <summary>
    /// Caches a message for request/response schemes
    /// </summary>
    internal class CacheMessage : IProcessor<CallbackArgs>
    {
        public void Execute(CallbackArgs args)
        {
            IPublish<Message> response = args.Publish;
            ISubscription subscription = args.Peer.GetSubscription(response.Key);

            if (subscription == null)
                return;

            ICachingOptions cachingOptions = subscription.CachingOptions;

            if (cachingOptions == null || !cachingOptions.EnableCaching)
                return;

            if (!cachingOptions.HasCacheKey)
            {
                return;
            }

            string callbackKey = string.Format("callback#{0}#{1}", args.Peer.Node.Contact.NodeId, response.Key);

            string cacheKey = callbackKey;

            if (string.Equals(cachingOptions.Key, response.Key))
            {
                string extraKey = cachingOptions.GetCacheKey(response.Message);

                if (!string.IsNullOrEmpty(extraKey))
                {
                    cacheKey = string.Format("{0}#{1}", callbackKey, extraKey);
                }
            }

            CacheManager.Add(cacheKey, args.Publish.Message, cachingOptions);

            // Request response scheme
            if (string.Equals(subscription.Key, cachingOptions.Key))
                return;

            IPublish<Message> request =
                CacheManager.Get<IPublish<Message>>(string.Format("publish#{0}#{1}", args.Peer.Node.Contact.NodeId,
                    cachingOptions.Key));

            if (request != null && string.Equals(request.Key, cachingOptions.Key))
            {
                string extraKey = cachingOptions.GetCacheKey(request.Message);
                if (!string.IsNullOrEmpty(extraKey))
                {
                    cacheKey = string.Format("{0}#{1}", callbackKey, extraKey);
                    CacheManager.Add(cacheKey, args.Publish.Message, cachingOptions);
                }
            }
        }
    }
}