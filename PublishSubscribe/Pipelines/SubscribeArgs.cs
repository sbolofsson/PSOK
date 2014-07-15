namespace PSOK.PublishSubscribe.Pipelines
{
    /// <summary>
    /// Argument used for the subscribe Pipeline.
    /// </summary>
    public class SubscribeArgs : PubSubArgs
    {
        /// <summary>
        /// The <see cref="ISubscription" /> which caused the current Pipeline execution.
        /// </summary>
        public ISubscription Subscription { get; set; }
    }
}