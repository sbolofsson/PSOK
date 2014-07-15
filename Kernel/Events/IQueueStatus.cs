namespace PSOK.Kernel.Events
{
    /// <summary>
    /// Indicates the status of a queue.
    /// </summary>
    public interface IQueueStatus
    {
        /// <summary>
        /// The name of the queue.
        /// </summary>
        string QueueName { get; }

        /// <summary>
        /// The size of the queue.
        /// </summary>
        int QueueSize { get; }
    }
}