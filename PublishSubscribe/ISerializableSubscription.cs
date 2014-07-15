namespace PSOK.PublishSubscribe
{
    /// <summary>
    /// A serializable subscription.
    /// </summary>
    public interface ISerializableSubscription
    {
        /// <summary>
        /// Converts the <see cref="ISerializableSubscription"/> to an <see cref="ISubscription"/>.
        /// </summary>
        /// <returns></returns>
        ISubscription ToSubscription();
    }
}
