using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Services;
using System.Data.Services.Common;
using PSOK.Kernel.EntityFramework;
using log4net;

namespace PSOK.PublishSubscribe.Services
{
    /// <summary>
    /// Default data service.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class DataService<T> : DataServiceBase<T> where T : DbContext, new()
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (DataService<T>));

        public static void InitializeService(DataServiceConfiguration dataServiceConfiguration)
        {
            // Configure settings
            try
            {
                dataServiceConfiguration.UseVerboseErrors = true;
                dataServiceConfiguration.DataServiceBehavior.AcceptCountRequests = true;
                dataServiceConfiguration.DataServiceBehavior.AcceptAnyAllRequests = true;
                dataServiceConfiguration.DataServiceBehavior.AcceptProjectionRequests = true;
                dataServiceConfiguration.DataServiceBehavior.AcceptReplaceFunctionInQuery = true;
                dataServiceConfiguration.DataServiceBehavior.AcceptSpatialLiteralsInQuery = true;
                dataServiceConfiguration.DataServiceBehavior.InvokeInterceptorsOnLinkDelete = true;
                dataServiceConfiguration.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;

                IEnumerable<string> entitySetNames = EntitySetHelper.GetEntitySetNames<T>();

                foreach (string entitySetName in entitySetNames)
                    dataServiceConfiguration.SetEntitySetAccessRule(entitySetName, EntitySetRights.All);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}