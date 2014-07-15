using PSOK.Kademlia;
using PSOK.Kernel.Pipelines;

namespace PSOK.PublishSubscribe.Pipelines.Shutdown
{
    internal class Dispose : IProcessor<ShutdownArgs>
    {
        public void Execute(ShutdownArgs args)
        {
            foreach (IPeer peer in Peer.Peers)
            {
                peer.Dispose();
            }
            BootstrapperNode.DisposeInstance();
        }
    }
}
