using System;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// A publish for communicating in the P2P network.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPublish<out T> where T : Message
    {
        /// <summary>
        /// The message to publish.
        /// </summary>
        T Message { get; }

        /// <summary>
        /// Headers containing metadata for the <see cref="IPublish{T}" />.
        /// </summary>
        IHeaders Headers { get; }

        /// <summary>
        /// The type of the <see cref="IPublish{T}" />.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// A unique key identifying the <see cref="IPublish{T}" />.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Specifies that if the <see cref="IPublish{T}" /> has failed to be delivered to a subscriber, it should be republished
        /// at a later point in time.
        /// </summary>
        bool Republish { get; }
    }
}