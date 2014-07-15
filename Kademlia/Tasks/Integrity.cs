using System.Collections.Generic;
using System.Linq;

namespace PSOK.Kademlia.Tasks
{
    /// <summary>
    /// Ensures the integrity of all DHT entries.
    /// </summary>
    internal class Integrity
    {
        public void Run()
        {
            IEnumerable<INode> nodes = Node.Nodes.Where(x => x.IsInitialized).ToList();
            foreach (INode node in nodes)
            {
                node.Dht.EnsureIntegrity();
            }
        }
    }
}
