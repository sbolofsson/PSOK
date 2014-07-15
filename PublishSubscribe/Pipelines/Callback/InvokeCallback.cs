using PSOK.Kernel.Pipelines;

namespace PSOK.PublishSubscribe.Pipelines.Callback
{
    /// <summary>
    /// Invokes the callback handler in the consuming application.
    /// </summary>
    internal class InvokeCallback : IProcessor<CallbackArgs>
    {
        public void Execute(CallbackArgs args)
        {
            ISubscription subscription = args.Peer.GetSubscription(args.Publish.Key);
            if (subscription != null)
                subscription.InvokeCallback(args.Publish.Message);
        }
    }
}