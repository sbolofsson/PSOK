using PSOK.Kernel.Configuration;
using PSOK.Kernel.Environment;
using PSOK.Kernel.Exceptions;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Defines how communication should be conducted.
    /// </summary>
    public static class Transport
    {
        private static Protocol _protocol;
        private static string _scheme;
        private static int? _port;
        private static bool? _enableSsl;

        private static string _domain;
        private static string _serviceDomainName;
        private static string _clientDomainName;

        /// <summary>
        /// The port to use for HTTP communication
        /// </summary>
        public static int HttpPort
        {
            get { return 80; }
        }

        /// <summary>
        /// The default port to use for TCP communication
        /// </summary>
        public static int TcpPort
        {
            get { return 808; }
        }

        /// <summary>
        /// The default port to use for other communication
        /// </summary>
        public static int DefaultPort
        {
            get { return 36400; }
        }

        /// <summary>
        /// The default protocol to use in the application.
        /// </summary>
        public static Protocol Protocol
        {
            get
            {
                if (_protocol != Protocol.None)
                    return _protocol;

                Config config = Config.ReadConfig();
                string protocol = config.Transport.Mode;
                switch (protocol)
                {
                    case "http":
                        return (_protocol = Protocol.Http);
                    case "tcp":
                        return (_protocol = Protocol.Tcp);
                    default:
                        throw new ConfigurationException("Could not determine transport mode.");
                }
            }
        }

        /// <summary>
        /// Indicates whether SSL communication is enabled.
        /// </summary>
        public static bool EnableSsl
        {
            get
            {
                if (_enableSsl != null)
                    return _enableSsl.Value;

                Config config = Config.ReadConfig();
                return (_enableSsl = config.Transport.Security).Value;
            }
        }

        /// <summary>
        /// The currently used protocol scheme.
        /// </summary>
        /// <returns></returns>
        public static string GetProtocolScheme()
        {
            if (_scheme != null)
                return _scheme;

            if (Protocol == Protocol.Http && !EnableSsl)
            {
                _scheme = "http";
            }
            else if (Protocol == Protocol.Http && EnableSsl)
            {
                _scheme = "https";
            }
            else if (Protocol == Protocol.Tcp)
            {
                _scheme = "net.tcp";
            }

            return _scheme;
        }

        /// <summary>
        /// The currently used protocol port.
        /// </summary>
        /// <returns></returns>
        public static int GetProtocolPort()
        {
            if (_port != null)
                return _port.Value;

            int defaultPort = HttpPort;

            if (Protocol == Protocol.Http && !EnableSsl)
            {
                defaultPort = HttpPort;
            }
            else if (Protocol == Protocol.Http && EnableSsl)
            {
                defaultPort = DefaultPort;
            }
            else if (Protocol == Protocol.Tcp)
            {
                defaultPort = TcpPort;
            }

            return (_port = defaultPort).Value;
        }

        /// <summary>
        /// The base url to use for services.
        /// </summary>
        /// <returns></returns>
        public static string GetServiceBaseUrl()
        {
            return string.Format("{0}://{1}:{2}", GetProtocolScheme(), Domain, GetProtocolPort());
        }

        /// <summary>
        /// The domain name that services are exposed under.
        /// </summary>
        public static string Domain
        {
            get
            {
                if (_domain != null)
                    return _domain;

                Config config = Config.ReadConfig();
                return (_domain = (!string.IsNullOrEmpty(config.Transport.Domain) ?
                    config.Transport.Domain : EnvironmentHelper.GetFqdn()));
            }
        }

        /// <summary>
        /// The domain name that services are exposed under.
        /// </summary>
        public static string ServiceDomainName
        {
            get
            {
                if (_serviceDomainName != null)
                    return _serviceDomainName;

                Config config = Config.ReadConfig();
                return (_serviceDomainName = (!string.IsNullOrEmpty(config.Transport.ServiceDns) ?
                    config.Transport.ServiceDns : EnvironmentHelper.GetFqdn()));
            }
        }

        /// <summary>
        /// The domain name that client channels expose themselves as.
        /// </summary>
        public static string ClientDomainName
        {
            get
            {
                if (_clientDomainName != null)
                    return _clientDomainName;

                Config config = Config.ReadConfig();
                return (_clientDomainName = (!string.IsNullOrEmpty(config.Transport.ClientDns) ?
                    config.Transport.ClientDns : EnvironmentHelper.GetFqdn()));
            }
        }
    }
}