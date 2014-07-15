using System;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace PSOK.Kademlia
{
    /// <summary>
    /// A set of contact information.
    /// </summary>
    [DataContract, Serializable]
    internal class Contact : IContact
    {
        /// <summary>
        /// Constructs a new <see cref="Contact" />.
        /// </summary>
        public Contact()
        {
        }

        /// <summary>
        /// Constructs a new <see cref="Contact" />.
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="nodeId"></param>
        public Contact(string baseUrl, string nodeId)
        {
            BaseUrl = baseUrl;
            NodeId = nodeId;
        }

        /// <summary>
        /// The unique id of the <see cref="Contact" />.
        /// </summary>
        [DataMember]
        public string NodeId { get; private set; }

        /// <summary>
        /// The base URL of the <see cref="Contact" />.
        /// </summary>
        [DataMember]
        public string BaseUrl { get; private set; }

        /// <summary>
        /// The public node URL of the <see cref="Contact" />.
        /// </summary>
        [IgnoreDataMember]
        public string NodeUrl
        {
            get { return string.Format("{0}/kademlia/{1}/node", BaseUrl, NodeId); }
        }

        /// <summary>
        /// Extra information associated with the <see cref="IContact"/>.
        /// </summary>
        [DataMember]
        public object Data { get; set; }

        /// <summary>
        /// Checks whether two <see cref="Contact" />s are equal based on their <see cref="NodeId" />.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            Contact contact = obj as Contact;
            return contact != null &&
                string.Equals(NodeId, contact.NodeId, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Computes the instance hash of the <see cref="Contact" />.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return NodeId.GetHashCode();
        }

        /// <summary>
        /// Clones the <see cref="Contact"/>.
        /// </summary>
        /// <returns></returns>
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Clones the <see cref="Contact"/>.
        /// </summary>
        /// <returns></returns>
        public IContact Copy()
        {
            return (this as ICloneable).Clone() as IContact;
        }

        /// <summary>
        /// Returns the base url and node id.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("BaseUrl: '{0}', NodeId: '{1}'.", BaseUrl, NodeId);
        }

        /// <summary>
        /// Converts the specified uri to an <see cref="IContact"/>
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static IContact ToContact(Uri uri)
        {
            string baseUrl = uri.GetLeftPart(UriPartial.Authority);
            const string pattern = "/kademlia/(.*?)/node";
            Match match = Regex.Match(uri.ToString(), pattern, RegexOptions.IgnoreCase);
            string nodeId = match.Groups[1].Value;
            return new Contact(baseUrl, nodeId);
        }
    }
}