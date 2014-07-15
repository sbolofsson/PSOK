using System;
using System.Data.Entity;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Pipelines;
using log4net;

namespace PSOK.PublishSubscribe.Services
{
    /// <summary>
    /// Base class for data services which ensures initialization of the <see cref="PubSubContext" />.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [ServiceBehavior(
        IncludeExceptionDetailInFaults = true,
        MaxItemsInObjectGraph = int.MaxValue,
        UseSynchronizationContext = false,
        InstanceContextMode = InstanceContextMode.PerCall,
        ConcurrencyMode = ConcurrencyMode.Single),
     AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public abstract class DataServiceBase<T> : EntityFrameworkDataService<T>, IDataServiceBase<T>
        where T : DbContext, new()
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (DataServiceBase<T>));

        /// <summary>
        /// Constructs a new <see cref="DataServiceBase{T}" />.
        /// </summary>
        protected DataServiceBase()
        {
            // Start request pipelines
            ProcessingPipeline.ProcessingRequest += (sender, args) => BuildContext();
            ProcessingPipeline.ProcessingChangeset += (sender, args) => BuildContext();

            // Finish request pipelines
            ProcessingPipeline.ProcessedRequest += (sender, args) => DisposeContext();
            ProcessingPipeline.ProcessedChangeset += (sender, args) => DisposeContext();
        }

        /// <summary>
        /// Builds the <see cref="PubSubContext" /> based on the current <see cref="WebOperationContext" />.
        /// </summary>
        private void BuildContext()
        {
            PubSubContext.BuildContext(WebOperationContext.Current);
        }

        /// <summary>
        /// Disposes the <see cref="PubSubContext" />.
        /// </summary>
        private void DisposeContext()
        {
            PubSubContext.Dispose();
        }

        /// <summary>
        /// Generic exception handler.
        /// </summary>
        /// <param name="args"></param>
        protected override void HandleException(HandleExceptionArgs args)
        {
            Exception exception = args.Exception;
            Exception innerException = exception.InnerException;

            if (exception is TargetInvocationException && innerException != null)
            {
                if (innerException is DataServiceException)
                {
                    args.Exception = innerException;
                }
                else
                {
                    args.Exception = new DataServiceException(400, innerException.Message);
                }

                Log.Error(innerException);
            }
            else
            {
                Log.Error(exception);
            }
        }
    }
}