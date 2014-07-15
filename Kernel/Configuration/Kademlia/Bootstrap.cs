using System.Configuration;

namespace PSOK.Kernel.Configuration.Kademlia
{
    /// <summary>
    /// Transport and communication settings.
    /// </summary>
    public class Bootstrap : System.Configuration.ConfigurationElement
    {
        /// <summary>
        /// The bootstrap urls.
        /// </summary>
        [ConfigurationProperty("urls", IsRequired = true)]
        public string Urls
        {
            get { return (string) this["urls"]; }
        }

        /// <summary>
        /// The bootstrap file.
        /// </summary>
        [ConfigurationProperty("file", IsRequired = true)]
        public string File
        {
            get { return (string)this["file"]; }
        }
    }
}