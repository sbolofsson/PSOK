namespace PSOK.Kademlia.Lookups
{
    /// <summary>
    /// Indicates parallelism settings for Kademlia.
    /// </summary>
    internal enum Parallelism
    {
        /// <summary>
        /// An unknown value.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Specifies strict parallelism.
        /// </summary>
        Strict = 100,

        /// <summary>
        /// Specifies loose parallelism.
        /// </summary>
        Loose = 200
    }
}