using PSOK.Kernel.Exceptions;

namespace PSOK.Kademlia.Bootstrapper.Exceptions
{
    /// <summary>
    /// Indicates something went wrong in relation to the <see cref="Bootstrapper"/>.
    /// </summary>
    public class BootstrapperException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="BootstrapperException"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public BootstrapperException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BootstrapperException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public BootstrapperException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}