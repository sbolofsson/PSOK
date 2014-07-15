using System;
using PSOK.Kernel.Events;
using PSOK.Kernel.Pipelines;
using log4net;

namespace PSOK.Kernel.Threads
{
    /// <summary>
    /// A worker class for processing <see cref="IEvent{T}" />s from a single <see cref="IEventQueue" />.
    /// </summary>
    internal class WorkerThread : Thread
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof (WorkerThread));

        // Instance fields
        private readonly IEventQueue _eventQueue;
        private volatile bool _stop;

        /// <summary>
        /// Constructs a new <see cref="WorkerThread" />.
        /// </summary>
        /// <param name="eventQueue"></param>
        public WorkerThread(IEventQueue eventQueue)
        {
            if (eventQueue == null)
                throw new ArgumentNullException("eventQueue");

            _eventQueue = eventQueue;

            Application.OnExit += () => _stop = true;
        }

        /// <summary>
        /// Processes an <see cref="IEvent{T}" /> from the <see cref="IEventQueue" /> associated to the <see cref="WorkerThread" />.
        /// </summary>
        protected override void Run()
        {
            while (!_stop)
            {
                try
                {
                    IEvent<PipelineArgs> evt = _eventQueue.ProcessEvent();
                    Pipeline pipeline = Pipeline.GetPipeline(evt.PipelineName);
                    pipeline.Invoke(evt.PipelineArgs);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
    }
}