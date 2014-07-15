using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kernel.Configuration;
using PSOK.Kernel.Exceptions;
using log4net;
using Exception = System.Exception;

namespace PSOK.Kernel.Pipelines
{
    /// <summary>
    /// An ordered collection of <see cref="Processor" />s.
    /// </summary>
    internal class Pipeline
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof (Pipeline));

        private static readonly ConcurrentDictionary<string, Lazy<Pipeline>> Pipelines =
            new ConcurrentDictionary<string, Lazy<Pipeline>>();

        // Instance fields
        private readonly Lazy<IEnumerable<IProcessor<PipelineArgs>>> _processors;

        private readonly string _name;

        /// <summary>
        /// Constructs a new <see cref="Pipeline" /> with the given name.
        /// </summary>
        /// <param name="name"></param>
        private Pipeline(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            _name = name;
            _processors = new Lazy<IEnumerable<IProcessor<PipelineArgs>>>(GetProcessors,
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// The name of the <see cref="Pipeline" />.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// The ordered <see cref="IProcessor{T}" />s.
        /// </summary>
        public IEnumerable<IProcessor<PipelineArgs>> Processors
        {
            get { return _processors.Value; }
        }

        /// <summary>
        /// Retrieves a <see cref="Pipeline" /> with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Pipeline GetPipeline(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(name);

            return
                Pipelines.GetOrAdd(name,
                    x => new Lazy<Pipeline>(() => new Pipeline(x), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
        }

        /// <summary>
        /// Invokes the pipeline and all its <see cref="IProcessor{T}" />s.
        /// </summary>
        /// <param name="args"></param>
        public void Invoke(PipelineArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            foreach (IProcessor<PipelineArgs> processor in Processors)
            {
                try
                {
                    processor.Execute(args);
                }
                catch (PipelineAbortedException ex)
                {
                    Log.InfoFormat("Pipeline '{0}' was aborted by processor. Message: {1}.", Name, ex.Message);
                    break;
                }
                catch (Exception ex)
                {
                    Log.Error(
                        string.Format("A Processor unexpectedly failed to execute."), ex);
                }
            }
        }

        /// <summary>
        /// Retrieves the <see cref="IProcessor{T}" />s of the <see cref="Pipeline" />.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IProcessor<PipelineArgs>> GetProcessors()
        {
            try
            {
                Config config = Config.ReadConfig();
                Configuration.Pipelines.Pipeline pipeline = config.Pipelines
                    .FirstOrDefault(x => string.Equals(x.Name, Name, StringComparison.InvariantCultureIgnoreCase));

                if (pipeline == null)
                    return new List<IProcessor<PipelineArgs>>();

                return pipeline.Select(x => Processor.GetProcessor(x.Type)).ToList();
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Could not get Processors for Pipeline '{0}'.", Name), ex);
                throw;
            }
        }
    }
}