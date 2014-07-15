using PSOK.Kernel.Threads;

namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating something went wrong in a <see cref="Thread"/>.
    /// </summary>
    public class ThreadException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="ThreadException"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public ThreadException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ThreadException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ThreadException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
