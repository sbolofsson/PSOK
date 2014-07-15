using System;
using System.ServiceProcess;
using PSOK.PublishSubscribe;
using PSOK.PublishSubscribe.Services;
using log4net;

namespace PSOK.Kademlia.Bootstrapper
{
    /// <summary>
    /// The service hosting the <see cref="Bootstrapper"/>
    /// </summary>
    public partial class BootstrapperService : ServiceBase
    {
        private BootstrapperNode _bootstrapper;
        private IDebugServiceHost _debugServiceHost;
        private static readonly ILog Log = LogManager.GetLogger(typeof (BootstrapperService));

        /// <summary>
        /// Constructs a new <see cref="BootstrapperService"/>.
        /// </summary>
        public BootstrapperService()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event raised when the service is started.
        /// </summary>
        /// <param name="args"></param>
        protected override void OnStart(string[] args)
        {
            try
            {
                _bootstrapper = new BootstrapperNode();
                _debugServiceHost = new DebugServiceHost(_bootstrapper.Node.Contact.DebugUrl());
                _debugServiceHost.Initialize();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Event raised when the service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                if (_bootstrapper != null)
                    _bootstrapper.Dispose();

                if (_debugServiceHost != null)
                    _debugServiceHost.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}