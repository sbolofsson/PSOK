using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using PSOK.Kernel.Exceptions;

namespace PSOK.Kernel.Reflection
{
    /// <summary>
    /// A helper class for working with properties.
    /// </summary>
    public static class PropertyHelper
    {
        /// <summary>
        /// Gets a property from a type.
        /// </summary>
        /// <typeparam name="T">The type to get the property from.</typeparam>
        /// <param name="propertyExpression">The expression returning the property.</param>
        /// <returns></returns>
        public static PropertyInfo GetPropertyInfo<T>(Expression<Func<T, object>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            MemberExpression memberExpression = propertyExpression.Body as MemberExpression;

            if (memberExpression == null)
                throw new ReflectionException(string.Format("Expression '{0}' does not refer to a property.",
                    propertyExpression));

            PropertyInfo propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo == null)
                throw new ReflectionException(string.Format("Expression '{0}' refers to a field, not a property.",
                    propertyExpression));

            return propertyInfo;
        }

        /// <summary>
        /// Sets a value on a property.
        /// </summary>
        /// <typeparam name="T">The type to get the property from.</typeparam>
        /// <param name="targetObject">An instance to set the property value on.</param>
        /// <param name="propertyExpression">The expression returning the property.</param>
        /// <param name="value">The default value to set.</param>
        /// <param name="covariantValue">
        /// A covariant value to set in case the type of the default value does not match
        /// the property type.
        /// </param>
        public static void SetProperty<T>(object targetObject, Expression<Func<T, object>> propertyExpression,
            object value,
            object covariantValue)
        {
            if (targetObject == null)
                throw new ArgumentNullException("targetObject");

            if (propertyExpression == null)
                throw new ArgumentNullException("propertyExpression");

            if (value == null)
                throw new ArgumentNullException("value");

            if (covariantValue == null)
                throw new ArgumentNullException("covariantValue");

            PropertyInfo propertyInfo = GetPropertyInfo(propertyExpression);

            PropertyInfo property =
                targetObject.GetType().GetProperties().FirstOrDefault(x => string.Equals(x.Name, propertyInfo.Name));

            if (property == null)
                return;

            object valueToSet = property.PropertyType.IsInstanceOfType(value) ? value : covariantValue;
            property.SetValue(targetObject, valueToSet);
        }

        /// <summary>
        /// Gets custom attributes for a property.
        /// </summary>
        /// <typeparam name="T">The custom attribute type.</typeparam>
        /// <param name="propertyInfo">The property to get the attribute from.</param>
        /// <returns></returns>
        public static T[] GetAttributes<T>(PropertyInfo propertyInfo) where T : Attribute
        {
            if (propertyInfo == null)
                throw new ArgumentNullException("propertyInfo");

            return Array.ConvertAll(propertyInfo.GetCustomAttributes(typeof (T), true), x => x as T);
        }

        /// <summary>
        /// Gets a property value.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="obj">The instance to get the property value from.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(object obj, string propertyName) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            PropertyInfo propertyInfo = obj.GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyInfo == null)
                return null;

            return propertyInfo.GetValue(obj, null) as T;
        }

        /// <summary>
        /// Gets a field value.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="obj">The instance to get the field value from.</param>
        /// <param name="propertyName">The name of the field.</param>
        /// <returns></returns>
        public static T GetFieldValue<T>(object obj, string propertyName) where T : class
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            Type type = obj.GetType();
            FieldInfo fieldInfo = null;

            while (fieldInfo == null && type != null)
            {
                fieldInfo = type.GetField(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                type = type.BaseType;
            }

            if (fieldInfo == null)
                return null;

            return fieldInfo.GetValue(obj) as T;
        }

        /// <summary>
        /// Sets a property value
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="obj">The instance to get the property value from.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="value">The value to set.</param>
        public static void SetPropertyValue<T>(object obj, string propertyName, T value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentNullException("propertyName");

            PropertyInfo propertyInfo = obj.GetType()
                .GetProperty(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (propertyInfo == null)
                throw new ReflectionException(string.Format("Property '{0}' was not found in type '{1}'.", propertyName,
                    obj.GetType().FullName));

            propertyInfo.SetValue(obj, value);
        }

        /// <summary>
        /// Sets a field value.
        /// </summary>
        /// <typeparam name="T">The type of the field.</typeparam>
        /// <param name="obj">The instance to get the field from.</param>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="value">The value to set.</param>
        public static void SetFieldValue<T>(object obj, string fieldName, T value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            if (string.IsNullOrEmpty(fieldName))
                throw new ArgumentNullException("fieldName");

            Type type = obj.GetType();
            FieldInfo fieldInfo = null;

            while (fieldInfo == null && type != null)
            {
                fieldInfo = type.GetField(fieldName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                type = type.BaseType;
            }

            if (fieldInfo == null)
                throw new ReflectionException(string.Format("Field '{0}' was not found in type '{1}'.", fieldName,
                    obj.GetType().FullName));

            fieldInfo.SetValue(obj, value);
        }
    }
}