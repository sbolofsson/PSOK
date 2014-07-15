using System.Collections.Generic;
using System.Linq;

namespace PSOK.Kademlia.Tasks
{
    /// <summary>
    /// Refreshes all <see cref="IBucket" />s.
    /// </summary>
    internal class RefreshBuckets
    {
        public void Run()
        {
            IEnumerable<INode> nodes = Node.Nodes.Where(x => x.IsInitialized).ToList();
            foreach (INode node in nodes)
            {
                node.BucketContainer.RefreshBuckets();
            }
        }
    }
}