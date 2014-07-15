using System;
using PSOK.Kademlia.Lookups;
using PSOK.Kernel.Configuration;
using PSOK.Kernel.Exceptions;

// ReSharper disable InconsistentNaming
namespace PSOK.Kademlia
{
    /// <summary>
    /// Container for constant settings in <see cref="Kademlia"/>.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// The Kademlia parallelism type.
        /// </summary>
        public static Parallelism Parallelism
        {
            get
            {
                if (_parallelism != Parallelism.Unknown)
                    return _parallelism;

                string parallelism = Config.ReadConfig().Kademlia.Parallelism;
                switch (parallelism)
                {
                    case "strict":
                        _parallelism = Parallelism.Strict;
                        break;
                    case "loose":
                        _parallelism = Parallelism.Loose;
                        break;
                    default:
                        throw new ConfigurationException("Could not determine Kademlia parallelism.");
                }
                return _parallelism;
            }
        }

        private static Parallelism _parallelism;

        /// <summary>
        /// The amount of <see cref="IContact" />s allowed in an <see cref="IBucket" />
        /// </summary>
        public static int K
        {
            get { return _k != null ? _k.Value : (_k = Config.ReadConfig().Kademlia.Bucketsize).Value; }
        }

        private static int? _k;

        /// <summary>
        /// The amount of <see cref="IBucket" />s
        /// </summary>
        public static int B
        {
            get { return _b != null ? _b.Value : (_b = Config.ReadConfig().Kademlia.Buckets).Value; }
        }

        private static int? _b;

        /// <summary>
        /// The concurrency parameter
        /// </summary>
        public static int Alpha
        {
            get { return _alpha != null ? _alpha.Value : (_alpha = Config.ReadConfig().Kademlia.Concurrency).Value; }
        }

        private static int? _alpha;

        /// <summary>
        /// The time in seconds until a key/value pair expires
        /// </summary>
        public static int TExpire
        {
            get
            {
                return _tExpire != null
                    ? _tExpire.Value
                    : (_tExpire = (int) TimeSpan.Parse(Config.ReadConfig().Kademlia.Dht.Expiration).TotalSeconds).Value;
            }
        }

        private static int? _tExpire;

        /// <summary>
        /// The time in seconds before an <see cref="IBucket" /> must be refreshed
        /// </summary>
        public static int TRefresh = 3600;

        /// <summary>
        /// The time in seconds between an <see cref="Node" /> have to republish its database
        /// </summary>
        public static int TReplicate = 3600;

        /// <summary>
        /// The time in seconds indicating when an <see cref="Node" /> have to republish a key/value pair
        /// </summary>
        public static int TRepublish = 86400;
    }
}
// ReSharper restore InconsistentNaming