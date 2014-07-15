using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kernel.Pipelines;

namespace PSOK.Kernel.Events
{
    /// <summary>
    /// A queue containing events to be processed.
    /// </summary>
    public class EventQueue : IEventQueue
    {
        private static readonly ConcurrentDictionary<string, Lazy<IEventQueue>> EventQueues =
            new ConcurrentDictionary<string, Lazy<IEventQueue>>();

        private readonly BlockingCollection<IEvent<PipelineArgs>> _queue =
            new BlockingCollection<IEvent<PipelineArgs>>(new ConcurrentQueue<IEvent<PipelineArgs>>());

        private readonly string _name;

        /// <summary>
        /// Constructs a new <see cref="EventQueue" />.
        /// </summary>
        /// <param name="name"></param>
        private EventQueue(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            _name = name;
        }

        /// <summary>
        /// The name of the <see cref="EventQueue" />.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        #region Static methods

        /// <summary>
        /// Creates a new <see cref="IEventQueue" /> with the specified name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static IEventQueue GetQueue(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            return
                EventQueues.GetOrAdd(name,
                    x => new Lazy<IEventQueue>(() => new EventQueue(x), LazyThreadSafetyMode.ExecutionAndPublication))
                    .Value;
        }

        /// <summary>
        /// Raises an <see cref="IEvent{T}" /> and puts it onto the relevant <see cref="IEventQueue" />.
        /// </summary>
        /// <param name="evt"></param>
        public static void RaiseEvent(IEvent<PipelineArgs> evt)
        {
            if (evt == null)
                throw new ArgumentNullException("evt");

            IEventQueue queue = GetQueue(evt.PipelineName);
            queue.AddEvent(evt);
        }

        /// <summary>
        /// Retrieves the status of all <see cref="EventQueue"/>s in the application.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IQueueStatus> GetStatuses()
        {
            return EventQueues.Values.Select(x => x.Value.GetStatus()).ToList();
        }

        #endregion

        #region IEventQueue methods

        /// <summary>
        /// Processes an <see cref="IEvent{T}" />.
        /// Blocks until there is an <see cref="IEvent{T}" /> to process.
        /// </summary>
        /// <returns></returns>
        IEvent<PipelineArgs> IEventQueue.ProcessEvent()
        {
            return _queue.Take();
        }

        /// <summary>
        /// Tries to process an <see cref="IEvent{T}" /> within a specified timeout.
        /// Blocks until the timeout occurs or there is an <see cref="IEvent{T}" /> to process.
        /// </summary>
        /// <param name="evt">The <see cref="IEvent{T}" /> that is retrieved.</param>
        /// <param name="timeout">The time to wait before a timeout should occur.</param>
        /// <returns>
        /// A success indicator indicating if there was an <see cref="IEvent{T}" /> to process within the specified
        /// timeout.
        /// </returns>
        bool IEventQueue.TryProcessEvent(out IEvent<PipelineArgs> evt, TimeSpan timeout)
        {
            if (timeout == null)
                throw new ArgumentNullException("timeout");

            return _queue.TryTake(out evt, timeout);
        }

        /// <summary>
        /// Adds an <see cref="IEvent{T}" /> to the <see cref="IEventQueue" />.
        /// </summary>
        /// <param name="evt"></param>
        void IEventQueue.AddEvent(IEvent<PipelineArgs> evt)
        {
            if (evt == null)
                throw new ArgumentNullException("evt");

            _queue.Add(evt);
        }

        /// <summary>
        /// Retrieves the <see cref="IQueueStatus" /> of the <see cref="IEventQueue" />.
        /// </summary>
        /// <returns></returns>
        IQueueStatus IEventQueue.GetStatus()
        {
            return new QueueStatus
            {
                QueueName = _name,
                QueueSize = _queue.Count
            };
        }

        #endregion
    }
}