using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kademlia.Services;
using log4net;

// ReSharper disable PossibleMultipleEnumeration
namespace PSOK.Kademlia.Lookups
{
    /// <summary>
    /// Defines a lookup capable of looking up nodes in Kademlia.
    /// </summary>
    internal abstract class LookupBase
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(LookupBase));

        // Instance fields

        // Constructor fields
        private readonly INode _owner;
        private readonly string _key;
        private readonly bool _findNode;

        // State tracking fields
        private volatile IEnumerable<IItem> _value;
        private volatile IContact _valueFoundAt;
        private double _closestNodeDistance;

        // Other contacted nodes' contact information
        private readonly HashSet<IContact> _alreadyContacted = new HashSet<IContact>();
        private readonly HashSet<IContact> _shortList;

        // Used to cancel lookups when a value has been found
        protected readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        // Locks
        private readonly ReaderWriterLockSlim _alreadyContactedLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly ReaderWriterLockSlim _shortListLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly SemaphoreSlim _distanceLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Constructs a new <see cref="LookupBase" />.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="key"></param>
        /// <param name="findNode"></param>
        protected LookupBase(INode owner, string key, bool findNode)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            _owner = owner;
            _key = key;
            _findNode = findNode;

            _shortList = new HashSet<IContact>(_owner.BucketContainer.GetClosestContacts(_key, Constants.Alpha));

            _owner.BucketContainer.SetTimestamp(key);

            _alreadyContacted.Add(owner.Contact);

            if (_shortList.Any())
                _closestNodeDistance = Node.Distance(_shortList.First().NodeId, _key);
        }

        /// <summary>
        /// Performs an asynchronous lookup.
        /// </summary>
        /// <returns></returns>
        public IResult LookupAsync()
        {
            if (!_shortList.Any())
                return null;

            Lookup();

            if (_value != null)
            {
                ReplicateLookup();
                return new Result(_value);
            }

            return new Result(
                    _shortList
                    .OrderBy(x => Node.Distance(x.NodeId, _key))
                    .Take(Constants.K).ToList()
                );
        }

        public abstract void Lookup();

        #region Shared lookup methods

        /// <summary>
        /// This is the basic lookup algorithm which can both find values and nodes.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        protected IEnumerable<IContact> FindNodeOrValue(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            if (contact.Equals(_owner.Contact))
                return new List<IContact>();

            IEnumerable<IContact> contacts = null;

            if (_findNode)
            {
                contacts = FindNode(contact);
            }
            else if (_value == null)
            {
                contacts = FindValue(contact);
            }

            if (contacts == null)
                return new List<IContact>();
            
            contacts = contacts
                .Where(x => !x.Equals(_owner.Contact))
                .ToList();
            
            AddToShortList(contacts);

            return contacts;
        }

        /// <summary>
        /// Finds a node in Kademlia.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        private IEnumerable<IContact> FindNode(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            try
            {
                using (NodeProxy nodeProxy = new NodeProxy(_owner, contact, true))
                {
                    IEnumerable<IContact> nodes = nodeProxy.Context.FindNode(_key);
                    return nodes;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, contact);
            }

            return new List<IContact>();
        }

        /// <summary>
        /// Finds a value in Kademlia.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        private IEnumerable<IContact> FindValue(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            IResult result = null;
            try
            {
                using (NodeProxy nodeProxy = new NodeProxy(_owner, contact, true))
                {
                    result = nodeProxy.Context.FindValue(_key);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, contact);
            }

            if (result != null && result.Entries != null)
            {
                _value = result.Entries;
                _valueFoundAt = contact;
                AbortAllLookups();
            }
            else if (result != null && result.Contacts != null)
            {
                return result.Contacts.ToList();
            }

            return new List<IContact>();
        }

        /// <summary>
        /// Stores the (key, value) pair at the closest node, observed to the key that did not return the value.
        /// </summary>
        private void ReplicateLookup()
        {
            IContact closestNode = _shortList
                    .OrderBy(x => Node.Distance(x.NodeId, _key))
                    .FirstOrDefault(x => !x.Equals(_valueFoundAt));
            if (closestNode == null)
                return;

            try
            {
                IItem item = new Item(_valueFoundAt);
                using (NodeProxy nodeProxy = new NodeProxy(_owner, closestNode, false))
                {
                    nodeProxy.Context.Store(_key, new List<IItem> { item });
                }
            }
            catch (Exception ex)
            {
                _owner.PingContact(closestNode);
                if (!Kernel.Exceptions.Exception.IsEndpointDown(ex))
                    Log.Error(ex);
            }
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Handles the specified <see cref="Exception" />.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="contact"></param>
        private void HandleException(Exception exception, IContact contact)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            if (contact == null)
                throw new ArgumentNullException("contact");

            _owner.PingContact(contact);

            if (!Kernel.Exceptions.Exception.IsEndpointDown(exception))
            {
                Log.Error(exception);
            }
        }

        /// <summary>
        /// Aborts all lookups immediately.
        /// </summary>
        private void AbortAllLookups()
        {
            CancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Retrieves the alpha closest <see cref="IContact" />s which have not yet been contacted from the short list.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<IContact> GetContacts(int count)
        {
            try
            {

                _alreadyContactedLock.EnterWriteLock();
                _shortListLock.EnterReadLock();

                if (
                    _shortList.OrderBy(x => Node.Distance(x.NodeId, _key))
                        .Take(Constants.K)
                        .All(_alreadyContacted.Contains))
                    return new List<IContact>();

                IEnumerable<IContact> contacts = _shortList.Except(_alreadyContacted)
                    .OrderBy(x => Node.Distance(x.NodeId, _key))
                    .Take(count)
                    .ToList();

                foreach (IContact contact in contacts)
                {
                    _alreadyContacted.Add(contact);
                }

                return contacts;
            }
            finally
            {
                _shortListLock.ExitReadLock();
                _alreadyContactedLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Adds the specified collection of contacts to the short list.
        /// </summary>
        /// <param name="contacts">The contacts to add to the short list.</param>
        protected void AddToShortList(IEnumerable<IContact> contacts)
        {
            if (contacts == null)
                throw new ArgumentNullException("contacts");

            IEnumerable<IContact> contactList = contacts as IList<IContact> ?? contacts.ToList();

            if (!contactList.Any())
                return;

            _shortListLock.EnterWriteLock();
            _shortList.UnionWith(contactList);
            _shortListLock.ExitWriteLock();
        }

        /// <summary>
        /// Updates the <see cref="_closestNodeDistance" /> if the specified distance is smaller.
        /// </summary>
        /// <param name="responses">The responses received from a node lookup.</param>
        /// <returns>True if the responses should result in another lookup cycle.<see cref="_closestNodeDistance" />.</returns>
        protected int GetNumberOfContacts(IEnumerable<IContact> responses)
        {
            if (responses == null || !responses.Any())
                return Constants.K;

            double distance = Node.Distance(responses.First().NodeId, _key);

            try
            {
                _distanceLock.Wait();

                // Check if we have found a closer node
                if (distance >= _closestNodeDistance)
                    return Constants.K;

                // We found a closer node
                _closestNodeDistance = distance;
                return Constants.Alpha;
            }
            finally
            {
                _distanceLock.Release();
            }
        }

        #endregion

    }
}
// ReSharper restore PossibleMultipleEnumeration