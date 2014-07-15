using System.ComponentModel;
using System.Configuration.Install;

namespace PSOK.Kademlia.Bootstrapper
{
    /// <summary>
    /// The service installer.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        /// <summary>
        /// Construct a new <see cref="ProjectInstaller"/>.
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}