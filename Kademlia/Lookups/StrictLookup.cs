using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PSOK.Kademlia.Lookups
{
    /// <summary>
    /// Performs a bounded parallel lookup in Kademlia.
    /// </summary>
    internal class StrictLookup : LookupBase
    {
        //private static readonly ILog Log = LogManager.GetLogger(typeof (StrictLookup));

        /// <summary>
        /// Constructs a new <see cref="StrictLookup"/>.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="key"></param>
        /// <param name="findNode"></param>
        public StrictLookup(INode owner, string key, bool findNode)
            : base(owner, key, findNode)
        {
        }

        /// <summary>
        /// Performs a strict parallel lookup.
        /// </summary>
        /// <returns></returns>
        public override void Lookup()
        {
            Task.WaitAll(GetContacts(Constants.Alpha).Select(FindNodeOrValueRecursivelyAsync).ToArray());
        }

        /// <summary>
        /// Recursively asynchronously finds a node or a value based on the specified contact.
        /// Used in a strict parallel lookup.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        private async Task FindNodeOrValueRecursivelyAsync(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            try
            {
                await Task.Run(() => FindNodeOrValueRecursively(contact), CancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {

            }
        }

        /// <summary>
        /// Recursively finds a node or a value based on the specified contact.
        /// Used in a bounded parallel lookup.
        /// </summary>
        /// <param name="contact"></param>
        private void FindNodeOrValueRecursively(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            IEnumerable<IContact> responses = FindNodeOrValue(contact);

            GetNumberOfContacts(responses);

            IContact closestContact = GetContacts(1).FirstOrDefault();

            if (closestContact == null)
                return;

            FindNodeOrValueRecursively(closestContact);
        }
    }
}