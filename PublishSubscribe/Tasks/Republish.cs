using System;
using PSOK.Kernel.Events;
using PSOK.Kernel.Pipelines;
using PSOK.PublishSubscribe.Events;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe.Tasks
{
    /// <summary>
    /// Ensures that <see cref="Message" />s which could not be delivered due to a non-responding subscriber
    /// are republished to those specific subscribers.
    /// </summary>
    internal class Republish
    {
        private static readonly IEventQueue EventQueue = Kernel.Events.EventQueue.GetQueue("republish");

        public void Run()
        {
            // If the event queue has not yet been initialized, skip the current run.
            if (EventQueue == null)
                return;

            IEvent<PipelineArgs> evt;

            while (EventQueue.TryProcessEvent(out evt, new TimeSpan(0)))
            {
                Kernel.Events.EventQueue.RaiseEvent(evt);
            }
        }

        /// <summary>
        /// Enqueues an <see cref="PublishEvent" /> for a later republish regardless of the value of
        /// <see cref="IPublish{T}.Republish" />.
        /// </summary>
        /// <param name="publishEvent">The <see cref="PublishEvent" /> to republish.</param>
        public static void ForceEnqueue(PublishEvent publishEvent)
        {
            if (publishEvent == null)
                throw new ArgumentNullException("publishEvent");

            EventQueue.AddEvent(publishEvent);
        }

        /// <summary>
        /// Enqueues the specified <see cref="PublishEvent" /> for a later republish but only if
        /// <see cref="IPublish{T}.Republish" /> is True.
        /// </summary>
        /// <param name="publishEvent">The <see cref="PublishEvent" /> to republish.</param>
        public static void Enqueue(PublishEvent publishEvent)
        {
            if (publishEvent == null)
                throw new ArgumentNullException("publishEvent");

            if (!publishEvent.PipelineArgs.Publish.Republish)
                return;

            EventQueue.AddEvent(publishEvent);
        }
    }
}