using System;
using PSOK.Kernel.Pipelines;

namespace PSOK.Kernel.Events
{
    /// <summary>
    /// Interface defining an event queue.
    /// </summary>
    public interface IEventQueue
    {
        /// <summary>
        /// Processes an <see cref="IEvent{T}" />.
        /// Blocks until there is an <see cref="IEvent{T}" /> to process.
        /// </summary>
        /// <returns></returns>
        IEvent<PipelineArgs> ProcessEvent();

        /// <summary>
        /// Tries to process an <see cref="IEvent{T}" /> within a specified timeout.
        /// Blocks until the timeout occurs or there is an <see cref="IEvent{T}" /> to process.
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool TryProcessEvent(out IEvent<PipelineArgs> evt, TimeSpan timeout);

        /// <summary>
        /// Adds an <see cref="IEvent{T}" /> to the <see cref="IEventQueue" />.
        /// </summary>
        /// <param name="evt"></param>
        void AddEvent(IEvent<PipelineArgs> evt);

        /// <summary>
        /// Retrieves the <see cref="IQueueStatus" /> of the <see cref="IEventQueue" />.
        /// </summary>
        /// <returns></returns>
        IQueueStatus GetStatus();
    }
}