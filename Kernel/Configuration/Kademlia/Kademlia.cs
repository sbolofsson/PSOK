using System.Configuration;

namespace PSOK.Kernel.Configuration.Kademlia
{
    /// <summary>
    /// Options controlling the behavior of the Kademlia implementation.
    /// </summary>
    public class Kademlia : System.Configuration.ConfigurationElement
    {
        /// <summary>
        /// The number of contacts a bucket can contain.
        /// </summary>
        [ConfigurationProperty("bucketsize", IsRequired = true)]
        public int Bucketsize
        {
            get { return (int) this["bucketsize"]; }
        }

        /// <summary>
        /// The number of buckets owned by a node.
        /// </summary>
        [ConfigurationProperty("buckets", IsRequired = true)]
        public int Buckets
        {
            get { return (int) this["buckets"]; }
        }

        /// <summary>
        /// The Kademlia concurrency parameter (alpha).
        /// </summary>
        [ConfigurationProperty("concurrency", IsRequired = true)]
        public int Concurrency
        {
            get { return (int) this["concurrency"]; }
        }

        /// <summary>
        /// The Kademlia parallelism type. Can be either 'strict', 'bounded' or 'loose'.
        /// </summary>
        [ConfigurationProperty("parallelism", IsRequired = true)]
        public string Parallelism
        {
            get { return (string) this["parallelism"]; }
        }

        /// <summary>
        /// The node id to use for the application.
        /// </summary>
        [ConfigurationProperty("nodeid", IsRequired = false)]
        public string NodeId
        {
            get { return (string)this["nodeid"]; }
        }

        /// <summary>
        /// DHT settings.
        /// </summary>
        [ConfigurationProperty("dht", IsRequired = true)]
        public Dht Dht
        {
            get { return this["dht"] as Dht; }
        }

        /// <summary>
        /// Bootstrap settings.
        /// </summary>
        [ConfigurationProperty("bootstrap", IsRequired = true)]
        public Bootstrap Bootstrap
        {
            get { return this["bootstrap"] as Bootstrap; }
        }
    }
}