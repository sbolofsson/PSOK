using System;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Messages.Events
{
    /// <summary>
    /// Base class for events.
    /// </summary>
    [Serializable, DataContract]
    public class Event : Message
    {
    }
}