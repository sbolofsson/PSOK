namespace PSOK.Kademlia.Reports
{
    /// <summary>
    /// Indicates the status of an entry in the <see cref="INodeDht"/>.
    /// </summary>
    public interface IItemStatus
    {
        /// <summary>
        /// The key of an entry in the <see cref="INodeDht"/>.
        /// </summary>
        string Key { get; }
    }
}
