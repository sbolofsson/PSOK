using System;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Messages.Responses
{
    /// <summary>
    /// Base class for responses.
    /// </summary>
    [Serializable, DataContract]
    public class Response : Message
    {
    }
}