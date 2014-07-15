using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace PSOK.Kademlia.Services
{
    /// <summary>
    /// Custom message inspector used to attach the custom header to WCF messages
    /// </summary>
    internal class MessageInspector : IClientMessageInspector, IDispatchMessageInspector
    {
        private readonly INode _owner;

        public MessageInspector(INode kademlia)
        {
            if (kademlia == null)
                throw new ArgumentNullException("kademlia");

            _owner = kademlia;
        }

        void IClientMessageInspector.AfterReceiveReply(ref Message reply, object correlationState)
        {
        }

        /// <summary>
        /// Attach the custom header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        object IClientMessageInspector.BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
            request = buffer.CreateMessage();

            request.Headers.Add(new Header(_owner.Contact));

            return null;
        }

        /// <summary>
        /// Read the custom header
        /// </summary>
        /// <param name="request"></param>
        /// <param name="channel"></param>
        /// <param name="instanceContext"></param>
        /// <returns></returns>
        object IDispatchMessageInspector.AfterReceiveRequest(ref Message request, IClientChannel channel,
            InstanceContext instanceContext)
        {
            MessageBuffer buffer = request.CreateBufferedCopy(Int32.MaxValue);
            request = buffer.CreateMessage();

            IContact contact = Header.ReadHeader(request);

            if (contact != null)
                _owner.Inform(contact);

            return null;
        }

        void IDispatchMessageInspector.BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}