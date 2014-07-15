using System;
using System.ServiceModel;
using PSOK.Kernel.Services;

namespace PSOK.Kademlia.Services
{
    /// <summary>
    /// This class represents a communication proxy around an <see cref="Node" />
    /// </summary>
    internal class NodeProxy : ServiceProxy<IKademlia>
    {
        // private static readonly ILog Log = LogManager.GetLogger(typeof (NodeProxy));
        private readonly IContact _contact;
        private readonly INode _owner;
        private readonly bool _inform;

        public NodeProxy(INode owner, IContact contact, bool inform)
            : base(contact.NodeUrl)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            if (contact == null)
                throw new ArgumentNullException("contact");

            if (contact.Equals(owner.Contact))
                throw new ArgumentException("NodeProxy cannot be set to it's owner.");
            
            _owner = owner;
            _contact = contact;
            _inform = inform;
        }

        protected override void Initialize(Action<ChannelFactory<IKademlia>> modifyChannelFactory = null, Action<System.ServiceModel.Channels.Binding> modifyBinding = null)
        {
            base.Initialize(x =>
            {
                if (modifyChannelFactory != null)
                    modifyChannelFactory(x);
                x.Endpoint.Behaviors.Add(new Behavior(_owner));
            }, x =>
            {
                if (modifyBinding != null)
                    modifyBinding(x);
                Binding.SetTimeout(x);
            });
        }

        /// <summary>
        /// Dynamically gets a channel factory based on the client
        /// </summary>
        /// <returns></returns>
        public new IKademlia Context
        {
            get
            {
                if(_inform)
                    _owner.Inform(_contact);
                return base.Context;
            }
        }
    }
}