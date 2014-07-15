namespace PSOK.Kernel
{
    /// <summary>
    /// Application status.
    /// </summary>
    public enum AppStatus
    {
        /// <summary>
        /// Indicates a running application.
        /// </summary>
        Running = 0,

        /// <summary>
        /// Indicates a running application with some faults.
        /// </summary>
        PartiallyRunning = 100,

        /// <summary>
        /// Indicates an application which is down.
        /// </summary>
        Down = 200
    }
}