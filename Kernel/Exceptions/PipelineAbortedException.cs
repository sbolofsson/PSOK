using PSOK.Kernel.Pipelines;

namespace PSOK.Kernel.Exceptions
{
    /// <summary>
    /// Exception indicating that a <see cref="Pipeline" /> was deliberately aborted.
    /// </summary>
    internal class PipelineAbortedException : Exception
    {
        public PipelineAbortedException(string message) : base(message)
        {
        }

        public PipelineAbortedException(string message, System.Exception innerException)
            : base(message, innerException)
        {
        }
    }
}