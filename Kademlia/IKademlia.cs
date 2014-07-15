using System.Collections.Generic;
using System.ServiceModel;
using PSOK.Kademlia.Lookups;
using PSOK.Kernel.Services;

namespace PSOK.Kademlia
{
    /// <summary>
    /// This interface defines all Kademlia primitives.
    /// </summary>
    [ServiceContract, ServiceKnownType("GetKnownTypes", typeof(KnownTypeResolver))]
    internal interface IKademlia : IServiceHost
    {
        /// <summary>
        /// Pings the node.
        /// </summary>
        [OperationContract]
        bool Ping();

        /// <summary>
        /// Instructs the node to store contact information about a node having the data corresponding to the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        [OperationContract(IsOneWay = false)]
        void Store(string key, IEnumerable<IItem> items);

        /// <summary>
        /// Ask a contact for a list of closest nodes (contacts) which know about the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [OperationContract]
        IEnumerable<IContact> FindNode(string key);

        /// <summary>
        /// Ask a contact for a value, if it is found the value is returned, otherwise it acts like <see cref="FindNode" />.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        [OperationContract]
        IResult FindValue(string key);
    }
}