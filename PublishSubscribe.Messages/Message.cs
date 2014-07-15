using System;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Messages
{
    /// <summary>
    /// Base class for all messages.
    /// </summary>
    [Serializable, DataContract]
    public class Message
    {
        /// <summary>
        /// Indicates the <see cref="Type" /> of the <see cref="Message" />.
        /// </summary>
        /// <returns></returns>
        public virtual Type GetMessageType()
        {
            return GetType();
        }
    }
}