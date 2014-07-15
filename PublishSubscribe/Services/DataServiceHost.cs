using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Services.Providers;
using System.Linq;
using System.Reflection;
using PSOK.Kernel.EntityFramework;
using PSOK.Kernel.Reflection;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Exceptions;
using PSOK.PublishSubscribe.Messages.Requests.DataRequest;
using log4net;

namespace PSOK.PublishSubscribe.Services
{
    /// <summary>
    /// Service host for a <see cref="EntityFrameworkDataService{T}" />.
    /// </summary>
    internal class DataServiceHost : ServiceHostBase, IDataContext
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (DataServiceHost));
        private readonly IPeer _peer;
        private Type _serviceType;
        private Type _dbContextType;
        private DbContext _dbContext;
        private bool _isServiceRegistered;

        /// <summary>
        /// Constructs a new <see cref="DataServiceHost" />.
        /// </summary>
        /// <param name="peer"></param>
        internal DataServiceHost(IPeer peer)
        {
            if (peer == null)
                throw new ArgumentNullException("peer");

            _peer = peer;
        }

        /// <summary>
        /// Initializes the service.
        /// </summary>
        public override void Initialize()
        {
            if (CheckIsInitialized)
                return;

            try
            {
                InitializeDisposeLock.EnterWriteLock();

                if (CheckIsInitialized)
                    return;

                ServiceBehavior behavior = new ServiceBehavior(Protocol.Http, Transport.EnableSsl);
                behavior.SetBehavior(ServiceHost);

                ServiceHost.Open();

                SubscribeToDataRequests();
            }
            finally
            {
                InitializeDisposeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="DataServiceHost" /> is initialized.
        /// This property is not thread safe.
        /// </summary>
        protected override bool CheckIsInitialized
        {
            get { return !_isServiceRegistered || base.CheckIsInitialized; }
        }

        /// <summary>
        /// Registers a <see cref="IDataServiceBase{T}" />.
        /// All <see cref="DataRequest" />s are automatically subscribed to.
        /// Other <see cref="Broker" />s can access the service by publishing <see cref="DataRequest" />s.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void RegisterDataService<T>() where T : IDataServiceBase<DbContext>, new()
        {
            if (_isServiceRegistered)
                throw new DataContextException("A dataservice is already registered with this context.");

            try
            {
                InitializeDisposeLock.EnterWriteLock();

                if (_isServiceRegistered)
                    throw new DataContextException("A dataservice is already registered with this context.");

                IDataServiceBase<DbContext> dataService =
                    Activator.CreateInstance(typeof (T)) as IDataServiceBase<DbContext>;

                if (dataService == null)
                    throw new DataContextException(
                        string.Format("Could not create an instance of the given '{0}' of type '{1}'.",
                            typeof(IDataServiceBase<DbContext>).Name, typeof(T).AssemblyQualifiedName()));

                _serviceType = dataService.GetType();
                _dbContextType = TypeHelper.GetGenericArgumentOfBaseType(dataService.GetType(), typeof (DbContext));

                if (_dbContextType == null)
                    throw new DataContextException(
                        string.Format("Could not determine the '{0}' type on the given '{1} of type '{2}'.",
                            typeof(DbContext).Name, typeof(IDataServiceBase<DbContext>).Name, typeof(T).AssemblyQualifiedName()));

                _dbContext = Activator.CreateInstance(_dbContextType) as DbContext;
                ServiceHost = new System.Data.Services.DataServiceHost(_serviceType,
                    new[] { new Uri(_peer.Node.Contact.DataContextUrl()) });

                _isServiceRegistered = true;
            }
            finally
            {
                InitializeDisposeLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Registers a <see cref="DbContext" />.
        /// All <see cref="DataRequest" />s are automatically subscribed to.
        /// Other <see cref="Broker" />s can access the service by publishing <see cref="DataRequest" />s.
        /// </summary>
        public void RegisterDbContext<T>() where T : DbContext, new()
        {
            RegisterDataService<DataService<T>>();
        }

        /// <summary>
        /// Subscribes to all <see cref="DataRequest" />s based on the registered <see cref="DbContext" />.
        /// </summary>
        private void SubscribeToDataRequests()
        {
            // Subscribe to all data requests for all entity types
            // CRUD security is implemented in the db context or dataservice layer and not in the P2P layer
            // This makes it easier for applications to centralize their validation
            IEnumerable<Type> entityTypes =
                (((Func<IEnumerable<Type>>) EntitySetHelper.GetEntityTypes<DbContext>).GetMethodInfo()
                    .GetGenericMethodDefinition().MakeGenericMethod(_dbContextType).Invoke(null, null) as
                    IEnumerable<Type> ?? new List<Type>()).ToList();

            IEnumerable<Type> dataRequests = TypeHelper.GetSubclasses(typeof (DataRequest))
                .Where(x => x.IsGenericType && x.GetGenericArguments().Length == 1)
                .ToList();

            ObjectContext objectContext = (_dbContext as IObjectContextAdapter).ObjectContext;

            foreach (Type dataRequest in dataRequests)
            {
                foreach (Type entityType in entityTypes)
                {
                    try
                    {
                        ISubscription subscription =
                            Activator.CreateInstance(
                                typeof (Subscription<>).MakeGenericType(dataRequest.MakeGenericType(entityType))) as
                                ISubscription;

                        if (subscription == null)
                            throw new ArgumentException(
                                string.Format(
                                    "Could not subscribe to data requests of type '{0}' for entities of type '{1}'.",
                                    dataRequest.Name, entityType.AssemblyQualifiedName()));

                        subscription.EntitySet = EntitySetHelper.GetEntitySetName(objectContext, entityType);
                        _peer.Subscribe(subscription);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }
            }
        }
    }
}