using System;
using PSOK.Kernel.Pipelines;
using log4net;

namespace PSOK.PublishSubscribe.Pipelines.Subscribe
{
    /// <summary>
    /// Stores an <see cref="ISubscription" /> remotely.
    /// </summary>
    internal class StoreSubscription : IProcessor<SubscribeArgs>
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (StoreSubscription));

        public void Execute(SubscribeArgs args)
        {
            try
            {
                args.Peer.Node.Put(args.Subscription.Key,
                    args.Subscription.MakeSerializable());
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}