using System.ComponentModel;
using System.Configuration;
using System.Reflection;
using System.Xml;
using PSOK.Kernel.Reflection;
using ConfigurationException = PSOK.Kernel.Exceptions.ConfigurationException;

namespace PSOK.Kernel.Configuration
{
    /// <summary>
    /// A configuration element which can contain CData information.
    /// </summary>
    public class CDataConfigurationElement : System.Configuration.ConfigurationElement
    {
        private readonly string _cDataConfigurationPropertyName;

        /// <summary>
        /// Constructs a new <see cref="CDataConfigurationElement"/>.
        /// </summary>
        public CDataConfigurationElement()
        {
            _cDataConfigurationPropertyName = GetCDataConfigurationPropertyName();
        }

        /// <summary>
        /// Retrieves the property name of the property annoted with the <see cref="CDataConfigurationPropertyAttribute" />
        /// attribute.
        /// Only one property may have this attribut as a configuration element can only have one CData value.
        /// </summary>
        /// <returns></returns>
        private string GetCDataConfigurationPropertyName()
        {
            string cDataConfigurationPropertyName = null;

            PropertyInfo[] properties = GetType().GetProperties();
            int cDataConfigurationPropertyCount = 0;
            int configurationElementPropertyCount = 0;

            foreach (PropertyInfo property in properties)
            {
                ConfigurationPropertyAttribute[] configurationPropertyAttributes =
                    PropertyHelper.GetAttributes<ConfigurationPropertyAttribute>(property);
                CDataConfigurationPropertyAttribute[] cDataConfigurationPropertyAttribute =
                    PropertyHelper.GetAttributes<CDataConfigurationPropertyAttribute>(property);

                bool hasConfigurationPropertyAttribute = configurationPropertyAttributes.Length != 0;
                bool hasCDataConfigurationPropertyAttribute = cDataConfigurationPropertyAttribute.Length != 0;

                if (hasConfigurationPropertyAttribute &&
                    property.PropertyType.IsSubclassOf(typeof (ConfigurationElement)))
                {
                    configurationElementPropertyCount++;
                }

                if (hasCDataConfigurationPropertyAttribute)
                {
                    cDataConfigurationPropertyCount++;

                    if (cDataConfigurationPropertyCount > 1)
                    {
                        throw new ConfigurationException(
                            "The configuration element has too many CData configuration elements.");
                    }

                    if (!hasConfigurationPropertyAttribute)
                    {
                        throw new ConfigurationException("The property is missing a configuration property attribute.");
                    }

                    if (!(property.PropertyType == typeof (string)))
                    {
                        throw new ConfigurationException("The type of the CDATA property must be string.");
                    }

                    cDataConfigurationPropertyName = configurationPropertyAttributes[0].Name;
                }
            }

            // A element containing configuration element properties cannot also have a CData value
            // It makes no sense as the child elements would be exactly the CData.
            if (configurationElementPropertyCount > 0 && cDataConfigurationPropertyCount > 0)
                throw new ConfigurationException("The class contains configuration element properties.");

            return cDataConfigurationPropertyName;
        }

        /// <summary>
        /// Serializes a configuration element.
        /// </summary>
        /// <param name="xmlWriter"></param>
        /// <param name="serializeCollectionKey"></param>
        /// <returns></returns>
        protected override bool SerializeElement(XmlWriter xmlWriter, bool serializeCollectionKey)
        {
            bool returnValue;
            if (string.IsNullOrEmpty(_cDataConfigurationPropertyName))
            {
                returnValue = base.SerializeElement(xmlWriter, serializeCollectionKey);
            }
            else
            {
                foreach (ConfigurationProperty configurationProperty in Properties)
                {
                    string name = configurationProperty.Name;
                    TypeConverter converter = configurationProperty.Converter;
                    string propertyValue = converter.ConvertToString(base[name]) ?? string.Empty;

                    if (name == _cDataConfigurationPropertyName)
                    {
                        xmlWriter.WriteCData(propertyValue);
                    }
                    else
                    {
                        xmlWriter.WriteAttributeString("name", propertyValue);
                    }
                }
                returnValue = true;
            }
            return returnValue;
        }

        /// <summary>
        /// Deserializes a configuration element.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="serializeCollectionKey"></param>
        protected override void DeserializeElement(XmlReader reader, bool serializeCollectionKey)
        {
            if (string.IsNullOrEmpty(_cDataConfigurationPropertyName))
            {
                base.DeserializeElement(reader, serializeCollectionKey);
            }
            else
            {
                foreach (ConfigurationProperty configurationProperty in Properties)
                {
                    string name = configurationProperty.Name;
                    if (name == _cDataConfigurationPropertyName)
                    {
                        string contentString = reader.ReadString();
                        base[name] = contentString.Trim();
                    }
                    else
                    {
                        string attributeValue = reader.GetAttribute(name);
                        base[name] = attributeValue;
                    }
                }
                reader.ReadEndElement();
            }
        }
    }
}