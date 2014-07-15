using System.Configuration;

namespace PSOK.Kernel.Configuration
{
    /// <summary>
    /// Configuration section for the p2p configuration section.
    /// </summary>
    public class Config : ConfigurationSection
    {
        /// <summary>
        /// Reads the P2P configuration.
        /// </summary>
        /// <returns></returns>
        public static Config ReadConfig()
        {
            return ConfigurationManager.GetSection("p2p") as Config;
        }

        /// <summary>
        /// Pipelines settings.
        /// </summary>
        [ConfigurationProperty("pipelines")]
        public Pipelines.Pipelines Pipelines
        {
            get { return this["pipelines"] as Pipelines.Pipelines; }
        }

        /// <summary>
        /// Process model settings.
        /// </summary>
        [ConfigurationProperty("processmodel")]
        public ProcessModel ProcessModel
        {
            get { return this["processmodel"] as ProcessModel; }
        }

        /// <summary>
        /// Transport and communication settings.
        /// </summary>
        [ConfigurationProperty("transport")]
        public Transport Transport
        {
            get { return this["transport"] as Transport; }
        }

        /// <summary>
        /// Scheduled tasks and agents.
        /// </summary>
        [ConfigurationProperty("scheduling")]
        public Scheduling.Scheduling Scheduling
        {
            get { return this["scheduling"] as Scheduling.Scheduling; }
        }

        /// <summary>
        /// Kademlia settings.
        /// </summary>
        [ConfigurationProperty("kademlia")]
        public Kademlia.Kademlia Kademlia
        {
            get { return this["kademlia"] as Kademlia.Kademlia; }
        }
    }
}