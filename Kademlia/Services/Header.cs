using System;
using System.ServiceModel.Channels;
using System.Xml;
using PSOK.Kernel.Serialization;

namespace PSOK.Kademlia.Services
{
    /// <summary>
    /// This class acts as a header sending custom information over WCF
    /// </summary>
    internal class Header : MessageHeader
    {
        private readonly IContact _contact;

        private const string HeaderName = "Contact";
        private const string HeaderNameSpace = "Kademlia";

        public Header(IContact contact)
        {
            if (contact == null)
                throw new ArgumentNullException("contact");

            _contact = contact;
        }

        /// <summary>
        /// Overrides the OnWriteHeaderContents method
        /// Gets called by the framework when creating the headers
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="messageVersion"></param>
        protected override void OnWriteHeaderContents(XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            string header = Serializer.Serialize(_contact);
            writer.WriteElementString(Name, Namespace, header);
        }

        /// <summary>
        /// The name of the header
        /// </summary>
        public override string Name
        {
            get { return HeaderName; }
        }

        /// <summary>
        /// The namespace of the header
        /// </summary>
        public override string Namespace
        {
            get { return HeaderNameSpace; }
        }

        /// <summary>
        /// Utility method for reading the header
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static IContact ReadHeader(Message message)
        {
            int index = message.Headers.FindHeader(HeaderName, HeaderNameSpace);

            if (index == -1)
                return null;

            XmlNode[] xmlNode = message.Headers.GetHeader<XmlNode[]>(index);
            return Serializer.Deserialize<IContact>(xmlNode[0].InnerText);
        }
    }
}