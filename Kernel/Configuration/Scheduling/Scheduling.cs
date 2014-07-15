using System.Configuration;

namespace PSOK.Kernel.Configuration.Scheduling
{
    /// <summary>
    /// Scheduling options for agents and the scheduler.
    /// </summary>
    [ConfigurationCollection(typeof (Agent), AddItemName = "agent", ClearItemsName = "clear", RemoveItemName = "remove",
        CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class Scheduling : ConfigurationElementCollection<Agent>
    {
        /// <summary>
        /// The frequency to check for scheduled <see cref="Agent" />s.
        /// </summary>
        [ConfigurationProperty("frequency", IsRequired = true)]
        public Frequency Frequency
        {
            get { return this["frequency"] as Frequency; }
        }
    }
}