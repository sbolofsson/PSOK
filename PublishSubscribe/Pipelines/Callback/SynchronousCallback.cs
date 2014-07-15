using System;
using System.Threading;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Pipelines;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe.Pipelines.Callback
{
    /// <summary>
    /// Ensures that a <see cref="Message" /> is available for synchronous <see cref="IPublish{T}" />es.
    /// </summary>
    internal class SynchronousCallback : IProcessor<CallbackArgs>
    {
        public void Execute(CallbackArgs args)
        {
            string callerId = args.Publish.Headers.Get("callerId");

            if (string.IsNullOrEmpty(callerId))
                return;

            string responseKey = args.Publish.Key;
            string cacheKeyHandle = string.Format("publish#{0}#{1}#handle", responseKey, callerId);
            string cacheKeyMessage = string.Format("publish#{0}#{1}#message", responseKey, callerId);

            SemaphoreSlim handle = CacheManager.Get<SemaphoreSlim>(cacheKeyHandle);

            if (handle == null)
                return;

            CacheManager.Add(cacheKeyMessage, args.Publish.Message, new CachingOptions<Message>
            {
                EnableCaching = true,
                Expiration = new TimeSpan(1, 0, 0)
            });

            handle.Release();

            CacheManager.Remove(cacheKeyHandle);
        }
    }
}