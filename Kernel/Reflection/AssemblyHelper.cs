using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Compilation;
using PSOK.Kernel.Comparers;

namespace PSOK.Kernel.Reflection
{
    /// <summary>
    /// Helper class for working with assemblies
    /// </summary>
    public static class AssemblyHelper
    {
        private static Assembly _entryAssembly;
        private const string Unknown = "[UNKNOWN]";

        /// <summary>
        /// Indicates the unique id of the entry <see cref="Assembly" />.
        /// </summary>
        /// <returns></returns>
        public static string GetEntryAssemblyId()
        {
            Assembly assembly = GetEntryAssembly();

            object[] attributes = assembly.GetCustomAttributes(typeof (GuidAttribute), true);

            if (!attributes.Any())
                return Unknown;

            GuidAttribute attribute = attributes.First() as GuidAttribute;
            return attribute != null ? new Guid(attribute.Value).ToString() : Unknown;
        }

        /// <summary>
        /// Indicates the file name of the entry <see cref="Assembly" />.
        /// </summary>
        /// <returns></returns>
        public static string GetEntryAssemblyName()
        {
            FileInfo fileInfo = GetEntryAssemblyFileInfo();
            return string.Format("{0}{1}", Path.GetFileNameWithoutExtension(fileInfo.Name), fileInfo.Extension.ToLower());
        }

        /// <summary>
        /// Indicates file information about the entry <see cref="Assembly" />.
        /// </summary>
        /// <returns></returns>
        public static FileInfo GetEntryAssemblyFileInfo()
        {
            return new FileInfo(new Uri(GetEntryAssembly().GetName().CodeBase).LocalPath);
        }

        /// <summary>
        /// Indicates the entry <see cref="Assembly" />.
        /// </summary>
        /// <returns></returns>
        public static Assembly GetEntryAssembly()
        {
            if (_entryAssembly != null)
                return _entryAssembly;

            Assembly entryAssembly = Assembly.GetEntryAssembly();

            if (entryAssembly != null)
                return (_entryAssembly = entryAssembly);

            Type globalAsaxType = null;
            try
            {
                globalAsaxType = BuildManager.GetGlobalAsaxType();
            }
            catch (Exception) { { } }

            Type globalAsaxBaseType = typeof (HttpApplication);

            if (globalAsaxType != null && globalAsaxBaseType.IsAssignableFrom(globalAsaxType))
            {
                while (globalAsaxType != null && globalAsaxType.BaseType != globalAsaxBaseType)
                {
                    globalAsaxType = globalAsaxType.BaseType;
                }

                if (globalAsaxType != null)
                    return (_entryAssembly = globalAsaxType.Assembly);
            }

            return (_entryAssembly = Assembly.LoadFile(Process.GetCurrentProcess().MainModule.FileName));
        }

        /// <summary>
        /// Indicates whether an assembly name is a system assembly name.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private static bool IsSystemAssemblyName(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
                throw new ArgumentNullException("assemblyName");

            return assemblyName.StartsWith("System") || assemblyName.StartsWith("Microsoft") ||
                   assemblyName.StartsWith("mscorlib") || assemblyName.StartsWith("vshost");
        }

        /// <summary>
        /// Indicates whether an <see cref="AssemblyName" /> is a system assembly name.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static bool IsSystemAssembly(this AssemblyName assemblyName)
        {
            if (assemblyName == null)
                throw new ArgumentNullException("assemblyName");

            return IsSystemAssemblyName(assemblyName.FullName) ||
                   IsSystemAssembly(AppDomain.CurrentDomain.Load(assemblyName));
        }

        /// <summary>
        /// Indicates whether an <see cref="Assembly" /> is a system assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static bool IsSystemAssembly(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            // Check assembly name
            if (IsSystemAssemblyName(assembly.FullName))
                return true;

            // If the name does not show it is a system assembly, then check the company assembly attribute
            AssemblyCompanyAttribute assemblyCompanyAttribute =
                Attribute.GetCustomAttribute(assembly, typeof (AssemblyCompanyAttribute), false) as
                    AssemblyCompanyAttribute;

            if (assemblyCompanyAttribute == null)
                return false;

            string company = assemblyCompanyAttribute.Company;

            return !string.IsNullOrEmpty(company) &&
                   (company.Contains("Microsoft") || string.Equals(company, "The Apache Software Foundation"));
        }

        /// <summary>
        /// Gets all referenced assemblies recursively.
        /// </summary>
        /// <param name="knownAssemblies">All assemblies known so far.</param>
        /// <param name="assemblies">The assemblies whose references have not yet been determined.</param>
        /// <param name="includeSystemAssemblies">Indicates whether system assemblies should be included.</param>
        private static void GetReferencedAssemblies(Dictionary<string, Assembly> knownAssemblies,
            IEnumerable<Assembly> assemblies, bool includeSystemAssemblies)
        {
            if (knownAssemblies == null)
                throw new ArgumentNullException("knownAssemblies");

            if (assemblies == null)
                throw new ArgumentNullException("assemblies");

            List<AssemblyName> referencedAssemblyNames = assemblies
                .SelectMany(x => x.GetReferencedAssemblies())
                .Distinct(new AssemblyNameEqualityComparer())
                .Where(
                    x => !knownAssemblies.ContainsKey(x.FullName) && (includeSystemAssemblies || !x.IsSystemAssembly()))
                .ToList();

            if (!referencedAssemblyNames.Any())
                return;

            List<Assembly> referencedAssemblies = new List<Assembly>();
            foreach (AssemblyName assemblyName in referencedAssemblyNames)
            {
                Assembly assembly = AppDomain.CurrentDomain.Load(assemblyName);
                knownAssemblies.Add(assemblyName.FullName, assembly);
                referencedAssemblies.Add(assembly);
            }

            GetReferencedAssemblies(knownAssemblies, referencedAssemblies, includeSystemAssemblies);
        }

        /// <summary>
        /// Gets all assemblies of the application
        /// </summary>
        /// <param name="includeSystemAssemblies">Indicates whether system assemblies should be included</param>
        /// <returns></returns>
        public static IEnumerable<Assembly> GetAllAssemblies(bool includeSystemAssemblies = false)
        {
            IEnumerable<Assembly> referencedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => includeSystemAssemblies || !x.IsSystemAssembly())
                .Distinct(new AssemblyEqualityComparer()).ToList();

            Dictionary<string, Assembly> knownAssemblies = referencedAssemblies.ToDictionary(x => x.FullName);
            GetReferencedAssemblies(knownAssemblies, referencedAssemblies, includeSystemAssemblies);
            return knownAssemblies.Values.ToList();
        }

        /// <summary>
        /// Retrieves an <see cref="Assembly"/> based on the specified name.
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        public static Assembly GetAssembly(string assemblyName)
        {
            if(string.IsNullOrEmpty(assemblyName))
                throw new ArgumentNullException("assemblyName");

            Assembly assembly = null;

            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch (Exception)
            {
                { }
            }

            if (assembly != null)
                return assembly;

            return GetAllAssemblies(IsSystemAssemblyName(assemblyName))
                .FirstOrDefault(x => string.Equals(x.GetName().Name, assemblyName));
        }
    }
}