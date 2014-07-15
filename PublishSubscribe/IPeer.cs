using System.Collections.Generic;
using PSOK.Kademlia;
using PSOK.PublishSubscribe.Reports;
using PSOK.PublishSubscribe.Services;

namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// Provides a layer between the consuming application and the Kademlia network.
    /// </summary>
    public interface IPeer : IPeerServiceHost, IBroker
    {
        /// <summary>
        /// Provides access to the underlying DHT.
        /// </summary>
        IDht Node { get; }

        /// <summary>
        /// The <see cref="ISubscription" />s of the <see cref="IPeer" />.
        /// </summary>
        IEnumerable<ISubscription> Subscriptions { get; }

        /// <summary>
        /// Retrieves an <see cref="ISubscription" /> based on the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        ISubscription GetSubscription(string key);

        /// <summary>
        /// Indicates the status of the <see cref="IPeer" />.
        /// </summary>
        /// <returns></returns>
        IBrokerStatus GetStatus();
    }
}