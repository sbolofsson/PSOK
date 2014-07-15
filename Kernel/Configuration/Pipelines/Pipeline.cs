using System.Configuration;

namespace PSOK.Kernel.Configuration.Pipelines
{
    /// <summary>
    /// Defines a pipeline.
    /// </summary>
    [ConfigurationCollection(typeof (Processor), AddItemName = "processor", ClearItemsName = "clear",
        RemoveItemName = "remove",
        CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class Pipeline : ConfigurationElementCollection<Processor>
    {
        /// <summary>
        /// Constructs a new pipeline.
        /// </summary>
        public Pipeline()
        {
        }

        /// <summary>
        /// Constructs a new pipeline with the given name.
        /// </summary>
        /// <param name="name"></param>
        public Pipeline(string name) : base(name)
        {
        }

        /// <summary>
        /// Indicates the amount of worker threads used to process events from each event queue.
        /// </summary>
        [ConfigurationProperty("workerthreads", IsRequired = false)]
        public int? WorkerThreads
        {
            get { return (int?)this["workerthreads"]; }
        }
    }
}