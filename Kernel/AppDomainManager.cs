using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace PSOK.Kernel
{
    /// <summary>
    /// Class responsible for starting the P2P infrastructure when the application is started.
    /// </summary>
    [Guid("1AC33DCA-50FD-46C5-9D55-2A19934AE4AE")]
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.Infrastructure)]
    public class AppDomainManager : System.AppDomainManager
    {
        /// <summary>
        /// Constructs a new <see cref="AppDomainManager"/>.
        /// </summary>
        public AppDomainManager()
        {
            Application.Start();
        }

        /// <summary>
        /// Initializes a new <see cref="AppDomain"/>.
        /// </summary>
        /// <param name="appDomainSetup"></param>
        public override void InitializeNewDomain(AppDomainSetup appDomainSetup)
        {
            InitializationFlags = AppDomainManagerInitializationOptions.RegisterWithHost;
        }
    }
}