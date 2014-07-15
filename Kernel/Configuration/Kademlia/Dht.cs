using System.Configuration;

namespace PSOK.Kernel.Configuration.Kademlia
{
    /// <summary>
    /// DHT settings.
    /// </summary>
    public class Dht : System.Configuration.ConfigurationElement
    {
        /// <summary>
        /// The expiration of items stored in the DHT.
        /// </summary>
        [ConfigurationProperty("expiration", IsRequired = true)]
        public string Expiration
        {
            get { return (string) this["expiration"]; }
        }
    }
}