using System;
using System.Collections.Generic;
using PSOK.Kademlia.Lookups;
using PSOK.Kernel.Services;
using log4net;

namespace PSOK.Kademlia.Services
{
    /// <summary>
    /// Defines a node service host in Kademlia.
    /// </summary>
    internal class NodeServiceHost : ServiceHostBase, IKademlia
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(NodeServiceHost));

        private readonly INode _node;

        /// <summary>
        /// Constructs a new <see cref="NodeServiceHost" />.
        /// </summary>
        /// <param name="node"></param>
        public NodeServiceHost(INode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            _node = node;
        }

        #region ServiceHostBase methods

        /// <summary>
        /// Initializes the service.
        /// </summary>
        public override void Initialize()
        {
            Initialize("node", _node.Contact.NodeUrl, typeof(IKademlia),
                x => x.Description.Behaviors.Add(new Behavior(_node)),
                Binding.SetTimeout);
        }

        #endregion

        #region IKademlia methods

        /// <summary>
        /// Pings the <see cref="NodeServiceHost" />.
        /// </summary>
        /// <returns></returns>
        public bool Ping()
        {
            return true;
        }

        /// <summary>
        /// Instructs the node to store contact information about a node having the data corresponding to the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="items"></param>
        public void Store(string key, IEnumerable<IItem> items)
        {
            try
            {
                _node.Store(key, items);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Ask a contact for a list of closest nodes (contacts) which know about the specified key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEnumerable<IContact> FindNode(string key)
        {
            try
            {
                return _node.FindNode(key);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return null;
        }

        /// <summary>
        /// Ask a contact for a value, if it is found the value is returned, otherwise it acts like <see cref="FindNode" />.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IResult FindValue(string key)
        {
            try
            {
                return _node.FindValue(key);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return null;
        }

        #endregion
    }
}