using PSOK.Kernel.Pipelines;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe.Pipelines
{
    /// <summary>
    /// Argument used for the callback <see cref="Pipeline" />.
    /// </summary>
    public class CallbackArgs : PubSubArgs
    {
        /// <summary>
        /// The <see cref="IPublish{T}"/> which caused the callback.
        /// </summary>
        public IPublish<Message> Publish { get; set; }
    }
}