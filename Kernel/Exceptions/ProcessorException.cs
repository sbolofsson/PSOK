using PSOK.Kernel.Pipelines;

namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating that a <see cref="Processor" /> unexpectedly failed.
    /// </summary>
    public class ProcessorException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="ProcessorException"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public ProcessorException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ProcessorException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ProcessorException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}