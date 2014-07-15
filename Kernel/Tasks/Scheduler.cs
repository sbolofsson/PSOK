using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kernel.Configuration;
using log4net;
using Thread = PSOK.Kernel.Threads.Thread;

namespace PSOK.Kernel.Tasks
{
    /// <summary>
    /// The scheduler responsible for invoking scheduled tasks (<see cref="Agent" />s).
    /// </summary>
    internal class Scheduler : Thread
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof(Scheduler));

        // Instance fields
        private Timer _timer;
        private readonly SemaphoreSlim _schedulerLock = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Constructs a new <see cref="Scheduler" />.
        /// </summary>
        public Scheduler()
        {
            OnDisposing += () =>
            {
                WaitHandle waitHandle = new AutoResetEvent(false);
                _timer.Dispose(waitHandle);
                WaitHandle.WaitAll(new[] { waitHandle });
            };
        }

        /// <summary>
        /// Runs the thread.
        /// </summary>
        protected override void Run()
        {
            Config config = Config.ReadConfig();
            TimeSpan frequency = config.Scheduling.Frequency.Value;
            _timer = new Timer(Run, null, new TimeSpan(0), frequency);
            IEnumerable<Agent> agents = config.Scheduling.Select(x => Agent.GetAgent(x.Type, x.Method)).ToList();
            foreach (Agent agent in agents)
            {
                agent.Start();
            }
        }

        /// <summary>
        /// Executes all due <see cref="Agent" />s.
        /// </summary>
        /// <param name="state"></param>
        private void Run(object state)
        {
            bool entered = false;

            try
            {
                if (!_schedulerLock.Wait(new TimeSpan(0)))
                    return;

                entered = true;

                Config config = Config.ReadConfig();
                IEnumerable<Agent> agents =
                    config.Scheduling.Select(x => Agent.GetAgent(x.Type, x.Method)).Where(x => x.IsDue).ToList();

                foreach (Agent agent in agents)
                {
                    agent.Schedule();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
            finally
            {
                if (entered)
                    _schedulerLock.Release();
            }
        }
    }
}