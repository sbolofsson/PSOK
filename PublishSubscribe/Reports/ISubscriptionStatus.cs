namespace PSOK.PublishSubscribe.Reports
{
    /// <summary>
    /// Indicates the status of a <see cref="ISubscription"/>.
    /// </summary>
    public interface ISubscriptionStatus
    {
        /// <summary>
        /// The <see cref="ISubscription"/> key.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// The <see cref="ISubscription"/> type.
        /// </summary>
        string Type { get; }
    }
}
