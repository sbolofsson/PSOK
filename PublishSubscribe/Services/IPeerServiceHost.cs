using System.ServiceModel;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe.Services
{
    /// <summary>
    /// A peer service host for intra process communication between <see cref="IPeer" />s.
    /// </summary>
    [ServiceContract, ServiceKnownType("GetKnownTypes", typeof(KnownTypeResolver))]
    public interface IPeerServiceHost : IServiceHost
    {
        /// <summary>
        /// Is invoked when a relevant <see cref="Message" /> is published.
        /// </summary>
        /// <param name="publish">The publish which caused the callback.</param>
        [OperationContract]
        void Callback(IPublish<Message> publish);

        /// <summary>
        /// Checks if the <see cref="IPeerServiceHost" /> is alive.
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        bool Ping();
    }
}