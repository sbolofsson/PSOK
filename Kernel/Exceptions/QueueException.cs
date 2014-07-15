using PSOK.Kernel.Events;

namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating that an <see cref="IEventQueue" /> unexpectedly failed.
    /// </summary>
    internal class EventQueueException : Exception
    {
        public EventQueueException(string message) : base(message)
        {
        }

        public EventQueueException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}