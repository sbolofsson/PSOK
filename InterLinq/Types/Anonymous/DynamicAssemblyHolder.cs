using System;
using System.Reflection;
using System.Reflection.Emit;

namespace InterLinq.Types.Anonymous
{
    /// <summary>
    /// A class that holds a <see cref="AssemblyBuilder">dynamic assembly</see>.
    /// </summary>
    internal class DynamicAssemblyHolder
    {

        #region Singleton (double locked)

        private static DynamicAssemblyHolder _instance;

        /// <summary>
        /// Singleton instance of the <see cref="DynamicAssemblyHolder"/>.
        /// </summary>
        public static DynamicAssemblyHolder Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (typeof(DynamicAssemblyHolder))
                    {
                        if (_instance == null)
                        {
                            _instance = new DynamicAssemblyHolder();
                            _instance.Initialize();
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Fields

        private AssemblyBuilder _assembly;

        #endregion

        #region Properties

        /// <summary>
        /// Gets a <see cref="ModuleBuilder"/> to create types in it. 
        /// </summary>
        public ModuleBuilder ModuleBuilder { get; private set; }

        #endregion

        #region Constuctors / Initialization

        /// <summary>
        /// Private constructor to avoid external instantiation.
        /// </summary>
        private DynamicAssemblyHolder() { }
        /// <summary>
        /// Initializes the <see cref="DynamicAssemblyHolder"/>.
        /// </summary>
        private void Initialize()
        {
            // get the current appdomain
            AppDomain ad = AppDomain.CurrentDomain;

            // create a new dynamic assembly
            AssemblyName an = new AssemblyName
                                  {
                                      Name = "InterLinq.Types.Anonymous.Assembly",
                                      Version = new Version("1.0.0.0")
                                  };

            _assembly = ad.DefineDynamicAssembly(
             an, AssemblyBuilderAccess.Run);

            // create a new module to hold code in the assembly
            ModuleBuilder = _assembly.GetDynamicModule("InterLinq.Types.Anonymous.Module") ??
                            _assembly.DefineDynamicModule("InterLinq.Types.Anonymous.Module");
        }

        #endregion

    }
}
