using System.Data.Entity;
using System.Data.Services;
using PSOK.Kernel.Exceptions;

namespace PSOK.PublishSubscribe.Exceptions
{
    /// <summary>
    /// Exception indicating something went wrong when registering a <see cref="DataService{T}" /> or <see cref="DbContext" />.
    /// </summary>
    internal class DataContextException : Exception
    {
        public DataContextException(string message) : base(message)
        {
        }

        public DataContextException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}