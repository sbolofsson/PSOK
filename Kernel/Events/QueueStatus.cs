using System;
using System.Runtime.Serialization;

namespace PSOK.Kernel.Events
{
    /// <summary>
    /// Indicates the status of an <see cref="IEventQueue" />.
    /// </summary>
    [DataContract, Serializable]
    internal class QueueStatus : IQueueStatus
    {
        /// <summary>
        /// The name of the <see cref="IEventQueue" />.
        /// </summary>
        [DataMember]
        public string QueueName { get; set; }

        /// <summary>
        /// The size of the <see cref="IEventQueue" />.
        /// </summary>
        [DataMember]
        public int QueueSize { get; set; }
    }
}