using System.Configuration;

namespace PSOK.Kernel.Configuration
{
    /// <summary>
    /// Process model settings.
    /// </summary>
    public class ProcessModel : System.Configuration.ConfigurationElement
    {
        /// <summary>
        /// Indicates the amount of worker threads used to process events from each event queue.
        /// </summary>
        [ConfigurationProperty("workerthreads", IsRequired = true)]
        public int WorkerThreads
        {
            get { return (int)this["workerthreads"]; }
        }
    }
}