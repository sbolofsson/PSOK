using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kademlia.Services;
using PSOK.Kernel;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Collections;
using PSOK.Kernel.Encoding;
using log4net;

namespace PSOK.Kademlia
{
    /// <summary>
    /// A container of <see cref="IContact" />s.
    /// </summary>
    internal class Bucket : IBucket
    {
        // Instance fields
        private static readonly ILog Log = LogManager.GetLogger(typeof (Bucket));
        private readonly BoundedCollection<IContact> _contacts;
        private readonly INode _owner;
        private readonly int _index;
        private DateTime? _lastNodeLookup;

        private readonly ReaderWriterLockSlim _contactsReaderWriterLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// Constructs a new <see cref="Bucket" /> with the specified owner and index.
        /// </summary>
        /// <param name="owner">The <see cref="Node" /> who owns the <see cref="Bucket" /></param>
        /// <param name="index"></param>
        public Bucket(INode owner, int index)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
            _index = index;
            _contacts = new BoundedCollection<IContact>(Constants.K);
        }

        /// <summary>
        /// Event which is raised when the <see cref="Bucket"/> is changed.
        /// </summary>
        public event Action<IContact, BucketOperation> OnChanged;

        /// <summary>
        /// The list of <see cref="IContact" />s contained in the <see cref="Bucket" /> sorted by last contact time.
        /// </summary>
        public IEnumerable<IContact> Contacts
        {
            get
            {
                try
                {
                    _contactsReaderWriterLock.EnterReadLock();
                    return _contacts.ToList().AsReadOnly();
                }
                finally
                {
                    _contactsReaderWriterLock.ExitReadLock();
                }
            }
        }

        /// <summary>
        /// Adds an <see cref="IContact" /> to the <see cref="Bucket" /> and maintains the sorting.
        /// </summary>
        /// <param name="contact"></param>
        public void Add(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            // Prevent a Kademlia from registering himself as a contact
            if (contact.Equals(_owner.Contact))
                return;

            BucketOperation operation = BucketOperation.Add;
            IContact firstContact = null;
            BucketOperation operationFirstContact = BucketOperation.Update;

            try
            {
                // Obtain lock
                _contactsReaderWriterLock.EnterWriteLock();

                // 2 cases:
                //   1. contact is in list
                //   2. contact is not in list
                if (_contacts.Remove(contact))
                    operation = BucketOperation.Update;

                // Check if there is space in the bucket
                // If we just successfully removed the bucket we know it is an update operation,
                // else it will be an add operation
                if (_contacts.Count < Constants.K)
                {
                    // There is space
                    _contacts.Add(contact);
                }
                else
                {
                    // There is not space, try to contact the least recently seen contact (located at the head)
                    firstContact = _contacts.First();
                    
                    bool replyReceived = false;

                    try
                    {
                        using (NodeProxy nodeProxy = new NodeProxy(_owner, firstContact, false))
                        {
                            replyReceived = nodeProxy.Context.Ping();
                        }
                    }
                    catch (Exception) { { } }

                    // Remove the least recently seen contact from head
                    _contacts.Remove(firstContact);

                    if (replyReceived)
                    {
                        // It responded, so move it to the tail of the list, and issue an update event
                        operationFirstContact = BucketOperation.Update;
                        _contacts.Add(firstContact);

                        // This means that we discard the new contact
                        operation = BucketOperation.Discard;
                    }
                    else
                    {
                        // The least recently seen contact did not respond, so issue a remove event
                        // and add the new contact to the tail, as it is the most recently seen
                        operationFirstContact = BucketOperation.Remove;
                        _contacts.Add(contact);

                        // This means that we add the new contact
                        operation = BucketOperation.Add;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            finally
            {
                // Release lock
                _contactsReaderWriterLock.ExitWriteLock();
            }

            if(firstContact != null)
                RaiseOnChanged(firstContact, operationFirstContact);
            RaiseOnChanged(contact, operation);
        }

        /// <summary>
        /// Removes an <see cref="IContact" /> from the <see cref="Bucket" /> and maintains the sorting.
        /// </summary>
        /// <param name="contact"></param>
        public void Remove(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            bool remove;

            try
            {
                _contactsReaderWriterLock.EnterWriteLock();
                remove = _contacts.Remove(contact);
            }
            finally
            {
                _contactsReaderWriterLock.ExitWriteLock();
            }

            if(remove)
                RaiseOnChanged(contact, BucketOperation.Remove);
        }

        /// <summary>
        /// Indicates if the <see cref="Bucket"/> contains the specified <see cref="IContact"/>.
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public bool Contains(IContact contact)
        {
            if(contact == null)
                throw new ArgumentNullException("contact");

            IEnumerable<IContact> contacts = Contacts;
            return contacts.Any(x => x.Equals(contact));
        }


        /// <summary>
        /// Computes the bucket index for the specified node ids.
        /// </summary>
        /// <param name="nodeId1"></param>
        /// <param name="nodeId2"></param>
        /// <returns></returns>
        public static int Index(string nodeId1, string nodeId2)
        {
            if (string.IsNullOrEmpty(nodeId1))
                throw new ArgumentNullException("nodeId1");

            if (string.IsNullOrEmpty(nodeId2))
                throw new ArgumentNullException("nodeId2");

            string cacheKey = string.Format("bucket#{0}#{1}", nodeId1, nodeId2);

            object index = CacheManager.GetOrAdd<object>(cacheKey, () =>
            {
                byte[] distance = Node.DistanceBytes(nodeId1, nodeId2);
                for (int i = 0; i < distance.Length; i++)
                {
                    byte word = distance[i];
                    if (word == 0)
                        continue;

                    return i * 8 + BitOperations.MostSignificantBit(word);
                }
                return distance.Length*8 - 1;
            }, new CachingOptions<object>
            {
                EnableCaching = true,
                Expiration = new TimeSpan(24, 0, 0)
            });
            return (int) index;
        }

        /// <summary>
        /// Refreshes the <see cref="IBucket"/>.
        /// </summary>
        public void Refresh(bool force = false)
        {
            if (!force && (_lastNodeLookup == null || ((DateTime.Now - _lastNodeLookup.Value).TotalSeconds < Constants.TRefresh)))
                return;

            string key = KeyProvider.GetRandomKey(_owner.Contact.NodeId, _index);
            _owner.IterativeFindNode(key);
        }

        /// <summary>
        /// Updates the lookup timestamp.
        /// </summary>
        public void SetTimestamp()
        {
            _lastNodeLookup = DateTime.Now;
        }

        /// <summary>
        /// Raised the <see cref="OnChanged"/> event.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="operation"></param>
        private void RaiseOnChanged(IContact contact, BucketOperation operation)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            if (OnChanged != null)
                OnChanged(contact, operation);
        }
    }
}