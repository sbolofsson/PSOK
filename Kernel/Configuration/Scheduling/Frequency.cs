using System;

namespace PSOK.Kernel.Configuration.Scheduling
{
    /// <summary>
    /// The frequency to check for scheduled <see cref="Agent" />s.
    /// </summary>
    public class Frequency : ConfigurationElement
    {
        /// <summary>
        /// The frequency.
        /// </summary>
        public TimeSpan Value
        {
            get { return TimeSpan.Parse(CData); }
        }
    }
}