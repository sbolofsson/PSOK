using PSOK.Kernel.Pipelines;

namespace PSOK.Kernel.Events
{
    /// <summary>
    /// Defines an event.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEvent<out T> where T : PipelineArgs
    {
        /// <summary>
        /// The name of the <see cref="Pipeline" /> to be invoked when the <see cref="IEvent{T}" /> is raised.
        /// </summary>
        string PipelineName { get; }

        /// <summary>
        /// The arguments to pass on to the <see cref="Pipeline" />.
        /// </summary>
        T PipelineArgs { get; }
    }
}