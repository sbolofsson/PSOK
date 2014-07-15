using System.Collections.Generic;
using System.Linq;

namespace PSOK.Kademlia.Tasks
{
    /// <summary>
    /// Replicates all entries in the DHT.
    /// </summary>
    internal class Replicate
    {

        public void Run()
        {
            IEnumerable<INode> nodes = Node.Nodes.Where(x => x.IsInitialized).ToList();
            foreach (INode node in nodes)
            {
                node.Dht.Replicate();
            }
        }
    }
}
