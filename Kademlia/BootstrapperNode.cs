using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using PSOK.Kernel;
using PSOK.Kernel.Configuration;
using PSOK.Kernel.Environment;
using log4net;

namespace PSOK.Kademlia
{
    /// <summary>
    /// Bootstraper for <see cref="INode" />s who want to join the P2P network.
    /// </summary>
    public class BootstrapperNode : IDisposable
    {
        // Static fields
        private static readonly SemaphoreSlim InitializeLock = new SemaphoreSlim(1, 1);
        private static readonly ILog Log = LogManager.GetLogger(typeof(BootstrapperNode));
        private static IDht _node;
        private static BootstrapperNode _bootstrapper;
        private static IEnumerable<IContact> _bootstrapperContacts; 

        /// <summary>
        /// Constructs a new <see cref="BootstrapperNode" />.
        /// </summary>
        public BootstrapperNode()
        {
            Application.Start();

            if (_node != null)
                return;

            try
            {
                InitializeLock.Wait();

                if (_node != null)
                    return;

                Node node = new Node();
                node.Initialize();
                _node = node;
                _bootstrapper = this;

                if (_node != null && _node.IsInitialized)
                    Log.Info("Initialized successfully.");
                else
                    Log.Error("Initialization failed.");
            }
            catch (Exception ex)
            {
                Log.Error("Initialization failed.");

                if (ex is AddressAlreadyInUseException)
                {
                    Log.Warn("A Kademlia.Bootstrapper is already running on this machine.");
                }
                else
                {
                    Log.Error(ex);
                    throw;
                }
            }
            finally
            {
                InitializeLock.Release();
            }
        }

        /// <summary>
        /// Disposes the <see cref="BootstrapperNode" /> and releases all resources held by the instance.
        /// </summary>
        public static void DisposeInstance()
        {
            try
            {
                InitializeLock.Wait();

                if (_bootstrapper != null)
                    _bootstrapper.Dispose();
            }
            finally
            {
                InitializeLock.Release();
            }
        }

        /// <summary>
        /// The underlying node.
        /// </summary>
        public IDht Node { get { return _node; } }

        /// <summary>
        /// Disposes the <see cref="BootstrapperNode" /> and releases all resources held by this instance.
        /// </summary>
        public void Dispose()
        {
            if(_node != null)
                _node.Dispose();
            Log.Info("Kademlia.Bootstrapper shut down.");
        }

        /// <summary>
        /// Retrieves all bootstrapper <see cref="IContact"/>
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IContact> GetContacts()
        {
            if (_bootstrapperContacts != null)
                return _bootstrapperContacts;

            Config config = Config.ReadConfig();
            string urls = config.Kademlia.Bootstrap.Urls;
            string file = config.Kademlia.Bootstrap.File;

            List<Uri> bootstrapUrls = new List<Uri>();
            if (!string.IsNullOrEmpty(urls))
            {
                bootstrapUrls.AddRange(
                    urls.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(x => new Uri(x)).ToList()
                );
            }
            if (!string.IsNullOrEmpty(file))
            {
                if (file.StartsWith("/"))
                    file = string.Format("{0}{1}", EnvironmentHelper.GetEnvironmentPath(), file);
                if (File.Exists(file))
                {
                    bootstrapUrls.AddRange(
                        File.ReadAllText(file, Encoding.UTF8)
                            .Split(new[] {"\r\n", "\n", "\r"}, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => new Uri(x)).ToList()
                        );
                }
            }

            List<IContact> bootstrappers = new List<IContact>();

            bootstrappers.AddRange(bootstrapUrls.Select(Contact.ToContact).ToList());

            _bootstrapperContacts = bootstrappers.Distinct().ToList();
            return _bootstrapperContacts;
        }
    }
}