using System;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Messages.Requests
{
    /// <summary>
    /// Base class for requests.
    /// </summary>
    [Serializable, DataContract]
    public class Request : Message
    {
    }
}