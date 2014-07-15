using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PSOK.Kernel.Reflection
{
    /// <summary>
    /// A helper class for working with object instances.
    /// </summary>
    public static class ObjectHelper
    {
        /// <summary>
        /// Traverses an object graph.
        /// </summary>
        /// <typeparam name="TBase">The base class type to use when determining if an object property should be traversed</typeparam>
        /// <typeparam name="TObj">The type of the object to traverse</typeparam>
        /// <typeparam name="TResult">A list of this type is the result</typeparam>
        /// <param name="objectToTraverse">A root object to traverse</param>
        /// <param name="selector">
        /// A function which determines the values to return.
        /// The parameters to this function are: the current object being traversed,
        /// property info from the current property and the property value.
        /// </param>
        /// <returns></returns>
        public static IEnumerable<TResult> TraverseObject<TBase, TObj, TResult>(TObj objectToTraverse,
            Func<object, PropertyInfo, object, TResult> selector)
        {
            if (EqualityComparer<TObj>.Default.Equals(objectToTraverse, default(TObj)))
                throw new ArgumentNullException("objectToTraverse");

            if (selector == null)
                throw new ArgumentNullException("selector");

            List<TResult> entries = new List<TResult>();

            Action<object> traverse = null;
            traverse = obj =>
            {
                if (obj == null)
                    return;

                IEnumerable<PropertyInfo> propertyInfos = obj.GetType().GetProperties();
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    object propertyValue = propertyInfo.GetValue(obj);

                    if (propertyValue == null)
                    {
                        entries.Add(selector(obj, propertyInfo, null));
                        continue;
                    }

                    Type propertyValueType = propertyValue.GetType();
                    if (TypeHelper.IsSubclassOf(propertyValueType, typeof (TBase)))
                    {
                        traverse(propertyValue);
                    }
                    else if (propertyValueType.IsEnumerable() && propertyValue is IEnumerable)
                    {
                        foreach (object objectProperty in propertyValue as IEnumerable)
                        {
                            traverse(objectProperty);
                        }
                    }
                    else
                    {
                        entries.Add(selector(obj, propertyInfo, propertyValue));
                    }
                }
            };

            traverse(objectToTraverse);

            return entries;
        }
    }
}