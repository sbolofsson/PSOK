using PSOK.PublishSubscribe.Pipelines;

namespace PSOK.PublishSubscribe.Events
{
    /// <summary>
    /// This event is raised whenever a message is published.
    /// </summary>
    internal class PublishEvent : Event<PublishArgs>
    {
        public override string PipelineName
        {
            get { return "publish"; }
        }
    }
}