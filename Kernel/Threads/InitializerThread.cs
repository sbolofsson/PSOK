using System;
using System.Globalization;
using PSOK.Kernel.Configuration;
using PSOK.Kernel.Configuration.Pipelines;
using PSOK.Kernel.Events;
using PSOK.Kernel.Tasks;
using log4net;
using log4net.Config;

namespace PSOK.Kernel.Threads
{
    /// <summary>
    /// Initializer thread for starting the entire application.
    /// </summary>
    internal class InitializerThread : Thread
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof (InitializerThread));

        /// <summary>
        /// Initializes the application.
        /// </summary>
        protected override void Run()
        {
            XmlConfigurator.Configure();
            Application.ApplicationStarting();
            InitializeCulture();
            InitializeEventQueues();
            InitializeScheduler();
            Application.ApplicationStarted();
        }

        /// <summary>
        /// Sets the default culture for the application.
        /// </summary>
        private static void InitializeCulture()
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        /// <summary>
        /// Initializes all <see cref="IEventQueue" />s.
        /// </summary>
        private static void InitializeEventQueues()
        {
            try
            {
                Config config = Config.ReadConfig();
                foreach (Pipeline pipeline in config.Pipelines)
                {
                    IEventQueue eventQueue = EventQueue.GetQueue(pipeline.Name);
                    InitializeWorkerThreads(eventQueue, pipeline.WorkerThreads
                        .GetValueOrDefault(config.ProcessModel.WorkerThreads));
                }
                Log.Info("Event queues and worker threads have been initialized.");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Initializes all <see cref="WorkerThread" />s.
        /// </summary>
        /// <param name="eventQueue"></param>
        /// <param name="threads"></param>
        private static void InitializeWorkerThreads(IEventQueue eventQueue, int threads)
        {
            for (int i = 0; i < threads; i++)
            {
                WorkerThread workerThread = new WorkerThread(eventQueue);
                workerThread.Start();
            }
        }

        /// <summary>
        /// Initializes the <see cref="Scheduler" />.
        /// </summary>
        private static void InitializeScheduler()
        {
            try
            {
                Scheduler scheduler = new Scheduler();
                scheduler.Start();
                Log.Info("Scheduler has been initialized.");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}