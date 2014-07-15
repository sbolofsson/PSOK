using System;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Defines the basic operations for a service host.
    /// </summary>
    public interface IServiceHost : IDisposable
    {
        /// <summary>
        /// Force initializes the <see cref="IServiceHost" />.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Indicates whether the <see cref="IServiceHost" /> is initialized.
        /// </summary>
        bool IsInitialized { get; }
    }
}