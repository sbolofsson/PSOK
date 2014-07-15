using System;
using System.Collections.Generic;
using System.Linq;
using PSOK.Kernel.Web;
using log4net;

namespace PSOK.PublishSubscribe.Tasks
{
    /// <summary>
    /// Ensures that all necessary objects in the application are initialized.
    /// </summary>
    internal class KeepAlive
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KeepAlive));
        private Ping _ping;

        public void Run()
        {
            InitializePeers();
            KeepIisAlive();
        }

        /// <summary>
        /// Keeps all <see cref="IPeer"/>s alive.
        /// </summary>
        private void InitializePeers()
        {
            IEnumerable<IPeer> peers = Peer.Peers.Where(x => !x.IsInitialized).ToList();
            foreach (IPeer peer in peers)
            {
                try
                {
                    peer.Initialize();
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }

        /// <summary>
        /// Keeps the IIS application pool alive.
        /// </summary>
        private void KeepIisAlive()
        {
            try
            {
                if (!IisHelper.IsHostedByIis)
                    return;

                if (_ping != null && _ping.Execute())
                    return;

                IEnumerable<Uri> uris = IisHelper.GetUris();
                foreach (Uri uri in uris)
                {
                    Ping ping = new Ping(uri);
                    if (ping.Execute())
                    {
                        _ping = ping;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}