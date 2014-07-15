using System;
using System.Data.Services;
using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Security;
using PSOK.Kernel.Exceptions;
using PSOK.Kernel.Reflection;
using PSOK.Kernel.Security;
using PSOK.Kernel.Serialization;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Control the behavior of a service provider and consumer.
    /// </summary>
    public class ServiceBehavior
    {
        private readonly Protocol _protocol;
        private readonly bool _enableSsl;

        /// <summary>
        /// Construct a new behavior with the given protocol and ssl options.
        /// </summary>
        /// <param name="protocol"></param>
        /// <param name="enableSsl"></param>
        public ServiceBehavior(Protocol protocol, bool enableSsl)
        {
            _protocol = protocol;
            _enableSsl = enableSsl;
        }

        /// <summary>
        /// Sets the behavior for the given servicehost.
        /// </summary>
        /// <param name="serviceHost"></param>
        public void SetBehavior(System.ServiceModel.ServiceHostBase serviceHost)
        {
            if (serviceHost == null)
                throw new ArgumentNullException("serviceHost");

            SetServiceCredentials(serviceHost);
            ServiceDescription serviceDescription = serviceHost.Description;
            EnableDebugging(serviceDescription);
            OptimizeServiceThrottling(serviceDescription);
            ReplaceDataContractSerializer(serviceDescription);

            // Add mex as the last thing
            AddMetadataEndpoint(serviceHost);
        }

        /// <summary>
        /// Sets the behavior on a channel factory.
        /// </summary>
        /// <param name="channelFactory"></param>
        public void SetBehavior(ChannelFactory channelFactory)
        {
            if (channelFactory == null)
                throw new ArgumentNullException("channelFactory");

            SetClientCredentials(channelFactory);
            ReplaceDataContractSerializer(channelFactory.Endpoint.Contract);
        }

        /// <summary>
        /// Adds a mex endpoint to a service host.
        /// </summary>
        /// <param name="serviceHost"></param>
        public void AddMetadataEndpoint(System.ServiceModel.ServiceHostBase serviceHost)
        {
            if (serviceHost == null)
                throw new ArgumentNullException("serviceHost");

            if (serviceHost.GetType() == typeof (DataServiceHost) ||
                TypeHelper.IsSubclassOf(serviceHost.GetType(), typeof (DataServiceHost)))
                return;

            System.ServiceModel.Channels.Binding binding = null;

            if (_protocol == Protocol.Http && !_enableSsl)
            {
                binding = Binding.GetMexHttpBinding();
            }
            else if (_protocol == Protocol.Http && _enableSsl)
            {
                binding = Binding.GetMexHttpsBinding();
            }
            else if (_protocol == Protocol.Tcp)
            {
                //binding = Binding.GetMexNetTcpBinding();
            }
            else
            {
                throw new ServiceException("Could not determine metadata binding for servicehost.");
            }

            if (binding != null)
                serviceHost.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, binding, "mex");
        }

        /// <summary>
        /// Performance optimizes and adds debug information to a service description.
        /// </summary>
        /// <param name="serviceDescription"></param>
        public void EnableDebugging(ServiceDescription serviceDescription)
        {
            if (serviceDescription == null)
                throw new ArgumentNullException("serviceDescription");

            ServiceMetadataBehavior serviceMetadataBehavior =
                serviceDescription.Behaviors.Find<ServiceMetadataBehavior>();

            if (serviceMetadataBehavior == null)
            {
                serviceMetadataBehavior = new ServiceMetadataBehavior();
                serviceDescription.Behaviors.Add(serviceMetadataBehavior);
            }

            serviceMetadataBehavior.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;

            if (_protocol == Protocol.Http)
            {
                serviceMetadataBehavior.HttpGetEnabled = true;
                serviceMetadataBehavior.HttpsGetEnabled = true;
            }

            ServiceDebugBehavior serviceDebugBehavior = serviceDescription.Behaviors.Find<ServiceDebugBehavior>();

            if (serviceDebugBehavior == null)
            {
                serviceDebugBehavior = new ServiceDebugBehavior();
                serviceDescription.Behaviors.Add(serviceDebugBehavior);
            }

            serviceDebugBehavior.IncludeExceptionDetailInFaults = true;
        }

        /// <summary>
        /// Sets properties on the specified <see cref="System.ServiceModel.Description.ServiceDescription"/> to optimize service throttling.
        /// </summary>
        /// <param name="serviceDescription"></param>
        public void OptimizeServiceThrottling(ServiceDescription serviceDescription)
        {
            if (serviceDescription == null)
                throw new ArgumentNullException("serviceDescription");

            ServiceThrottlingBehavior serviceThrottlingBehavior =
                serviceDescription.Behaviors.Find<ServiceThrottlingBehavior>();

            if (serviceThrottlingBehavior == null)
            {
                serviceThrottlingBehavior = new ServiceThrottlingBehavior();
                serviceDescription.Behaviors.Add(serviceThrottlingBehavior);
            }

            serviceThrottlingBehavior.MaxConcurrentCalls = GetServiceThrottling();
            serviceThrottlingBehavior.MaxConcurrentInstances = GetServiceThrottling();
            serviceThrottlingBehavior.MaxConcurrentSessions = GetServiceThrottling();
        }

        /// <summary>
        /// Replace the data contract serialiezer on the specified <see cref="System.ServiceModel.Description.ServiceDescription"/> with a custom serializer.
        /// </summary>
        /// <param name="serviceDescription"></param>
        public void ReplaceDataContractSerializer(ServiceDescription serviceDescription)
        {
            if (serviceDescription == null)
                throw new ArgumentNullException("serviceDescription");

            foreach (ServiceEndpoint serviceEndpoint in serviceDescription.Endpoints)
            {
                ReplaceDataContractSerializer(serviceEndpoint.Contract);
            }
        }

        /// <summary>
        /// Sets the behavior on a service contract description.
        /// </summary>
        /// <param name="contractDescription"></param>
        public static void ReplaceDataContractSerializer(ContractDescription contractDescription)
        {
            if (contractDescription == null)
                throw new ArgumentNullException("contractDescription");

            // Ignore Mex
            if (contractDescription.ContractType == typeof (IMetadataExchange))
                return;

            foreach (OperationDescription operationDescription in contractDescription.Operations)
            {
                ReplaceDataContractSerializer(operationDescription);
            }
        }

        /// <summary>
        /// Changes the data contract serializer used on a service operation.
        /// </summary>
        /// <param name="operationDescription"></param>
        public static void ReplaceDataContractSerializer(OperationDescription operationDescription)
        {
            if (operationDescription == null)
                throw new ArgumentNullException("operationDescription");

            System.ServiceModel.Description.DataContractSerializerOperationBehavior
                dataContractSerializerOperationBehavior =
                    operationDescription.Behaviors
                        .Find<System.ServiceModel.Description.DataContractSerializerOperationBehavior>();

            if (dataContractSerializerOperationBehavior != null)
            {
                // Remove old behavior
                operationDescription.Behaviors.Remove(dataContractSerializerOperationBehavior);

                // Add custom data contract resolver
                dataContractSerializerOperationBehavior.DataContractResolver = new DataContractResolver();
            }

            // Add custom behavior
            if (operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>() == null)
                operationDescription.Behaviors.Add(new DataContractSerializerOperationBehavior(operationDescription));
        }

        /// <summary>
        /// Sets the credentials to use for the service host.
        /// </summary>
        /// <param name="serviceHost"></param>
        public void SetServiceCredentials(System.ServiceModel.ServiceHostBase serviceHost)
        {
            if (serviceHost == null)
                throw new ArgumentNullException("serviceHost");

            if (serviceHost.Credentials == null)
                throw new ArgumentException("ServiceHost credentials may not be null.");

            serviceHost.Credentials.ServiceCertificate.Certificate = null;

            // Set service certificate
            if (_enableSsl)
                serviceHost.Credentials.ServiceCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName, Transport.ServiceDomainName);

            // Set the trust level for client certificates
            X509ClientCertificateAuthentication x509ClientCertificateAuthentication =
                serviceHost.Credentials.ClientCertificate.Authentication;
            x509ClientCertificateAuthentication.CertificateValidationMode =
                X509CertificateValidationMode.PeerOrChainTrust;
            x509ClientCertificateAuthentication.TrustedStoreLocation = StoreLocation.LocalMachine;
            x509ClientCertificateAuthentication.RevocationMode = X509RevocationMode.NoCheck;
            x509ClientCertificateAuthentication.CustomCertificateValidator = new X509CertificateValidator();
        }

        /// <summary>
        /// Sets the client credentials for a channel factory.
        /// </summary>
        /// <param name="channelFactory"></param>
        public void SetClientCredentials(ChannelFactory channelFactory)
        {
            if (channelFactory == null)
                throw new ArgumentNullException("channelFactory");

            if (channelFactory.Credentials == null)
                throw new ArgumentException("ChannelFactory credentials may not be null.");

            channelFactory.Credentials.ClientCertificate.Certificate = null;

            // Set client certificate
            if (_enableSsl)
                channelFactory.Credentials.ClientCertificate.SetCertificate(StoreLocation.LocalMachine, StoreName.My,
                    X509FindType.FindBySubjectName, Transport.ClientDomainName);

            // Set the trust level for service certificates
            X509ServiceCertificateAuthentication x509ServiceCertificateAuthentication =
                channelFactory.Credentials.ServiceCertificate.Authentication;
            x509ServiceCertificateAuthentication.CertificateValidationMode =
                X509CertificateValidationMode.PeerOrChainTrust;
            x509ServiceCertificateAuthentication.TrustedStoreLocation =
                StoreLocation.LocalMachine;
            x509ServiceCertificateAuthentication.RevocationMode = X509RevocationMode.NoCheck;
            x509ServiceCertificateAuthentication.CustomCertificateValidator = new X509CertificateValidator();
        }

        /// <summary>
        /// Gets a performance optimized service throttling value.
        /// </summary>
        /// <returns></returns>
        public static int GetServiceThrottling()
        {
            // http://msdn.microsoft.com/en-us/library/ee377061.aspx
            // Should be more than 16 * processor count
            return 100 * System.Environment.ProcessorCount;
        }
    }
}