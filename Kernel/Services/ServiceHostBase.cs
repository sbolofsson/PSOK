using System;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Description;
using System.Threading;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Base class defining some basic settings and initialization methods for WCF services.
    /// </summary>
    [ServiceBehavior(
        IncludeExceptionDetailInFaults = true,
        MaxItemsInObjectGraph = int.MaxValue,
        UseSynchronizationContext = false,
        InstanceContextMode = InstanceContextMode.Single,
        ConcurrencyMode = ConcurrencyMode.Multiple),
     AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public abstract class ServiceHostBase : IServiceHost
    {
        /// <summary>
        /// The underlying service host object used for hosting service endpoints.
        /// </summary>
        protected System.ServiceModel.ServiceHostBase ServiceHost;

        /// <summary>
        /// A lock to ensure thread safety on initialization and dispose operations.
        /// </summary>
        protected readonly ReaderWriterLockSlim InitializeDisposeLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Indicates whether the current <see cref="ServiceHostBase"/> instance has been disposed.
        /// </summary>
        protected bool Disposed;

        /// <summary>
        /// Initializes the <see cref="ServiceHostBase" />.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Initializes the <see cref="ServiceHostBase" /> with a single endpoint.
        /// </summary>
        /// <param name="name">The name of the endpoint.</param>
        /// <param name="url">The url to host the endpoint at.</param>
        /// <param name="serviceType">The interface type of the service.</param>
        /// <param name="modifyServiceHost"></param>
        /// <param name="modifyBinding"></param>
        protected void Initialize(string name, string url, Type serviceType, Action<System.ServiceModel.ServiceHostBase> modifyServiceHost = null, Action<System.ServiceModel.Channels.Binding> modifyBinding = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException("url");

            if (serviceType == null)
                throw new ArgumentNullException("serviceType");

            if (CheckIsInitialized)
                return;

            try
            {
                InitializeDisposeLock.EnterWriteLock();

                if (CheckIsInitialized)
                    return;

                ServiceHost = new ServiceHost(this, new Uri(url));

                EndpointIdentity endpointIdentity = EndpointIdentity.CreateDnsIdentity(Transport.ServiceDomainName);
                EndpointAddress endpointAddress = new EndpointAddress(new Uri(url), endpointIdentity);

                System.ServiceModel.Channels.Binding binding = Binding.GetDefaultBinding();

                if (modifyBinding != null)
                    modifyBinding(binding);

                ServiceEndpoint serviceEndpoint = new ServiceEndpoint(ContractDescription.GetContract(serviceType))
                {
                    Name = name,
                    Address = endpointAddress,
                    Contract = ContractDescription.GetContract(serviceType),
                    Binding = binding
                };

                ServiceHost.AddServiceEndpoint(serviceEndpoint);

                ServiceBehavior behavior = new ServiceBehavior(Transport.Protocol, Transport.EnableSsl);
                behavior.SetBehavior(ServiceHost);

                if (modifyServiceHost != null)
                    modifyServiceHost(ServiceHost);

                if (EnableLogging)
                {
                    ServiceHost.Description.Behaviors.Add(new LogMessageBehavior());
                }

                ServiceHost.Open();
            }
            finally
            {
                InitializeDisposeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Disposes the service.
        /// </summary>
        public void Dispose()
        {
            if (Disposed)
                return;

            try
            {
                InitializeDisposeLock.EnterWriteLock();
                if (Disposed)
                    return;

                ServiceHelper.CloseServiceHost(ServiceHost);
                Disposed = true;
            }
            finally
            {
                InitializeDisposeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="ServiceHostBase" /> is initialized.
        /// This property is not thread safe.
        /// </summary>
        protected virtual bool CheckIsInitialized
        {
            get
            {
                return Disposed ||
                       (ServiceHost != null &&
                        (ServiceHost.State == CommunicationState.Opened ||
                         ServiceHost.State == CommunicationState.Opening));
            }
        }

        /// <summary>
        /// Checks whether the <see cref="ServiceHostBase" /> is initialized.
        /// This property is thread safe.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                try
                {
                    InitializeDisposeLock.EnterReadLock();
                    return CheckIsInitialized;
                }
                finally
                {
                    InitializeDisposeLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Indicates whether logging is enabled for this <see cref="ServiceHostBase"/>.
        /// </summary>
        public bool EnableLogging
        {
            get
            {
                string webserviceLogging = ConfigurationManager.AppSettings["WebserviceLogging"];
                if (string.IsNullOrEmpty(webserviceLogging))
                    return false;
                bool enableLogging;
                bool.TryParse(webserviceLogging, out enableLogging);
                return enableLogging;
            }
        }
    }
}