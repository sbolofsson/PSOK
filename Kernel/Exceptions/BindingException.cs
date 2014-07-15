namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating something went wrong when working with an IIS binding.
    /// </summary>
    public class BindingException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="BindingException"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public BindingException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="BindingException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public BindingException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}