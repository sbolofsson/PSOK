using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PSOK.Kernel.Reflection;

namespace PSOK.Kernel.Comparers
{
    /// <summary>
    /// Options for how to determine method equality
    /// </summary>
    public class MethodInfoEqualityOptions
    {
        /// <summary>
        /// Specifies whether base class methods should also be considered as a matching method.
        /// </summary>
        public bool CheckBaseClass { get; set; }

        /// <summary>
        /// Specifies whether interface methods should also be considered as a matching method.
        /// </summary>
        public bool CheckInterface { get; set; }

        /// <summary>
        /// Specifies whether generic interface methods should also be considered as a matching method.
        /// </summary>
        public bool CheckGenericInterface { get; set; }
    }

    /// <summary>
    /// Comparer which checks for method equality
    /// </summary>
    public class MethodInfoEqualityComparer
    {
        /// <summary>
        /// Checks if two methods are equal.
        /// </summary>
        /// <param name="methodInfoX"></param>
        /// <param name="methodInfoY"></param>
        /// <param name="options">
        /// The options defining method equality. If none are given, normal method signature equality (as
        /// defined in the <see cref="AreFromSameClass" /> method) is used
        /// </param>
        /// <returns></returns>
        public static bool AreMethodsEqual(MethodInfo methodInfoX, MethodInfo methodInfoY,
            MethodInfoEqualityOptions options = null)
        {
            if (methodInfoX == null)
                throw new ArgumentNullException("methodInfoX");

            if (methodInfoY == null)
                throw new ArgumentNullException("methodInfoY");

            if (options == null)
                options = new MethodInfoEqualityOptions();

            return (AreFromSameClass(methodInfoX, methodInfoY)) ||
                   (options.CheckBaseClass && AreFromBaseClass(methodInfoX, methodInfoY)) ||
                   (options.CheckInterface && AreFromInterface(methodInfoX, methodInfoY)) ||
                   (options.CheckGenericInterface && AreFromGenericInterface(methodInfoX, methodInfoY));
        }

        /// <summary>
        /// Checks if one of the given methods is an implementation of the other's declaring interface type.
        /// </summary>
        /// <param name="methodInfoX"></param>
        /// <param name="methodInfoY"></param>
        /// <returns></returns>
        public static bool AreFromInterface(MethodInfo methodInfoX, MethodInfo methodInfoY)
        {
            if (methodInfoX == null)
                throw new ArgumentNullException("methodInfoX");

            if (methodInfoY == null)
                throw new ArgumentNullException("methodInfoY");

            Type typeX = methodInfoX.DeclaringType;
            Type typeY = methodInfoY.DeclaringType;

            // Check if the methods are anonymous or equal
            if (typeX == null || typeY == null || typeX == typeY)
                return false;

            // Check if none or both declaring types are interfaces
            if ((!typeX.IsInterface && !typeY.IsInterface) || (typeX.IsInterface && typeY.IsInterface))
                return false;

            // Check if the signatures match
            if (!AreSignaturesEqual(methodInfoX, methodInfoY))
                return false;

            Type interfaceType = typeX.IsInterface ? typeX : typeY;
            Type implementingType = typeX.IsInterface ? typeY : typeX;

            // Check if the implementing type implements the interface
            if (!implementingType.GetInterfaces().Contains(interfaceType))
                return false;

            InterfaceMapping interfaceMapping = implementingType.GetInterfaceMap(interfaceType);
            MethodInfo implementingMethod = typeX.IsInterface ? methodInfoY : methodInfoX;

            // Check if the given implementing method is part of the implementation of the interface
            return interfaceMapping.TargetMethods.Contains(implementingMethod);
        }

