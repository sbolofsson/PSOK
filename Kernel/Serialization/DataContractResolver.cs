using System;
using System.Xml;
using PSOK.Kernel.Encoding;
using PSOK.Kernel.Reflection;
using PSOK.Kernel.Services;
using log4net;

namespace PSOK.Kernel.Serialization
{
    /// <summary>
    /// Custom data contract resolver to resolve types at runtime in WCF services.
    /// Implementation inspired by
    /// http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractresolver.aspx
    /// </summary>
    public class DataContractResolver : System.Runtime.Serialization.DataContractResolver
    {
        private static readonly System.Runtime.Serialization.DataContractResolver DummyDataContractResolver = null;

        private static readonly ILog Log = LogManager.GetLogger(typeof(KnownTypeResolver));

        /// <summary>
        /// Checks if type is considered a known type.
        /// Gets called by the WCF framework when serializing.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="declaredType"></param>
        /// <param name="knownTypeResolver"></param>
        /// <param name="typeName"></param>
        /// <param name="typeNamespace"></param>
        /// <returns></returns>
        public override bool TryResolveType(Type type, Type declaredType,
            System.Runtime.Serialization.DataContractResolver knownTypeResolver, out XmlDictionaryString typeName,
            out XmlDictionaryString typeNamespace)
        {

            try
            {
                if (!KnownTypeResolver.IsKnownType(type))
                    return knownTypeResolver.TryResolveType(type, declaredType, DummyDataContractResolver, out typeName,
                        out typeNamespace);

                XmlDictionary dictionary = new XmlDictionary();
                typeName = dictionary.Add(Encoder.SafeEncode(type.AssemblyQualifiedName()));
                typeNamespace = dictionary.Add("http://p2p.com");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }

        /// <summary>
        /// Resolves a type by name.
        /// Gets called by the WCF framework when deserializing.
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="typeNamespace"></param>
        /// <param name="declaredType"></param>
        /// <param name="knownTypeResolver"></param>
        /// <returns></returns>
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType,
            System.Runtime.Serialization.DataContractResolver knownTypeResolver)
        {
            try
            {
                return KnownTypeResolver.ResolveType(Encoder.SafeDecode(typeName)) ??
                       (KnownTypeResolver.IsKnownType(declaredType)
                           ? declaredType
                           : knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType,
                               DummyDataContractResolver));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }
    }
}