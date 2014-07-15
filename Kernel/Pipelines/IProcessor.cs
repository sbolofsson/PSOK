namespace PSOK.Kernel.Pipelines
{
    /// <summary>
    /// Defines a processor in a <see cref="Pipeline" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IProcessor<in T> where T : PipelineArgs
    {
        /// <summary>
        /// Executes the <see cref="IProcessor{T}" /> with the specified <see cref="PipelineArgs" />.
        /// </summary>
        /// <param name="args"></param>
        void Execute(T args);
    }
}