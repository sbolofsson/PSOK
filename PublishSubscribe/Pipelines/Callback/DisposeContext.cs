using PSOK.Kernel.Pipelines;

namespace PSOK.PublishSubscribe.Pipelines.Callback
{
    internal class DisposeContext : IProcessor<PipelineArgs>
    {
        public void Execute(PipelineArgs args)
        {
            PubSubContext.Dispose();
        }
    }
}