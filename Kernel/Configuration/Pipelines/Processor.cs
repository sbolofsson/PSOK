using System.Configuration;

namespace PSOK.Kernel.Configuration.Pipelines
{
    /// <summary>
    /// A single processor contained in a <see cref="Pipeline" />.
    /// </summary>
    public class Processor : System.Configuration.ConfigurationElement
    {
        /// <summary>
        /// The fully qualified assembly name of the processor.
        /// </summary>
        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get { return this["type"] as string; }
        }
    }
}