namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating something went wrong when using reflection.
    /// </summary>
    public class ReflectionException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="ReflectionException"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public ReflectionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ReflectionException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ReflectionException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}