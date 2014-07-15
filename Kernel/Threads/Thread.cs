using System;
using System.Threading;
using System.Threading.Tasks;
using PSOK.Kernel.Exceptions;
using log4net;
using Exception = System.Exception;

namespace PSOK.Kernel.Threads
{
    /// <summary>
    /// Classes can inherit this class to be invoked on a seperate thread.
    /// </summary>
    public abstract class Thread : IDisposable
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof (Thread));
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        // Instance fields
        private bool _disposed;
        private readonly SemaphoreSlim _threadLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Indicates if the <see cref="Thread"/> has been started.
        /// </summary>
        protected bool Started;

        /// <summary>
        /// Event which is raised then the <see cref="Thread"/> is disposing.
        /// </summary>
        protected event Action OnDisposing;

        /// <summary>
        /// Constructs a new <see cref="Thread" />.
        /// </summary>
        protected Thread()
        {
            Application.OnExit += Dispose;
        }

        /// <summary>
        /// Runs the <see cref="Thread" />.
        /// </summary>
        protected abstract void Run();

        /// <summary>
        /// Starts the <see cref="Thread" /> asynchronously.
        /// </summary>
        public void Start()
        {
            if (Started)
                throw new ThreadException("This thread has already been started");

            if (_disposed)
                return;

            try
            {
                _threadLock.Wait();

                if (Started)
                    throw new ThreadException("This thread has already been started");

                if (_disposed)
                    return;

                Task.Run(() =>
                {
                    try
                    {
                        Run();
                    }
                    catch (TaskCanceledException)
                    {
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }, CancellationTokenSource.Token);

                Started = true;
            }
            finally
            {
                _threadLock.Release();
            }
        }

        /// <summary>
        /// Disposes the <see cref="Thread"/>.
        /// </summary>
        public virtual void Dispose()
        {
            if (_disposed)
                return;

            try
            {
                _threadLock.Wait();

                if (_disposed)
                    return;

                if (OnDisposing != null)
                    OnDisposing();

                CancellationTokenSource.Cancel();
                
                _disposed = true;
            }
            finally
            {
                _threadLock.Release();
            }
        }
    }
}