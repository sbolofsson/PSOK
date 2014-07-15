using System.Configuration;

namespace PSOK.Kernel.Configuration
{
    /// <summary>
    /// Defines a basic CData configuration element.
    /// </summary>
    public class ConfigurationElement : CDataConfigurationElement
    {
        /// <summary>
        /// Indicates the CDATA value of the <see cref="CDataConfigurationElement"/>.
        /// </summary>
        [ConfigurationProperty("cdata", IsRequired = true), CDataConfigurationProperty]
        public string CData
        {
            get { return base["cdata"] as string; }
        }
    }
}