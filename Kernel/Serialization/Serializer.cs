using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using PSOK.Kernel.Environment;
using InterLinq;
using InterLinq.Expressions;
using InterLinq.Expressions.Helpers;
using log4net;

namespace PSOK.Kernel.Serialization
{
    /// <summary>
    /// Helper class for serializing and deserializing objects.
    /// </summary>
    public static class Serializer
    {
        // Static fields
        private static readonly ILog Log = LogManager.GetLogger(typeof (Serializer));

        /// <summary>
        /// Gets a data contract serializer for the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DataContractSerializer GetDataContractSerializer(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            return new DataContractSerializer(type, null, int.MaxValue, false, true, null, new DataContractResolver());
        }

        /// <summary>
        /// Serializes an object to XML.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>The XML.</returns>
        public static string Serialize<T>(T obj)
        {
            if (ReferenceEquals(obj, null))
                throw new ArgumentNullException("obj");

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Encoding = System.Text.Encoding.UTF8,
                Indent = true,
                IndentChars = "\t",
                NewLineChars = System.Environment.NewLine,
                ConformanceLevel = ConformanceLevel.Document,
                NewLineHandling = NewLineHandling.Replace,
                CloseOutput = false,
                DoNotEscapeUriAttributes = true,
                NamespaceHandling = NamespaceHandling.Default,
                CheckCharacters = false,
                NewLineOnAttributes = false,
                OmitXmlDeclaration = false,
                WriteEndDocumentOnClose = true,
                Async = false
            };

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings))
                {
                    try
                    {
                        DataContractSerializer dataContractSerializer = GetDataContractSerializer(typeof(T));
                        dataContractSerializer.WriteObject(xmlWriter, obj);
                        xmlWriter.Flush();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }

                    return System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
                }
            }
        }

        /// <summary>
        /// Serializes an object to an XML file.
        /// The XML file will be overwritten if it exists.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="filePath">The file path to save the XML to. May be relative.</param>
        public static void SerializeToFile<T>(T obj, string filePath)
        {
            if (ReferenceEquals(obj, null))
                throw new ArgumentNullException("obj");

            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            try
            {
                if (filePath.StartsWith("/"))
                    filePath = string.Format("{0}{1}", EnvironmentHelper.GetEnvironmentPath(), filePath);

                int index = filePath.LastIndexOf("/", StringComparison.Ordinal);

                if (index > -1)
                {
                    string directory = filePath.Substring(0, index);
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                }

                string xml = Serialize(obj);
                using (StreamWriter streamWriter = new StreamWriter(filePath, false, System.Text.Encoding.UTF8))
                {
                    streamWriter.Write(xml); 
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        /// <summary>
        /// Deserializes XML to an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="xml">The XML.</param>
        /// <returns>The type of the object to return.</returns>
        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentNullException("xml");

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml)))
                {
                    DataContractSerializer dataContractSerializer = GetDataContractSerializer(typeof(T));
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8);
                    return (T)dataContractSerializer.ReadObject(xmlTextWriter.BaseStream);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return default(T);
        }

        /// <summary>
        /// Deserializes an XML file to an object.
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="filePath">The file path of the XML file. May be relative.</param>
        /// <returns>The type of the object to return.</returns>
        public static T DeserializeFromFile<T>(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException("filePath");

            try
            {
                if (filePath.StartsWith("/") || filePath.StartsWith("~/"))
                    filePath = string.Format("{0}{1}", EnvironmentHelper.GetEnvironmentPath(), filePath);

                if (!File.Exists(filePath))
                    throw new FileNotFoundException(string.Format("Could not find file at '{0}'.", filePath));

                string xml = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
                return Deserialize<T>(xml);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return default(T);
        }

        /// <summary>
        /// Serializes the given object to a byte array.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            IFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, obj);
                return memoryStream.GetBuffer();
            }
        }

        /// <summary>
        /// Serializes an <see cref="Expression"/>.
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string SerializeExpression(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return Serialize(expression.MakeSerializable());
        }

        /// <summary>
        /// Deserializes the given byte array to an object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T Deserialize<T>(byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException("buffer");

            IFormatter binaryFormatter = new BinaryFormatter();
            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                return (T)binaryFormatter.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Deserializes the specified xml to an <see cref="Expression"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static Expression<T> DeserializeExpression<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
                throw new ArgumentNullException("xml");
            
            SerializableExpression expression = Deserialize<SerializableExpression>(xml);
            SerializableExpressionConverter converter = new SerializableExpressionConverter(expression, new DummyQueryHandler());
            return converter.Visit(expression) as Expression<T>;
        }

        private class DummyQueryHandler : IQueryHandler
        {
            public IQueryable<T> GetTable<T>() where T : class
            {
                throw new NotImplementedException();
            }

            public IQueryable GetTable(Type type)
            {
                throw new NotImplementedException();
            }

            public IQueryable Get(Type type)
            {
                throw new NotImplementedException();
            }

            public IQueryable<T> Get<T>() where T : class
            {
                throw new NotImplementedException();
            }

            public bool StartSession()
            {
                return true;
            }

            public bool CloseSession()
            {
                return true;
            }
        }

    }
}