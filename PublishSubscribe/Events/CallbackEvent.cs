using PSOK.PublishSubscribe.Pipelines;

namespace PSOK.PublishSubscribe.Events
{
    /// <summary>
    /// This event is raised whenever a callback occurs.
    /// </summary>
    internal class CallbackEvent : Event<CallbackArgs>
    {
        public override string PipelineName
        {
            get { return "callback"; }
        }
    }
}