using PSOK.Kernel.Tasks;

namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating that something went wrong when trying to execute an <see cref="Agent" />.
    /// </summary>
    internal class AgentException : Exception
    {
        public AgentException(string message) : base(message)
        {
        }

        public AgentException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}