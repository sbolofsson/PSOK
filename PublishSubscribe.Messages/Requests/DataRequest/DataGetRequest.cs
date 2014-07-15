using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Runtime.Serialization;
using PSOK.PublishSubscribe.Messages.Entities;
using PSOK.PublishSubscribe.Messages.Responses.DataResponse;

namespace PSOK.PublishSubscribe.Messages.Requests.DataRequest
{
    /// <summary>
    /// A request to retrieve some entities.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [Serializable, DataContract]
    public class DataGetRequest<TEntity> : DataRequest where TEntity : Entity
    {
        /// <summary>
        /// The query to use for retrieving the entities.
        /// </summary>
        public Func<DataServiceQuery<TEntity>, IEnumerable<TEntity>> Query { private get; set; }

        /// <summary>
        /// Executes the <see cref="DataGetRequest{TEntity}" />.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Message Execute(DataRequestParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (Query == null)
                throw new ArgumentException("Could not execute request because no query has been set.");

            DataServiceContext serviceContext = Resolve(parameters);
            DataServiceQuery<TEntity> query = serviceContext.CreateQuery<TEntity>(parameters.EntitySet);
            IEnumerable<TEntity> result = Query(query);
            DataServiceQuery<TEntity> dataServiceQuery = result as DataServiceQuery<TEntity>;
            IEnumerable<TEntity> entities = dataServiceQuery != null ? dataServiceQuery.Execute() : result;
            return new DataResponse<IEnumerable<TEntity>> {Result = entities};
        }
    }

    /// <summary>
    /// A request to retrieve some data.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class DataGetRequest<TEntity, TResult> : DataRequest where TEntity : Entity
    {
        /// <summary>
        /// The query to use for retrieving the data.
        /// </summary>
        public Func<DataServiceQuery<TEntity>, TResult> Query { private get; set; }

        /// <summary>
        /// Executes the <see cref="DataGetRequest{TEntity,TResult}" />.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Message Execute(DataRequestParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (Query == null)
                throw new ArgumentException("Could not execute request because no query has been set.");

            DataServiceContext serviceContext = Resolve(parameters);
            DataServiceQuery<TEntity> query = serviceContext.CreateQuery<TEntity>(parameters.EntitySet);
            TResult result = Query(query);
            return new DataResponse<TResult> {Result = result};
        }

        /// <summary>
        /// Indicates the message type of the <see cref="DataGetRequest{T}"/>.
        /// </summary>
        /// <returns></returns>
        public override Type GetMessageType()
        {
            return typeof (DataGetRequest<TEntity>);
        }
    }
}