        /// <summary>
        /// Checks if one of the given methods is an implementation of the other's declaring generic interface type.
        /// Equality is defined as an exact method signature match or a sub/base class relationship between the
        /// generic type arguments as well as in the method signatures.
        /// </summary>
        /// <param name="methodInfoX"></param>
        /// <param name="methodInfoY"></param>
        /// <returns></returns>
        public static bool AreFromGenericInterface(MethodInfo methodInfoX, MethodInfo methodInfoY)
        {
            if (methodInfoX == null)
                throw new ArgumentNullException("methodInfoX");

            if (methodInfoY == null)
                throw new ArgumentNullException("methodInfoY");

            // Check if the method names match
            if (!AreNamesEqual(methodInfoX, methodInfoY))
                return false;

            Type typeX = methodInfoX.DeclaringType;
            Type typeY = methodInfoY.DeclaringType;

            // Check if the methods are anonymous or equal
            if (typeX == null || typeY == null || typeX == typeY)
                return false;

            // Check if none or both declaring types are generic interfaces
            if ((!(typeX.IsInterface && typeX.IsGenericType) && !(typeY.IsInterface && typeY.IsGenericType))
                || (typeX.IsInterface && typeX.IsGenericType && typeY.IsInterface && typeY.IsGenericType))
                return false;

            // Determine the implementing type and method as well as the interface type and method
            Type interfaceType = typeX.IsInterface ? typeX : typeY;
            Type implementingType = typeX.IsInterface ? typeY : typeX;
            MethodInfo interfaceMethod = typeX.IsInterface ? methodInfoX : methodInfoY;
            MethodInfo implementingMethod = typeX.IsInterface ? methodInfoY : methodInfoX;

            // Check if the given implementing method is part of any implementations of the generic interface
            Type interfaceImplementing = implementingType.GetInterfaces()
                .FirstOrDefault(
                    x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType.GetGenericTypeDefinition() &&
                         implementingType.GetInterfaceMap(x).TargetMethods.Contains(implementingMethod));

            if (interfaceImplementing == null)
                return false;

            // Check if the parameters match
            ParameterInfo[] paramsInterfaceMethod = interfaceMethod.GetParameters();
            ParameterInfo[] paramsImplementingMethod = implementingMethod.GetParameters();

            if (paramsInterfaceMethod.Length != paramsImplementingMethod.Length)
                return false;

            // ReSharper disable LoopCanBeConvertedToQuery
            bool parametersEqual = true;
            for (int i = 0; i < paramsInterfaceMethod.Length; i++)
            {
                if (paramsInterfaceMethod[i].ParameterType != paramsImplementingMethod[i].ParameterType)
                {
                    parametersEqual = false;
                    break;
                }
            }
            // ReSharper restore LoopCanBeConvertedToQuery
            if (parametersEqual)
                return true;

            // Check if any other implementation of the interface methods have a signature match
            // If so, the given method cannot be the best match
            if (implementingType.GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == interfaceType.GetGenericTypeDefinition())
                .Any(x => implementingType.GetInterfaceMap(x).TargetMethods.Any(y =>
                    y != implementingMethod && AreSignaturesEqual(y, interfaceMethod))))
                return false;

