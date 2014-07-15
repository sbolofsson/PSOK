using System;
using System.Runtime.Serialization;

namespace PSOK.Kademlia
{
    /// <summary>
    /// An item in the DHT.
    /// </summary>
    [DataContract, Serializable]
    internal class Item : IItem
    {
        private DateTime? _expiration;

        /// <summary>
        /// Constructs a new <see cref="Item"/>.
        /// </summary>
        public Item() { }

        /// <summary>
        /// Constructs a new <see cref="Item"/> with the specified <see cref="IContact"/>.
        /// </summary>
        /// <param name="contact"></param>
        public Item(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            TimeToLive = Constants.TExpire;
            Contact = contact;
        }

        /// <summary>
        /// The time to live of the <see cref="Item"/>.
        /// </summary>
        [DataMember]
        public int TimeToLive { get; set; }

        /// <summary>
        /// Indicates the expiration time of the <see cref="Item"/>.
        /// </summary>
        [IgnoreDataMember]
        public DateTime Expiration
        {
            get
            {
                return _expiration != null ?
                    _expiration.Value :
                    (_expiration = DateTime.Now.AddSeconds(TimeToLive)).Value;
            }
        }

        /// <summary>
        /// Indicates whether the <see cref="Item"/> has expired.
        /// </summary>
        [IgnoreDataMember]
        public bool IsExpired
        {
            get
            {
                return DateTime.Now > Expiration;
            }
        }

        /// <summary>
        /// The <see cref="IContact"/> of the <see cref="Item"/>.
        /// Contains information about the <see cref="INode"/> which published the <see cref="Item"/>.
        /// </summary>
        [DataMember]
        public IContact Contact { get; set; }

        /// <summary>
        /// Checks whether two <see cref="Item" />s are equal based on their <see cref="Contact" />.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Item item = obj as Item;
            return item != null && item.Contact.Equals(Contact);
        }

        /// <summary>
        /// Computes the instance hash of the <see cref="Item" />.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Contact.GetHashCode();
        }
    }
}
