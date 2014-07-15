using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using PSOK.Kernel.Security;

namespace PSOK.Kernel.Reflection
{
    /// <summary>
    /// Extensions for the <see cref="Type"/> class.
    /// </summary>
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<Type, Lazy<string>> AssemblyQualifiedNames =
            new ConcurrentDictionary<Type, Lazy<string>>();

        // private static readonly ILog Log = LogManager.GetLogger(typeof(TypeExtensions));

        /// <summary>
        /// Indiciates the assembly qualified name without the public key token and without the assembly version.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string AssemblyQualifiedName(this Type type)
        {
            string assemblyQualifiedName = type.AssemblyQualifiedName;

            if (assemblyQualifiedName == null)
                return assemblyQualifiedName;

            return AssemblyQualifiedNames.GetOrAdd(type,
                    x => new Lazy<string>(() => GetAssemblyQualifiedName(x), LazyThreadSafetyMode.ExecutionAndPublication))
                    .Value;
        }

        /// <summary>
        /// Computes the unique key of the <see cref="Type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string Key(this Type type)
        {
            return EncryptionProvider.Hash(type.AssemblyQualifiedName());
        }

        private static string GetAssemblyQualifiedName(Type type)
        {
            // Try Type, Assembly format
            string typeName = string.Format("{0}, {1}", type.FullName, type.Assembly.GetName().Name);

            if (!type.IsGenericType || type.IsGenericTypeDefinition)
                return typeName;

            // Handle generic types
            IEnumerable<Type> typeArguments = type.GetGenericArguments();
            foreach (Type typeArgument in typeArguments)
            {
                string assemblyQualifiedName = typeArgument.AssemblyQualifiedName;
                if (assemblyQualifiedName != null)
                    typeName = typeName.Replace(assemblyQualifiedName, typeArgument.AssemblyQualifiedName());
            }

            return typeName;
        }

        /// <summary>
        /// Indicates if the specified type is generic and implements <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsEnumerable(this Type type)
        {
            if(type == null)
                throw new ArgumentNullException("type");

            return 
                type.IsGenericType &&
                (type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
                type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>)));
        }
    }
}
