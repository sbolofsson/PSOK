using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PSOK.Kernel.Exceptions;
using PSOK.Kernel.Reflection;
using log4net;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Resolves known types at runtime
    /// </summary>
    public static class KnownTypeResolver
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(KnownTypeResolver));

        private static readonly ConcurrentDictionary<string, Type> KnownTypes =
            new ConcurrentDictionary<string, Type>();

        /// <summary>
        /// Finds a known type
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type ResolveType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            try
            {
                // Check the dictionary
                Type type;

                if (KnownTypes.TryGetValue(typeName, out type))
                    return type;

                type = TypeHelper.GetType(typeName);

                if (type == null)
                    throw new ServiceException(string.Format("Could not resolve type '{0}'.", typeName));
                
                KnownTypes[typeName] = type;

                return type;
            }
            catch (System.Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Determines whether the given type is known.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsKnownType(Type type)
        {
            return ResolveType(type.AssemblyQualifiedName()) != null;
        }

        /// <summary>
        /// Gets the known types for WCF services.
        /// Is called by the framework.
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            // This method is not allowed to return generic types!
            return GetKnownTypes().Where(t => !t.IsGenericType);
        }

        /// <summary>
        /// Gets the known types for WCF services.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Type> GetKnownTypes()
        {
            return KnownTypes.Values.ToList();
        }
    }
}