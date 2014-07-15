using System;
using System.Runtime.Serialization;

namespace PSOK.Kademlia.Reports
{
    /// <summary>
    /// Indicates the status of an <see cref="INode" />.
    /// </summary>
    [DataContract, Serializable]
    internal class NodeStatus : INodeStatus
    {
        /// <summary>
        /// Indicates if the <see cref="Node" /> is fully initialized.
        /// </summary>
        [DataMember]
        public bool IsNodeInitialized { get; set; }

        /// <summary>
        /// Indicates the size of the <see cref="Node" />'s <see cref="IBucketContainer" />.
        /// </summary>
        [DataMember]
        public int BucketContainerSize { get; set; }

        /// <summary>
        /// The base URL of the <see cref="Node" />.
        /// </summary>
        [DataMember]
        public string BaseUrl { get; set; }

        /// <summary>
        /// The id of the <see cref="Node" />.
        /// </summary>
        [DataMember]
        public string NodeId { get; set; }

        /// <summary>
        /// Indicates the status of the <see cref="NodeDht"/> associated to the <see cref="Node"/>.
        /// </summary>
        [DataMember]
        public INodeDhtStatus DhtStatus { get; set; }

        /// <summary>
        /// Retrieves the contact information of the <see cref="Node" />.
        /// </summary>
        /// <returns></returns>
        public IContact GetContact()
        {
            return new Contact(BaseUrl, NodeId);
        }

        /// <summary>
        /// Resets the status object.
        /// </summary>
        public void Reset()
        {
            IsNodeInitialized = false;
            BucketContainerSize = 0;
            DhtStatus = null;
        }
    }
}