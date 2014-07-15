using PSOK.Kernel.Events;
using PSOK.Kernel.Pipelines;

namespace PSOK.PublishSubscribe.Events
{
    /// <summary>
    /// Defines an event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class Event<T> : IEvent<T> where T : PipelineArgs
    {
        /// <summary>
        /// The name of the <see cref="Pipeline" /> to be invoked when the <see cref="Event{T}" /> is raised.
        /// </summary>
        public abstract string PipelineName { get; }

        /// <summary>
        /// The arguments to pass on to the <see cref="Pipeline" />.
        /// </summary>
        public T PipelineArgs { get; set; }
    }
}
