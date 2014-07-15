using PSOK.Kernel.Pipelines;

namespace PSOK.PublishSubscribe.Pipelines.Callback
{
    /// <summary>
    /// Initializes the <see cref="PubSubContext" /> for the current callback <see cref="Pipeline" />.
    /// </summary>
    internal class CreateContext : IProcessor<CallbackArgs>
    {
        public void Execute(CallbackArgs args)
        {
            PubSubContext.BuildContext(args.Publish);
        }
    }
}