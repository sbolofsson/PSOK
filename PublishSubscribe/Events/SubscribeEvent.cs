using PSOK.PublishSubscribe.Pipelines;

namespace PSOK.PublishSubscribe.Events
{
    /// <summary>
    /// This event is raised whenever a subscription is made.
    /// </summary>
    internal class SubscribeEvent : Event<SubscribeArgs>
    {
        public override string PipelineName
        {
            get { return "subscribe"; }
        }
    }
}