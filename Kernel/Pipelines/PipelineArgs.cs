using System;
using PSOK.Kernel.Exceptions;

namespace PSOK.Kernel.Pipelines
{
    /// <summary>
    /// Arguments for a <see cref="Pipeline" />.
    /// </summary>
    public abstract class PipelineArgs : EventArgs
    {
        /// <summary>
        /// Aborts the current <see cref="Pipeline" />.
        /// </summary>
        public void AbortPipeline()
        {
            AbortPipeline("Pipeline was aborted.");
        }

        /// <summary>
        /// Aborts the current <see cref="Pipeline" /> with a specified message.
        /// </summary>
        /// <param name="message">The reason for aborting the <see cref="Pipeline" />.</param>
        public void AbortPipeline(string message)
        {
            throw new PipelineAbortedException(message);
        }
    }
}