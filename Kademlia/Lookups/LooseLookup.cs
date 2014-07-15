using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSOK.Kademlia.Lookups
{
    /// <summary>
    /// Performs a loose parallel lookup in Kademlia.
    /// </summary>
    internal class LooseLookup : LookupBase
    {
        /// <summary>
        /// Constructs a new <see cref="StrictLookup"/>.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="key"></param>
        /// <param name="findNode"></param>
        public LooseLookup(INode owner, string key, bool findNode)
            : base(owner, key, findNode)
        {
        }

        /// <summary>
        /// Performs a loose parallel lookup.
        /// </summary>
        /// <returns></returns>
        public override void Lookup()
        {
            Task.WaitAll(new[] { NodeLookupLoose(Constants.Alpha) });
        }

        /// <summary>
        /// Finds a node or a value asynchronously.
        /// </summary>
        /// <returns></returns>
        private async Task NodeLookupLoose(int count)
        {
            try
            {
                IEnumerable<Task> lookups = GetContacts(count)
                    .Select(x => Task.Run(() => FindNodeOrValue(x), CancellationTokenSource.Token)
                        .ContinueWith(async y =>
                        {
                            await NodeLookupLoose(GetNumberOfContacts(y.Result));
                        }, CancellationTokenSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion,
                            TaskScheduler.Current));

                await Task.WhenAll(lookups);
            }
            catch (TaskCanceledException)
            {

            }
        }
    }
}
