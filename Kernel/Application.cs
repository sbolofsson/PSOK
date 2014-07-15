using System;
using System.Text;
using System.Threading;
using System.Web;
using PSOK.Kernel.Pipelines;
using PSOK.Kernel.Threads;
using PSOK.Kernel.Web;
using log4net;
using log4net.Config;

namespace PSOK.Kernel
{
    /// <summary>
    /// Class representing the scope of the application
    /// </summary>
    public static class Application
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof (Application));
        private static readonly SemaphoreSlim InitializeLock = new SemaphoreSlim(1, 1);
        private static bool _isInitialized;
        private static bool _applicationStarted;
        private static bool _applicationStarting;
        private static HttpApplication _httpApplication;

        /// <summary>
        /// Event handler which is called when the P2P application infrastructure is exiting.
        /// </summary>
        public static event Action OnExit;

        /// <summary>
        /// Event handler which is called when an unhandled Exception occurs.
        /// </summary>
        public static event Action<Exception> OnUnhandledError;

        /// <summary>
        /// Starts the P2P application infrastructure asynchronously.
        /// </summary>
        public static void Start()
        {
            Start(null);
        }

        /// <summary>
        /// Starts the P2P application infrastructure asynchronously with the specified <see cref="HttpApplication" />.
        /// </summary>
        public static void Start(HttpApplication httpApplication)
        {
            if (_isInitialized)
                return;

            try
            {
                InitializeLock.Wait();

                if (_isInitialized)
                    return;

                XmlConfigurator.Configure();

                AppDomain.CurrentDomain.UnhandledException += ApplicationError;
                AppDomain.CurrentDomain.ProcessExit += ApplicationExit;
                _httpApplication = httpApplication;

                if (httpApplication != null)
                {
                    httpApplication.Error += ApplicationError;
                    httpApplication.Disposed += ApplicationExit;
                }

                InitializerThread initializerThread = new InitializerThread();
                initializerThread.Start();

                _isInitialized = true;
            }
            finally
            {
                InitializeLock.Release();
            }
        }

        /// <summary>
        /// Is called when the P2P application infrastructure has finished starting.
        /// </summary>
        internal static void ApplicationStarted()
        {
            if (_applicationStarted)
                return;

            try
            {
                InitializeLock.Wait();

                if (_applicationStarted)
                    return;

                Pipeline pipeline = Pipeline.GetPipeline("initialized");
                pipeline.Invoke(new InitializedArgs());

                Log.Info("Application has been started.");

                _applicationStarted = true;
            }
            finally
            {
                InitializeLock.Release();
            }
        }

        /// <summary>
        /// Is called before the P2P application infrastructure has started.
        /// </summary>
        internal static void ApplicationStarting()
        {
            if (_applicationStarting)
                return;

            try
            {
                InitializeLock.Wait();

                if (_applicationStarting)
                    return;

                Pipeline pipeline = Pipeline.GetPipeline("initializing");
                pipeline.Invoke(new InitializingArgs());

                _applicationStarting = true;
            }
            finally
            {
                InitializeLock.Release();
            }
        }

        /// <summary>
        /// Event handler which is called when the application is shutting down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ApplicationExit(object sender, EventArgs e)
        {
            XmlConfigurator.Configure();

            StringBuilder stringBuilder = new StringBuilder("Application is shutting down.");

            ShutdownInfo shutdownInfo = ShutdownInfo.GetShutdownInfo();
            if (shutdownInfo != null && !string.IsNullOrEmpty(shutdownInfo.Message))
                stringBuilder.Append(string.Format(" Message:\n{0}", shutdownInfo.Message));

            Log.Info(stringBuilder.ToString());

            Pipeline pipeline = Pipeline.GetPipeline("shutdown");
            pipeline.Invoke(new ShutdownArgs());

            if (OnExit == null)
                return;

            try
            {
                OnExit();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Event handler which is called when an unhandled Exception occurs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void ApplicationError(object sender, EventArgs args)
        {
            Exception exception = _httpApplication.Server.GetLastError();
            ApplicationError(exception);
        }

        /// <summary>
        /// Event handler which is called when an unhandled Exception occurs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void ApplicationError(object sender, UnhandledExceptionEventArgs args)
        {
            StringBuilder stringBuilder = new StringBuilder("An unhandled exception occurred.");
            if (args.IsTerminating)
                stringBuilder.Append(" The application will terminate.");
            
            Exception exception = args.ExceptionObject as Exception;
            if (exception != null)
            {
                Log.Error(stringBuilder.ToString(), exception);
                ApplicationError(exception);
                return;
            }
            if (args.ExceptionObject != null)
            {
                stringBuilder.Append(" Message: ");
                stringBuilder.Append(args.ExceptionObject);
            }
            Log.Error(stringBuilder.ToString());
        }

        /// <summary>
        /// Event handler which is called when an unhandled Exception occurs.
        /// </summary>
        /// <param name="exception"></param>
        private static void ApplicationError(Exception exception)
        {
            if (OnUnhandledError != null && exception != null)
                OnUnhandledError(exception);
        }
    }
}