using System;
using System.Net.Security;
using System.Security.Authentication.ExtendedProtection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using System.Xml;
using PSOK.Kernel.Exceptions;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Class representing a WCF binding.
    /// </summary>
    public static class Binding
    {
        /// <summary>
        /// Gets the default binding to use.
        /// </summary>
        /// <returns></returns>
        public static System.ServiceModel.Channels.Binding GetDefaultBinding()
        {
            switch (Transport.Protocol)
            {
                case Protocol.Http:
                    return GetWsHttpBinding();

                case Protocol.Tcp:
                    return GetNetTcpBinding();

                default:
                    throw new ServiceException("Could not determine binding.");
            }
        }

        /// <summary>
        /// Gets a performance optimized <see cref="System.ServiceModel.NetTcpBinding" /> binding.
        /// </summary>
        /// <returns></returns>
        public static NetTcpBinding GetNetTcpBinding()
        {
            /*
                The ListenBacklog and MaxConnections are ignored when PortSharingEnabled = true
                In this case, the NetTcpPortSharing service is used to control the ports and
                therefore the values specified in the machine wide C:\Windows\Microsoft.NET\Framework64\v4.0.30319\SMSvcHost.exe.config
                are used instead.
                
                If PortSharingEnabled = false and a metadata endpoint (mex) is added to the service endpoint, then
                the ListenBacklog and MaxConnections values must correspond with the values from the mex binding or
                an exception will be thrown.
                The framework uses 12 * System.Environment.ProcessorCount to determine these values for the mex endpoint.
                See more here: http://support.microsoft.com/kb/2773443
             
             */

            return new NetTcpBinding
            {
                CloseTimeout = GetDefaultTimeout(),
                OpenTimeout = GetDefaultTimeout(),
                ReceiveTimeout = GetDefaultTimeout(),
                SendTimeout = GetDefaultTimeout(),
                ListenBacklog = GetListenBacklog(),
                MaxConnections = GetMaxPendingConnections(),
                PortSharingEnabled = true,
                ReaderQuotas = GetReaderQuotas(),
                TransactionFlow = false,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                ReliableSession = new OptionalReliableSession
                {
                    Enabled = false,
                    Ordered = true,
                    InactivityTimeout = GetDefaultTimeout()
                },
                MaxBufferPoolSize = int.MaxValue,
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                TransferMode = TransferMode.Streamed,
                Security = Transport.EnableSsl
                    ? new NetTcpSecurity
                    {
                        Mode = SecurityMode.TransportWithMessageCredential,
                        Message = new MessageSecurityOverTcp
                        {
                            AlgorithmSuite = new TripleDesSecurityAlgorithmSuite(),
                            ClientCredentialType = MessageCredentialType.Certificate
                        },
                        Transport = new TcpTransportSecurity
                        {
                            ClientCredentialType = TcpClientCredentialType.Certificate,
                            ProtectionLevel = ProtectionLevel.EncryptAndSign,
                            ExtendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.WhenSupported)
                        }
                    }
                    : new NetTcpSecurity {Mode = SecurityMode.None}
            };
        }

        /// <summary>
        /// Gets a performance optimized <see cref="System.ServiceModel.WSHttpBinding" /> binding.
        /// </summary>
        /// <returns></returns>
        public static WSHttpBinding GetWsHttpBinding()
        {
            return new WSHttpBinding
            {
                CloseTimeout = GetDefaultTimeout(),
                OpenTimeout = GetDefaultTimeout(),
                ReceiveTimeout = GetDefaultTimeout(),
                SendTimeout = GetDefaultTimeout(),
                AllowCookies = false,
                BypassProxyOnLocal = false,
                HostNameComparisonMode = HostNameComparisonMode.StrongWildcard,
                MaxBufferPoolSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                MessageEncoding = WSMessageEncoding.Mtom,
                TextEncoding = System.Text.Encoding.UTF8,
                UseDefaultWebProxy = false,
                ReaderQuotas = GetReaderQuotas(),
                TransactionFlow = false,
                ReliableSession = new OptionalReliableSession
                {
                    Enabled = false,
                    Ordered = true,
                    InactivityTimeout = GetDefaultTimeout()
                },
                Security = Transport.EnableSsl
                    ? new WSHttpSecurity
                    {
                        Mode = SecurityMode.TransportWithMessageCredential,
                        Message = new NonDualMessageSecurityOverHttp
                        {
                            ClientCredentialType = MessageCredentialType.Certificate,
                            AlgorithmSuite = new TripleDesSecurityAlgorithmSuite(),
                            EstablishSecurityContext = false,
                            NegotiateServiceCredential = false
                        },
                        Transport = new HttpTransportSecurity
                        {
                            ClientCredentialType = HttpClientCredentialType.Certificate,
                            ProxyCredentialType = HttpProxyCredentialType.None,
                            Realm = string.Empty,
                            ExtendedProtectionPolicy = new ExtendedProtectionPolicy(PolicyEnforcement.WhenSupported)
                        }
                    }
                    : new WSHttpSecurity {Mode = SecurityMode.None}
            };
        }

        /// <summary>
        /// Gets a performance optimized <see cref="XmlDictionaryReaderQuotas" />
        /// </summary>
        /// <returns></returns>
        public static XmlDictionaryReaderQuotas GetReaderQuotas()
        {
            return new XmlDictionaryReaderQuotas
            {
                MaxDepth = int.MaxValue,
                MaxStringContentLength = int.MaxValue,
                MaxArrayLength = int.MaxValue,
                MaxBytesPerRead = int.MaxValue,
                MaxNameTableCharCount = int.MaxValue
            };
        }

        /// <summary>
        /// Creates a mex net tcp binding.
        /// </summary>
        /// <returns></returns>
        public static System.ServiceModel.Channels.Binding GetMexNetTcpBinding()
        {
            System.ServiceModel.Channels.Binding mexBinding = MetadataExchangeBindings.CreateMexTcpBinding();
            SetTimeouts(mexBinding);
            return mexBinding;
        }

        /// <summary>
        /// Creates a mex http binding.
        /// </summary>
        /// <returns></returns>
        public static System.ServiceModel.Channels.Binding GetMexHttpBinding()
        {
            System.ServiceModel.Channels.Binding mexBinding = MetadataExchangeBindings.CreateMexHttpBinding();
            SetTimeouts(mexBinding);
            return mexBinding;
        }

        /// <summary>
        /// Creates a mex https binding.
        /// </summary>
        /// <returns></returns>
        public static System.ServiceModel.Channels.Binding GetMexHttpsBinding()
        {
            System.ServiceModel.Channels.Binding mexBinding = MetadataExchangeBindings.CreateMexHttpsBinding();
            SetTimeouts(mexBinding);
            return mexBinding;
        }

        /// <summary>
        /// Sets the default timeouts for the given mex binding.
        /// </summary>
        /// <param name="mexBinding"></param>
        private static void SetTimeouts(System.ServiceModel.Channels.Binding mexBinding)
        {
            if (mexBinding == null)
                throw new ArgumentNullException("mexBinding");

            mexBinding.CloseTimeout = GetDefaultTimeout();
            mexBinding.OpenTimeout = GetDefaultTimeout();
            mexBinding.ReceiveTimeout = GetDefaultTimeout();
            mexBinding.SendTimeout = GetDefaultTimeout();
        }

        /// <summary>
        /// Gets the default timeout
        /// </summary>
        /// <returns></returns>
        public static TimeSpan GetDefaultTimeout()
        {
            return new TimeSpan(0, 0, 10);
        }

        /// <summary>
        /// Gets max pending accepts
        /// </summary>
        /// <returns></returns>
        public static int GetMaxPendingAccepts()
        {
            // http://msdn.microsoft.com/en-us/library/aa702669(v=vs.110).aspx
            // The default is 4 * processor count
            return 100 * System.Environment.ProcessorCount * 4;
        }

        /// <summary>
        /// Gets listen backlog
        /// </summary>
        /// <returns></returns>
        public static int GetListenBacklog()
        {
            // http://msdn.microsoft.com/en-us/library/aa702669(v=vs.110).aspx
            // The default is 16 * processor count
            return 100*System.Environment.ProcessorCount*4;
        }

        /// <summary>
        /// Get max pending connections
        /// </summary>
        /// <returns></returns>
        public static int GetMaxPendingConnections()
        {
            // http://msdn.microsoft.com/en-us/library/aa702669(v=vs.110).aspx
            // The default is 100
            return 100 * System.Environment.ProcessorCount * 4;
        }
    }
}