using PSOK.Kernel.Exceptions;
using PSOK.PublishSubscribe.Pipelines;

namespace PSOK.PublishSubscribe.Exceptions
{
    /// <summary>
    /// Exception indicating an error with the <see cref="PubSubContext" />.
    /// </summary>
    internal class PubSubContextException : Exception
    {
        public PubSubContextException(string message) : base(message)
        {
        }

        public PubSubContextException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}