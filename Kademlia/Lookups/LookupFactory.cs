using PSOK.Kademlia.Exceptions;

namespace PSOK.Kademlia.Lookups
{
    /// <summary>
    /// Factory class responsible for instantiating new <see cref="LookupBase"/> classes.
    /// </summary>
    internal static class LookupFactory
    {
        /// <summary>
        /// Creates a new lookup based on the specified owner, key and findNode.
        /// </summary>
        /// <param name="owner">The <see cref="INode"/> initiating the lookup.</param>
        /// <param name="key">The key to find.</param>
        /// <param name="findNode">Indicates if nodes or a value is searched for.</param>
        /// <returns></returns>
        public static LookupBase GetLookup(INode owner, string key, bool findNode)
        {
            switch (Constants.Parallelism)
            {
                case Parallelism.Strict:
                    return new StrictLookup(owner, key, findNode);

                case Parallelism.Loose:
                    return new LooseLookup(owner, key, findNode);

                default:
                    throw new LookupException("A Lookup was started with an unrecognized parallelism setting.");
            }
        }
    }
}
