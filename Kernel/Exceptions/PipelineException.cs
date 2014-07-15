using PSOK.Kernel.Pipelines;

namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating that a <see cref="Pipeline" /> was unexpectedly aborted.
    /// </summary>
    internal class PipelineException : Exception
    {
        public PipelineException(string message) : base(message)
        {
        }

        public PipelineException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}