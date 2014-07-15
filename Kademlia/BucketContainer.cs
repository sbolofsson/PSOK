using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kernel;

namespace PSOK.Kademlia
{
    /// <summary>
    /// A container of <see cref="IBucket" />s.
    /// </summary>
    internal class BucketContainer : IBucketContainer
    {
        // private static readonly ILog Log = LogManager.GetLogger(typeof(BucketContainer));

        private readonly IList<IBucket> _buckets = new List<IBucket>(Constants.B);
        private readonly INode _owner;

        private readonly ReaderWriterLockSlim _bucketsReaderWriterLock =
            new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        private readonly SemaphoreSlim _initializeLock = new SemaphoreSlim(1, 1);
        private bool _isInitialized;

        /// <summary>
        /// Event which is raised when the <see cref="BucketContainer"/> is changed.
        /// </summary>
        public event Action<IContact, BucketOperation> OnChanged;

        /// <summary>
        /// Constructs a new <see cref="BucketContainer" />.
        /// </summary>
        /// <param name="owner"></param>
        public BucketContainer(INode owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        #region IBucketContainer methods

        /// <summary>
        /// Indicates the number of <see cref="IContact"/>s in the <see cref="BucketContainer"/>.
        /// </summary>
        public int Size { get { return GetAllBuckets().Sum(x => x.Contacts.Count()); } }

        /// <summary>
        /// Initializes the <see cref="BucketContainer" />.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                _initializeLock.Wait();

                if (_isInitialized)
                    return;

                // Fill up bucket list
                _bucketsReaderWriterLock.EnterWriteLock();
                for (int i = 0; i < Constants.B; i++)
                {
                    IBucket bucket = new Bucket(_owner, i);
                    bucket.OnChanged += RaiseOnChanged;
                    _buckets.Add(bucket);
                }
                _bucketsReaderWriterLock.ExitWriteLock();

                _isInitialized = true;
            }
            finally
            {
                _initializeLock.Release();
            }
        }

        /// <summary>
        /// Adds an <see cref="IContact" /> to the relevant <see cref="IBucket" />.
        /// </summary>
        /// <param name="contact"></param>
        public void Add(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            IBucket bucket = GetBucketForKey(contact.NodeId);
            bucket.Add(contact);
        }

        /// <summary>
        /// Removes an <see cref="IContact" /> from the relevant <see cref="IBucket" />.
        /// </summary>
        /// <param name="contact"></param>
        public void Remove(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            IBucket bucket = GetBucketForKey(contact.NodeId);
            bucket.Remove(contact);
        }

        /// <summary>
        /// Finds the closest contacts for the specified key.
        /// </summary>
        /// <param name="key">The key to retrieve the <see cref="IContact" />s based on.</param>
        /// <param name="count">The maximum number of <see cref="IContact" />s to retrieve.</param>
        /// <returns></returns>
        public List<IContact> GetClosestContacts(string key, int count)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            IReadOnlyList<IBucket> buckets = GetAllBuckets();
            return buckets.SelectMany(x => x.Contacts)
                .OrderBy(x => Node.Distance(x.NodeId, key)).Take(count).ToList();
        }

        /// <summary>
        /// Indicates if the <see cref="BucketContainer"/> contains the specified <see cref="IContact"/>.
        /// </summary>
        /// <returns></returns>
        public bool Contains(IContact contact)
        {
            IReadOnlyList<IBucket> buckets = GetAllBuckets();
            return buckets.Any(x => x.Contains(contact));
        }

        /// <summary>
        /// Refreshes all <see cref="IBucket"/>s further away than the closest neighbour.
        /// </summary>
        public void RefreshNeighbours()
        {
            bool closestNeighbourFound = false;
            IReadOnlyList<IBucket> buckets = GetAllBuckets();
            foreach (IBucket bucket in buckets)
            {
                if (!bucket.Contacts.Any())
                    continue;

                if (!closestNeighbourFound)
                {
                    closestNeighbourFound = true;
                }
                else
                {
                    bucket.Refresh(true);
                }
            }
        }

        /// <summary>
        /// Refreshes all the <see cref="IBucket" />s.
        /// </summary>
        public void RefreshBuckets()
        {
            IEnumerable<IBucket> buckets = GetAllBuckets();
            foreach (IBucket bucket in buckets)
            {
                bucket.Refresh();
            }
        }

        /// <summary>
        /// Updates the timestamp for the appropriate <see cref="Bucket"/>.
        /// </summary>
        /// <param name="key"></param>
        public void SetTimestamp(string key)
        {
            IBucket bucket = GetBucketForKey(key);
            bucket.SetTimestamp();
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Creates a copy of all <see cref="IBucket" />s.
        /// </summary>
        private IReadOnlyList<IBucket> GetAllBuckets()
        {
            try
            {
                _bucketsReaderWriterLock.EnterReadLock();
                return _buckets.ToList().AsReadOnly();
            }
            finally
            {
                _bucketsReaderWriterLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Get the <see cref="IBucket" /> for the relevant key/Kademlia id
        /// </summary>
        /// <param name="key"></param>
        private IBucket GetBucketForKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            IReadOnlyList<IBucket> buckets = GetAllBuckets();
            int index = Bucket.Index(key, _owner.Contact.NodeId);
            return buckets[index];
        }

        /// <summary>
        /// Raised the <see cref="OnChanged"/> event.
        /// </summary>
        /// <param name="contact"></param>
        /// <param name="operation"></param>
        private void RaiseOnChanged(IContact contact, BucketOperation operation)
        {
            if (OnChanged != null)
                OnChanged(contact, operation);
        }

        #endregion
    }
}