            // Now we know:
            // 1. The given method is part of the implementation of the generic interface
            // 2. None of the other methods part of the implementation have a perfect match on name and parameters
            // 3. The length of the parameters are the same
            // 4. The name of the methods are the same
            // Thus, the interface method must be a method that is dependent on the generic type parameters
            // Therefore, we check if the interface method and the given implementation method have a subclass relationship
            Type[] genericArgumentsInterface = interfaceType.GetGenericArguments();
            Type[] genericArgumentsImplementing = interfaceImplementing.GetGenericArguments();
            for (int i = 0; i < paramsInterfaceMethod.Length; i++)
            {
                Type paramInterface = paramsInterfaceMethod[i].ParameterType;
                Type paramImplementing = paramsImplementingMethod[i].ParameterType;

                // If the parameters are equal, we are not dealing with a type that is dependent on a generic type argument
                if (paramInterface == paramImplementing)
                    continue;

                // Check if the respective types are contained in the generic type arguments on the same indices
                // and if there is a subclass relationship between the types
                IEnumerable<int> indicesInterface =
                    Enumerable.Range(0, genericArgumentsInterface.Length)
                        .Where(x => genericArgumentsInterface[x] == paramInterface).ToList();

                IEnumerable<int> indicesImplementing =
                    Enumerable.Range(0, genericArgumentsImplementing.Length)
                        .Where(x => genericArgumentsImplementing[x] == paramImplementing).ToList();

                HashSet<int> indicesSetInterface = new HashSet<int>(indicesInterface);
                bool indicesAreEqual = indicesInterface.Count() == indicesImplementing.Count() &&
                                       indicesImplementing.All(indicesSetInterface.Contains);

                if (indicesAreEqual && TypeHelper.IsSubclassOf(paramImplementing, paramInterface))
                    continue;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if one of the given methods are from the other's base class and if the method signatures are equal.
        /// </summary>
        /// <param name="methodInfoX"></param>
        /// <param name="methodInfoY"></param>
        /// <returns></returns>
        public static bool AreFromBaseClass(MethodInfo methodInfoX, MethodInfo methodInfoY)
        {
            if (methodInfoX == null)
                throw new ArgumentNullException("methodInfoX");

            if (methodInfoY == null)
                throw new ArgumentNullException("methodInfoY");

            bool isMethodInfoXBaseClass = TypeHelper.IsSubclassOf(methodInfoY.DeclaringType, methodInfoX.DeclaringType);
            bool isMethodInfoYBaseClass = !isMethodInfoXBaseClass &&
                                          TypeHelper.IsSubclassOf(methodInfoX.DeclaringType, methodInfoY.DeclaringType);

            return (isMethodInfoXBaseClass || isMethodInfoYBaseClass) && AreSignaturesEqual(methodInfoX, methodInfoY);
        }

        /// <summary>
        /// Checks if the given methods are from the same declaring class and have the same signatures.
        /// </summary>
        /// <param name="methodInfoX"></param>
        /// <param name="methodInfoY"></param>
        /// <returns></returns>
        public static bool AreFromSameClass(MethodInfo methodInfoX, MethodInfo methodInfoY)
        {
            if (methodInfoX == null)
                throw new ArgumentNullException("methodInfoX");

            if (methodInfoY == null)
                throw new ArgumentNullException("methodInfoY");

            return methodInfoX.DeclaringType == methodInfoY.DeclaringType &&
                   AreSignaturesEqual(methodInfoX, methodInfoY);
        }

        /// <summary>
        /// Checks if the method parameters of the given method are of same length, position, and type
        /// </summary>
        /// <param name="methodInfoX"></param>
        /// <param name="methodInfoY"></param>
        /// <returns></returns>
        public static bool AreParametersEqual(MethodInfo methodInfoX, MethodInfo methodInfoY)
        {
            if (methodInfoX == null)
                throw new ArgumentNullException("methodInfoX");

            if (methodInfoY == null)
                throw new ArgumentNullException("methodInfoY");

            // Are the parameters the same
            ParameterInfo[] parametersX = methodInfoX.GetParameters();
            ParameterInfo[] parametersY = methodInfoY.GetParameters();

            if (parametersX.Length != parametersY.Length)
                return false;

            // ReSharper disable LoopCanBeConvertedToQuery
            bool parametersEqual = true;
            for (int i = 0; i < parametersX.Length; i++)
            {
                if (parametersX[i].ParameterType != parametersY[i].ParameterType)
                {
                    parametersEqual = false;
                    break;
                }
            }
            // ReSharper restore LoopCanBeConvertedToQuery
            return parametersEqual;
        }

        /// <summary>
        /// Checks if the the names of the given methods are equal.
        /// </summary>
        /// <param name="methodInfoX"></param>
        /// <param name="methodInfoY"></param>
        /// <returns></returns>
        public static bool AreNamesEqual(MethodInfo methodInfoX, MethodInfo methodInfoY)
        {
            if (methodInfoX == null)
                throw new ArgumentNullException("methodInfoX");

            if (methodInfoY == null)
                throw new ArgumentNullException("methodInfoY");

            // Methods have to have the same name
            return string.Equals(methodInfoX.Name, methodInfoY.Name);
        }

        /// <summary>
        /// Check if the signatures (name, parameters, and generic arity) of the given methods are equal.
        /// </summary>
        /// <param name="methodInfoX"></param>
        /// <param name="methodInfoY"></param>
        /// <returns></returns>
        public static bool AreSignaturesEqual(MethodInfo methodInfoX, MethodInfo methodInfoY)
        {
            if (methodInfoX == null)
                throw new ArgumentNullException("methodInfoX");

            if (methodInfoY == null)
                throw new ArgumentNullException("methodInfoY");

            return AreNamesEqual(methodInfoX, methodInfoY) && AreParametersEqual(methodInfoX, methodInfoY) &&
                   methodInfoX.IsGenericMethod == methodInfoY.IsGenericMethod &&
                   methodInfoX.IsPrivate == methodInfoY.IsPrivate &&
                   methodInfoX.IsPublic == methodInfoY.IsPublic && methodInfoX.IsStatic == methodInfoY.IsStatic &&
                   // This breaks abstract class and interface method comparisons
                   /* methodInfoX.IsVirtual == methodInfoY.IsVirtual && methodInfoX.IsAbstract == methodInfoY.IsAbstract && */
                   methodInfoX.IsConstructor == methodInfoY.IsConstructor &&
                   methodInfoX.IsGenericMethodDefinition == methodInfoY.IsGenericMethodDefinition;
        }
    }
}