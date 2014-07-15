using System.Collections.Generic;
using System.Linq;

namespace PSOK.Kademlia.Tasks
{
    /// <summary>
    /// Republishes all entries published earlier by this node.
    /// </summary>
    internal class Republish
    {
        public void Run()
        {
            IEnumerable<INode> nodes = Node.Nodes.Where(x => x.IsInitialized).ToList();
            foreach (INode node in nodes)
            {
                node.Republish();
            }
        }
    }
}