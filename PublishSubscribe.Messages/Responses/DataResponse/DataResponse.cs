using System;
using System.Runtime.Serialization;

namespace PSOK.PublishSubscribe.Messages.Responses.DataResponse
{
    /// <summary>
    /// Base class for data responses.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable, DataContract]
    public class DataResponse<T> : Response
    {
        /// <summary>
        /// The result of the <see cref="DataResponse{T}" />.
        /// </summary>
        public T Result { get; set; }
    }
}