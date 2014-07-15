namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Indicates something went wrong while working with the server.
    /// </summary>
    public class ServerManagerException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="ServerManagerException"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public ServerManagerException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ServerManagerException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ServerManagerException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
