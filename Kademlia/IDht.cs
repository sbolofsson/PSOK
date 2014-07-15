using System.Collections.Generic;
using PSOK.Kernel.Services;

namespace PSOK.Kademlia
{
    /// <summary>
    /// Defines the DHT primitives.
    /// </summary>
    public interface IDht : IServiceHost
    {
        /// <summary>
        /// The <see cref="INode" />'s contact information.
        /// </summary>
        IContact Contact { get; }

        /// <summary>
        /// Stores in the DHT that the application
        /// has the data for the specified key. 
        /// </summary>
        /// <param name="key">A unique key identifying an associated value.</param>
        /// <param name="data">Extra information to store in the DHT. Should be not be the actual data associated to the key.</param>
        void Put(string key, object data);

        /// <summary>
        /// Conducts a search in the DHT for the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>A collection of contact information about the applications' who posses the data stored under the specified key.</returns>
        IEnumerable<IContact> Get(string key);
    }
}
