/*using System;
using PSOK.Kernel.Interfaces;
using PSOK.Kernel.Pipelines;

namespace PSOK.Kernel.Events
{
    /// <summary>
    /// An helper class to raise events when certain things happen in the application.
    /// </summary>
    public static class Event
    {
        /// <summary>
        /// Raises an <see cref="IEvent{T}" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        public static void RaiseEvent<T>(IEvent<T> evt) where T : PipelineArgs
        {
            if (evt == null)
                throw new ArgumentNullException("evt");

            EventQueue.RaiseEvent(evt);
        }
    }
}*/