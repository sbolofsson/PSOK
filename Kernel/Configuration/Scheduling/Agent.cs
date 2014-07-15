using System.Configuration;

namespace PSOK.Kernel.Configuration.Scheduling
{
    /// <summary>
    /// Agent settings.
    /// </summary>
    [ConfigurationCollection(typeof (Param), AddItemName = "param", ClearItemsName = "clear", RemoveItemName = "remove",
        CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class Agent : ConfigurationElementCollection<Param>
    {
        /// <summary>
        /// The fully qualified assembly name of the agent.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }

        /// <summary>
        /// The interval at which the agent should be invoked.
        /// </summary>
        [ConfigurationProperty("interval", IsRequired = true)]
        public string Interval
        {
            get { return this["interval"] as string; }
        }

        /// <summary>
        /// The method to invoke.
        /// </summary>
        [ConfigurationProperty("method", IsRequired = true)]
        public string Method
        {
            get { return this["method"] as string; }
        }
    }
}