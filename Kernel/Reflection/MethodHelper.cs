using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PSOK.Kernel.Comparers;
using PSOK.Kernel.Exceptions;

namespace PSOK.Kernel.Reflection
{
    /// <summary>
    /// A helper class for working with methods.
    /// </summary>
    public static class MethodHelper
    {
        /// <summary>
        /// Retrieves a method based on the specified method expression and target type.
        /// </summary>
        /// <typeparam name="T">The input parameter to the function. Can be an abstract class, interface or normal class</typeparam>
        /// <typeparam name="TMethod">The method which should be retrieved</typeparam>
        /// <param name="methodExpression">An expression returning the method to retrieve</param>
        /// <param name="target">
        /// The target object to get the method from (can be a subclass of T or an implementation of an
        /// interface)
        /// </param>
        /// <returns></returns>
        public static MethodInfo GetMethod<T, TMethod>(Expression<Func<T, TMethod>> methodExpression, Type target)
        {
            if (methodExpression == null)
                throw new ArgumentNullException("methodExpression");

            if (target == null)
                throw new ArgumentNullException("target");

            MethodInfo methodInfo = GetMethod(methodExpression);

            MethodInfoEqualityOptions options = new MethodInfoEqualityOptions
            {
                CheckBaseClass = true,
                CheckInterface = true,
                CheckGenericInterface = true
            };

            IEnumerable<MethodInfo> equalMethodInfos =
                target.GetMethods()
                    .Where(x => MethodInfoEqualityComparer.AreMethodsEqual(x, methodInfo, options))
                    .ToList();

            if (equalMethodInfos.Count() > 1)
                throw new ReflectionException("Found more than one matching method.");

            return equalMethodInfos.FirstOrDefault();
        }

        /// <summary>
        /// Retrieves a method based on the specified method expression.
        /// </summary>
        /// <typeparam name="T">The input parameter to the function. Can be an abstract class, interface or normal class</typeparam>
        /// <typeparam name="TMethod">The method which should be retrieved</typeparam>
        /// <param name="methodExpression">An expression returning the method to retrieve</param>
        /// <returns></returns>
        public static MethodInfo GetMethod<T, TMethod>(Expression<Func<T, TMethod>> methodExpression)
        {
            if (methodExpression == null)
                throw new ArgumentNullException("methodExpression");

            UnaryExpression unaryExpression = methodExpression.Body as UnaryExpression;
            if (unaryExpression == null)
                throw new ReflectionException("Cannot convert method body to unary expression.");

            MethodCallExpression methodCallExpression = unaryExpression.Operand as MethodCallExpression;
            if (methodCallExpression == null)
                throw new ReflectionException("Cannot convert unary expression operand to method call expression.");

            ConstantExpression constantExpression = methodCallExpression.Object as ConstantExpression;
            if (constantExpression == null)
                throw new ReflectionException(
                    "Cannot convert object property of the method call expression to a constant expression.");

            MethodInfo methodInfo = constantExpression.Value as MethodInfo;
            if (methodInfo == null)
                throw new ReflectionException("Cannot convert constant expression value to method info.");

            return methodInfo;
        }

        /// <summary>
        /// Retrieves the real method pointed to by a <see cref="Delegate"/> when the real type T is known at runtime.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="del"></param>
        /// <returns></returns>
        public static T GetUnderlyingMethod<T>(Delegate del) where T : class
        {
            return GetUnderlyingMethod(del) as T;
        }

        /// <summary>
        /// Retrieves the real method pointed to by a <see cref="Delegate"/>.
        /// </summary>
        /// <param name="del"></param>
        /// <returns></returns>
        public static Delegate GetUnderlyingMethod(Delegate del)
        {
            MethodInfo methodInfo = del.Method;
            ParameterExpression[] parameters =
                del.Method.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name))
                    .ToArray();
            // ReSharper disable CoVariantArrayConversion
            return
                Expression.Lambda(
                methodInfo.IsStatic ?
                Expression.Call(methodInfo, parameters) :
                Expression.Call(Expression.Constant(del.Target), methodInfo, parameters),
                parameters).Compile();
            // ReSharper restore CoVariantArrayConversion
        }
    }
}