using System;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Messages;
using log4net;

namespace PSOK.PublishSubscribe.Services
{
    /// <summary>
    /// Servicehost for communication between <see cref="IPeer" />s.
    /// </summary>
    internal class PeerServiceHost : ServiceHostBase, IPeerServiceHost
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (PeerServiceHost));
        private readonly IPeer _peer;

        /// <summary>
        /// Constructs a new <see cref="PeerServiceHost" />.
        /// </summary>
        /// <param name="peer"></param>
        public PeerServiceHost(IPeer peer)
        {
            if (peer == null)
                throw new ArgumentNullException("peer");

            _peer = peer;
        }

        #region IServiceHost methods

        /// <summary>
        /// Initializes the <see cref="PeerServiceHost" />.
        /// </summary>
        public override void Initialize()
        {
            Initialize("peer", _peer.Node.Contact.PeerUrl(), typeof (IPeerServiceHost));
        }

        #endregion

        #region IPeerServiceHost methods

        /// <summary>
        /// Is invoked when a relevant <see cref="Message" /> is published.
        /// </summary>
        /// <param name="publish"></param>
        public void Callback(IPublish<Message> publish)
        {
            if (publish == null)
                throw new ArgumentNullException("publish");

            try
            {
                _peer.Callback(publish);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Checks if the <see cref="IPeerServiceHost" /> is alive.
        /// </summary>
        /// <returns></returns>
        public bool Ping()
        {
            return true;
        }

        #endregion
    }
}