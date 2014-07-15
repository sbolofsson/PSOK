using System;
using System.Runtime.Serialization;
using PSOK.Kernel.Reflection;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// A serializable subscription.
    /// </summary>
    [DataContract, Serializable]
    internal class SerializableSubscription : ISerializableSubscription
    {
        /// <summary>
        /// The entity set of the <see cref="SerializableSubscription" />.
        /// </summary>
        [DataMember]
        public string EntitySet { get; set; }

        /// <summary>
        /// The serialized condition.
        /// </summary>
        [DataMember]
        public string ConditionXml { get; set; }

        /// <summary>
        /// The type of the <see cref="SerializableSubscription" />.
        /// </summary>
        [DataMember]
        public string Type { get; set; }

        public ISubscription ToSubscription()
        {
            ISubscription subscription = new Subscription<Message>();
            subscription.EntitySet = EntitySet;
            subscription.ConditionXml = ConditionXml;
            subscription = subscription.MakeCopy(TypeHelper.GetType(Type));
            return subscription;
        }
    }
}
