namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating something went wrong when reading the configuration file.
    /// </summary>
    public class ConfigurationException : Exception
    {
        /// <summary>
        /// Constructs a new <see cref="ConfigurationException"/> using the specified message and.
        /// </summary>
        /// <param name="message"></param>
        public ConfigurationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructs a new <see cref="ConfigurationException"/> using the specified message and inner exception.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        public ConfigurationException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}