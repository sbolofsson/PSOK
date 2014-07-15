using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PSOK.Kernel.Collections;
using PSOK.Kernel.Exceptions;

namespace PSOK.Kernel.Reflection
{
    /// <summary>
    /// Helper class for working with types
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// Gets all subclasses of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="includeSystemTypes"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSubclasses(Type type, bool includeSystemTypes = false)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return AssemblyHelper.GetAllAssemblies(includeSystemTypes)
                .SelectMany(x => x.GetTypes().Where(y => y.IsClass && y != type && IsSubclassOf(y, type))).ToList();
        }

        /// <summary>
        /// Creates all possible type combinations based on specified type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IEnumerable<Type> CreateTypes(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (!type.IsClass)
                throw new ArgumentException("The specified type must be a class definition.");

            if (!type.IsGenericType)
                return new List<Type> { type };

            Type genericTypeDefinition = type.IsGenericTypeDefinition ? type : type.GetGenericTypeDefinition();
            
            // Detect endless recursion
            IEnumerable<Type> genericTypeArguments = genericTypeDefinition.GetGenericArguments()
                .Select(x => x.BaseType).ToList();

            if (genericTypeArguments.Any(x => IsSubclassOf(type, x)))
                throw new ReflectionException(string.Format("Could not create all combinations of type '{0}' as the type itself can be substituted for a generic type argument, thus leading to endless recursion.", type.FullName));

            return genericTypeArguments
                .Select(x => GetSubclasses(x).Append(x).SelectMany(CreateTypes))
                .CartesianProduct()
                .Select(x => genericTypeDefinition.MakeGenericType(x.ToArray()))
                .ToList();
        }

        /// <summary>
        /// Gets all types implementing the given interface type.
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="includeSystemTypes"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetTypesByInterface(Type interfaceType, bool includeSystemTypes = false)
        {
            if (interfaceType == null)
                throw new ArgumentNullException("interfaceType");

            if (!interfaceType.IsInterface)
                throw new ReflectionException("The given type is not an interface.");

            return AssemblyHelper.GetAllAssemblies(includeSystemTypes)
                .SelectMany(
                    x => x.GetTypes().Where(y => y.IsClass && y != interfaceType && interfaceType.IsAssignableFrom(y) ||
                                                 (interfaceType.IsGenericTypeDefinition &&
                                                  y.GetInterfaces()
                                                      .Any(z => z.GetGenericTypeDefinition() == interfaceType))))
                .ToList();
        }

        /// <summary>
        /// Checks if the given type is a subclass of the other given type.
        /// The given base type may be a generic type definition,
        /// in which case the sub class will be considered a subclass if it inherits a concrete implementation.
        /// </summary>
        /// <param name="typeToCheck">The possible subclass</param>
        /// <param name="baseType">The base class</param>
        /// <returns></returns>
        public static bool IsSubclassOf(Type typeToCheck, Type baseType)
        {
            if (typeToCheck == null)
                throw new ArgumentNullException("typeToCheck");

            if (baseType == null)
                throw new ArgumentNullException("baseType");

            if (typeToCheck.IsSubclassOf(baseType))
                return true;

            if (!baseType.IsGenericTypeDefinition)
                return false;

            // Check for match on generic type definition
            while (typeToCheck != null)
            {
                Type currentType = typeToCheck.IsGenericType ? typeToCheck.GetGenericTypeDefinition() : typeToCheck;
                if (baseType == currentType)
                {
                    return true;
                }
                typeToCheck = typeToCheck.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Checks whether a given type (or a generic parent type) has a generic type argument of the other given type
        /// </summary>
        /// <param name="genericType">The type which itself or a parent is a generic type</param>
        /// <param name="baseType">The base type to check amongst the generic type arguments</param>
        /// <returns></returns>
        public static Type GetGenericArgumentOfBaseType(Type genericType, Type baseType)
        {
            if (genericType == null)
                throw new ArgumentNullException("genericType");

            if (baseType == null)
                throw new ArgumentNullException("baseType");

            Type genericTypeBase = genericType;

            while (genericTypeBase != null)
            {
                if (genericTypeBase.IsGenericType)
                {
                    Type genericTypeArgumentSubclass =
                        genericTypeBase.GenericTypeArguments.FirstOrDefault(x => IsSubclassOf(x, baseType));

                    if (genericTypeArgumentSubclass != null)
                        return genericTypeArgumentSubclass;
                }

                genericTypeBase = genericTypeBase.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Retrieves a <see cref="Type"/> based on the specified qualified assembly name.
        /// </summary>
        /// <param name="assemblyQualifiedName"></param>
        /// <returns></returns>
        public static Type GetType(string assemblyQualifiedName)
        {
            if(string.IsNullOrEmpty(assemblyQualifiedName))
                throw new ArgumentNullException("assemblyQualifiedName");

            Type type = Type.GetType(assemblyQualifiedName);

            if (type != null)
                return type;

            int index = assemblyQualifiedName.LastIndexOf(',');

            if (index < 0)
                return null;

            string typeName = assemblyQualifiedName.Substring(0, index);

            if (assemblyQualifiedName.Length < index + 1)
                return null;

            string assemblyName = assemblyQualifiedName.Substring(index + 2);
            
            Assembly assembly = AssemblyHelper.GetAssembly(assemblyName);
            return assembly != null ? assembly.GetType(typeName, false) : null;
        }
    }
}