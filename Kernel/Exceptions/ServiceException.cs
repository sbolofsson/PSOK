namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating something went wrong in an WCF service or operation.
    /// </summary>
    public class ServiceException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="ServiceException"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public ServiceException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ServiceException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ServiceException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}