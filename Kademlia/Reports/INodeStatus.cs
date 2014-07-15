namespace PSOK.Kademlia.Reports
{
    /// <summary>
    /// Indicates the status of an <see cref="INode" />.
    /// </summary>
    public interface INodeStatus
    {
        /// <summary>
        /// Indicates if the <see cref="INode" /> is fully initialized.
        /// </summary>
        bool IsNodeInitialized { get; }

        /// <summary>
        /// Indicates the size of the <see cref="INode" />'s <see cref="IBucketContainer" />.
        /// </summary>
        int BucketContainerSize { get; }

        /// <summary>
        /// The base URL of the <see cref="INode" />.
        /// </summary>
        string BaseUrl { get; }

        /// <summary>
        /// The id of the <see cref="INode" />.
        /// </summary>
        string NodeId { get; }

        /// <summary>
        /// Indicates the status of the <see cref="INodeDht"/> associated to the <see cref="INode"/>.
        /// </summary>
        INodeDhtStatus DhtStatus { get; }

        /// <summary>
        /// Retrieves the contact information of the <see cref="INode" />.
        /// </summary>
        /// <returns></returns>
        IContact GetContact();

        /// <summary>
        /// Resets the status object.
        /// </summary>
        void Reset();
    }
}