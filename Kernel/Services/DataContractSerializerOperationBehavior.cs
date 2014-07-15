using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Description;
using System.Xml;
using PSOK.Kernel.Serialization;

namespace PSOK.Kernel.Services
{
    /// <summary>
    /// Class that exposes the custom serializer.
    /// </summary>
    public class DataContractSerializerOperationBehavior :
        System.ServiceModel.Description.DataContractSerializerOperationBehavior
    {
        /// <summary>
        /// Constructs a new <see cref="DataContractSerializerOperationBehavior"/> with the specified <see cref="System.ServiceModel.Description.OperationDescription"/>.
        /// </summary>
        /// <param name="operationDescription"></param>
        public DataContractSerializerOperationBehavior(OperationDescription operationDescription)
            : base(operationDescription)
        {
        }

        /// <summary>
        /// Gets called by WCF when a serializer is needed.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return Serializer.GetDataContractSerializer(type);
        }

        /// <summary>
        /// Gets called by WCF when a serializer is needed.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="ns"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns,
            IList<Type> knownTypes)
        {
            return Serializer.GetDataContractSerializer(type);
        }
    }
}