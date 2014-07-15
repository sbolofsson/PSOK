using System.Collections.Generic;
using PSOK.Kademlia;
using PSOK.Kernel.Pipelines;
using PSOK.PublishSubscribe.Messages;

namespace PSOK.PublishSubscribe.Pipelines
{
    /// <summary>
    /// Argument used for the publish <see cref="Pipeline" />.
    /// </summary>
    public class PublishArgs : PubSubArgs
    {
        /// <summary>
        /// The <see cref="IPublish{T}" /> which caused the current Pipeline execution.
        /// </summary>
        public IPublish<Message> Publish { get; set; }

        /// <summary>
        /// The subscribers who should receive the <see cref="Publish" />.
        /// </summary>
        public IEnumerable<IContact> Subscribers { get; set; }
    }
}