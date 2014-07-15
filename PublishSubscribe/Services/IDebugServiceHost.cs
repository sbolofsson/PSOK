using System.Collections.Generic;
using System.ServiceModel;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Reports;

namespace PSOK.PublishSubscribe.Services
{
    /// <summary>
    /// A service host for retrieving and delivering application status reports.
    /// </summary>
    [ServiceContract, ServiceKnownType("GetKnownTypes", typeof(KnownTypeResolver))]
    public interface IDebugServiceHost : IServiceHost
    {
        /// <summary>
        /// Delivers an <see cref="IStatusReport" />.
        /// </summary>
        /// <param name="statusReport"></param>
        [OperationContract(IsOneWay = false)]
        void Deliver(IStatusReport statusReport);

        /// <summary>
        /// Retrives all the <see cref="IStatusReport" />s delivered to this service.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        IEnumerable<IStatusReport> GetReports();
    }
}