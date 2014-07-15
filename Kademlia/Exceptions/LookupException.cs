using PSOK.Kernel.Exceptions;

namespace PSOK.Kademlia.Exceptions
{
    /// <summary>
    /// Exception indicating something went wrong during a lookup.
    /// </summary>
    internal class LookupException : Exception
    {
        public LookupException(string message) : base(message)
        {
        }

        public LookupException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}