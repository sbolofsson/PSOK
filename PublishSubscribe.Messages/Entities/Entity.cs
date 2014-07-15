using System;
using System.Data.Services.Common;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Messages.Entities
{
    /// <summary>
    /// Base class for entities.
    /// </summary>
    [DataServiceKey("Id"), Serializable, DataContract]
    public class Entity : Message
    {
    }
}