using System;
using System.Runtime.Serialization;
using PSOK.Kernel.Reflection;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// A publish for communicating in the P2P network.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable, DataContract]
    public class Publish<T> : IPublish<T> where T : Message
    {
        /// <summary>
        /// Constructs a new <see cref="Publish{T}" />.
        /// </summary>
        public Publish()
        {
            Headers = new Headers();
        }

        /// <summary>
        /// The message to publish.
        /// </summary>
        [DataMember]
        public T Message { get; set; }

        /// <summary>
        /// Headers containing metadata for the <see cref="Publish{T}" />.
        /// </summary>
        [DataMember]
        public IHeaders Headers { get; private set; }

        /// <summary>
        /// The type of the <see cref="Publish{T}" />.
        /// </summary>
        [IgnoreDataMember]
        Type IPublish<T>.Type
        {
            get { return Message != null ? Message.GetMessageType() : typeof (T); }
        }

        /// <summary>
        /// A unique key identifying the <see cref="Publish{T}" />.
        /// </summary>
        [IgnoreDataMember]
        string IPublish<T>.Key
        {
            get { return (this as IPublish<T>).Type.Key(); }
        }

        /// <summary>
        /// Specifies that if the <see cref="Publish{T}" /> has failed to be delivered to a subscriber, it should be republished at
        /// a later point in time.
        /// </summary>
        [DataMember]
        public bool Republish { get; set; }
    }
}