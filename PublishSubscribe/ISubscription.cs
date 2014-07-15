using System;
using PSOK.Kernel.Caching;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// A subscription for communicating in the P2P network.
    /// </summary>
    public interface ISubscription
    {
        /// <summary>
        /// Indicates options on how to cache messages matching the <see cref="ISubscription" />.
        /// </summary>
        ICachingOptions CachingOptions { get; set; }

        /// <summary>
        /// The type of the <see cref="ISubscription" />.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// A unique key identifying the <see cref="ISubscription" />.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// The entity set of the <see cref="ISubscription" />.
        /// </summary>
        string EntitySet { get; set; }
        
        /// <summary>
        /// Specifies whether subclasses of the type T should also be subscribed to.
        /// </summary>
        bool IncludeSubclasses { get; set; }

        /// <summary>
        /// Checks whether the condition of the <see cref="ISubscription" /> holds.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ConditionHolds(Message message);

        /// <summary>
        /// The serialized condition.
        /// </summary>
        string ConditionXml { get; set; }

        /// <summary>
        /// Invokes the callback of the <see cref="ISubscription" /> if the condition holds.
        /// If the is no condition the callback will just be invoked.
        /// </summary>
        /// <param name="message"></param>
        void InvokeCallback(Message message);

        /// <summary>
        /// Creates a copy of the <see cref="ISubscription" /> based on the given type
        /// </summary>
        /// <param name="type">The type to use as the foundation for creating a copy.</param>
        /// <returns></returns>
        ISubscription MakeCopy(Type type);

        /// <summary>
        /// Creates a serializable copy of the <see cref="Subscription{T}"/>.
        /// </summary>
        /// <returns></returns>
        ISerializableSubscription MakeSerializable();
    }
}