using System.Collections.ObjectModel;

namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating that something went wrong in a <see cref="Collection{T}"/>/>.
    /// </summary>
    internal class CollectionException : Exception
    {
        public CollectionException(string message) : base(message)
        {
        }

        public CollectionException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
