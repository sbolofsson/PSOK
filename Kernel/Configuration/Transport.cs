using System.Configuration;

namespace PSOK.Kernel.Configuration
{
    /// <summary>
    /// Transport and communication settings.
    /// </summary>
    public class Transport : System.Configuration.ConfigurationElement
    {
        /// <summary>
        /// The transport mode to use - valid settings are: http, tcp.
        /// </summary>
        [ConfigurationProperty("mode", IsRequired = true)]
        public string Mode
        {
            get { return (string) this["mode"]; }
        }

        /// <summary>
        /// Indicates whether SSL is enabled.
        /// </summary>
        [ConfigurationProperty("security", IsRequired = true)]
        public bool Security
        {
            get { return (bool) this["security"]; }
        }

        /// <summary>
        /// The domain name of services. Must preferable match the ServiceDns attribute
        /// </summary>
        [ConfigurationProperty("domain", IsRequired = false)]
        public string Domain
        {
            get { return (string)this["domain"]; }
        }

        /// <summary>
        /// The DNS name of services. Must match with the Common Name (CN) in the subject of the used X.509 certificate.
        /// </summary>
        [ConfigurationProperty("servicedns", IsRequired = true)]
        public string ServiceDns
        {
            get { return (string) this["servicedns"]; }
        }

        /// <summary>
        /// The DNS name of clients. Must match with the Common Name (CN) in the subject of the used X.509 certificate.
        /// </summary>
        [ConfigurationProperty("clientdns", IsRequired = true)]
        public string ClientDns
        {
            get { return (string) this["clientdns"]; }
        }
    }
}