namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating something went wrong when working certificates.
    /// </summary>
    public class CertificateException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="CertificateException"/> using the specified message.
        /// </summary>
        /// <param name="message"></param>
        public CertificateException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="CertificateException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public CertificateException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}