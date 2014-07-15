using PSOK.Kernel.Pipelines;

namespace PSOK.PublishSubscribe.Pipelines
{
    /// <summary>
    /// Base class for P2P related <see cref="Pipeline" />s.
    /// </summary>
    public abstract class PubSubArgs : PipelineArgs
    {
        /// <summary>
        /// Indicates if the current P2P args are valid (there is an initialized <see cref="IPeer"/> attached).
        /// </summary>
        public bool IsValid
        {
            get { return Peer != null && Peer.IsInitialized; }
        }

        /// <summary>
        /// Indicates the <see cref="IPeer" /> who started the <see cref="Pipeline" />.
        /// </summary>
        public IPeer Peer { get; set; }
    }
}