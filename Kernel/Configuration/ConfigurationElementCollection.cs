using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace PSOK.Kernel.Configuration
{
    /// <summary>
    /// A generic configuration element collection class.
    /// that allows configuration elements to be named at runtime.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConfigurationElementCollection<T> : ConfigurationElementCollection, IList<T>
        where T : System.Configuration.ConfigurationElement, new()
    {
        private readonly string _name;

        /// <summary>
        /// Constructs a new configuration collection.
        /// </summary>
        public ConfigurationElementCollection()
        {
        }

        /// <summary>
        /// Constructs a new configuration collection with the given name.
        /// </summary>
        /// <param name="name"></param>
        public ConfigurationElementCollection(string name)
        {
            _name = name;
        }

        /// <summary>
        /// The name of the configuration element.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Creates a new element of type T.
        /// </summary>
        /// <returns></returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        /// <summary>
        /// Creates a new element with the given name of type T.
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        protected override System.Configuration.ConfigurationElement CreateNewElement(string elementName)
        {
            return new T();
        }

        /// <summary>
        /// Gets an element key based on the given element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        protected override object GetElementKey(System.Configuration.ConfigurationElement element)
        {
            PropertyInfo[] properties = typeof (T).GetProperties();
            PropertyInfo keyProperty = (from property in properties
                where property.IsDefined(typeof (ConfigurationPropertyAttribute), true)
                let attribute =
                    property.GetCustomAttributes(typeof (ConfigurationPropertyAttribute), true)[0] as
                        ConfigurationPropertyAttribute
                where attribute != null && attribute.IsKey
                select property).FirstOrDefault();

            object key = null;
            if (keyProperty != null)
            {
                key = keyProperty.GetValue(element, null);
            }

            return key ?? element; // Returning null is not allowed
        }

        /// <summary>
        /// This is the trick - always return true for any given element name.
        /// This allows a dynamic expansion of the configuration collection, which is just what we want.
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        protected override bool IsElementName(string elementName)
        {
            return true;
        }

        /// <summary>
        /// The collection type.
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        /// <summary>
        /// Default enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            foreach (T type in this)
            {
                yield return type;
            }
        }

        /// <summary>
        /// Adds a configuration element of type T.
        /// </summary>
        /// <param name="configurationElement"></param>
        public void Add(T configurationElement)
        {
            BaseAdd(configurationElement, true);
        }

        /// <summary>
        /// Clears the collection.
        /// </summary>
        public void Clear()
        {
            BaseClear();
        }

        /// <summary>
        /// Checks if a configuration element is contained in the collection.
        /// </summary>
        /// <param name="configurationElement"></param>
        /// <returns></returns>
        public bool Contains(T configurationElement)
        {
            return !(IndexOf(configurationElement) < 0);
        }

        /// <summary>
        /// Copies the contents of the configuration element collection to an array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            // ReSharper disable CoVariantArrayConversion
            base.CopyTo(array, arrayIndex);
            // ReSharper restore CoVariantArrayConversion
        }

        /// <summary>
        /// Removes a configuration element.
        /// </summary>
        /// <param name="configurationElement"></param>
        /// <returns></returns>
        public bool Remove(T configurationElement)
        {
            BaseRemove(GetElementKey(configurationElement));
            return true;
        }

        /// <summary>
        /// Indicates whether the configuration element collection is readonly.
        /// </summary>
        bool ICollection<T>.IsReadOnly
        {
            get { return IsReadOnly(); }
        }

        /// <summary>
        /// Indicates the index of the specified configuration element.
        /// </summary>
        /// <param name="configurationElement"></param>
        /// <returns></returns>
        public int IndexOf(T configurationElement)
        {
            return BaseIndexOf(configurationElement);
        }

        /// <summary>
        /// Adds a configuration element to the configuration element collection.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="configurationElement"></param>
        public void Insert(int index, T configurationElement)
        {
            BaseAdd(index, configurationElement);
        }

        /// <summary>
        /// Removes a configuration element from the collection.
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        /// <summary>
        /// Gets a configuration element at the specified index location.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public T this[int index]
        {
            get { return (T) BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
    }
}