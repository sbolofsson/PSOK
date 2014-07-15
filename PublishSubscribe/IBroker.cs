using System;
using PSOK.PublishSubscribe.Messages;
using PSOK.PublishSubscribe.Services;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// A peer in the P2P network.
    /// </summary>
    public interface IBroker : IDisposable
    {
        /// <summary>
        /// The <see cref="IDataContext" /> associated to the <see cref="IBroker" />.
        /// </summary>
        IDataContext DataContext { get; }

        /// <summary>
        /// Publishes an <see cref="IPublish{T}" /> to all subscribed <see cref="IBroker" />s.
        /// </summary>
        /// <param name="publish">The <see cref="IPublish{T}" /> to publish.</param>
        void Publish(IPublish<Message> publish);

        /// <summary>
        /// Publishes an <see cref="IPublish{T}" /> to all subscribed <see cref="IBroker" />s and waits for a response.
        /// Blocks until a <see cref="Message" /> is returned or the specified timeout occurs.
        /// </summary>
        /// <typeparam name="T">The expected return type.</typeparam>
        /// <param name="publish">The <see cref="IPublish{T}" /> to publish.</param>
        /// <param name="timeout">The amount of time to wait for a response.</param>
        /// <returns>
        /// A <see cref="Message" /> of type T if it was published within the specified timeout. Otherwise null is
        /// returned.
        /// </returns>
        T Publish<T>(IPublish<Message> publish, TimeSpan timeout) where T : Message;

        /// <summary>
        /// Subscribes to a certain <see cref="Message" /> type.
        /// </summary>
        /// <param name="subscription">Information about what should be subscribed to.</param>
        void Subscribe(ISubscription subscription);

        /// <summary>
        /// Retrieves a cached <see cref="Message" /> of type T that the <see cref="IBroker" /> is subscribed to.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetCachedMessage<T>() where T : Message;

        /// <summary>
        /// Retrieves a cached <see cref="Message" /> of type T based on the specified <see cref="Message" />.
        /// Use this method for request/response schemes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <returns></returns>
        T GetCachedMessage<T>(Message message) where T : Message;
    }
}