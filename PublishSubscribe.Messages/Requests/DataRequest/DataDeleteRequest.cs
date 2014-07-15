using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Runtime.Serialization;
using PSOK.PublishSubscribe.Messages.Entities;

namespace PSOK.PublishSubscribe.Messages.Requests.DataRequest
{
    /// <summary>
    /// A request to delete some entities.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    [Serializable, DataContract]
    public class DataDeleteRequest<TEntity> : DataRequest where TEntity : Entity
    {
        /// <summary>
        /// The entities to delete.
        /// </summary>
        public IEnumerable<TEntity> Entities { get; set; }

        /// <summary>
        /// Executes the <see cref="DataDeleteRequest{TEntity}" />.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Message Execute(DataRequestParameters parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            if (Entities == null || !Entities.Any())
                throw new ArgumentException("Could not execute request because no entities have been set.");

            DataServiceContext serviceContext = Resolve(parameters);

            foreach (TEntity entity in Entities)
            {
                if (!serviceContext.Entities.Contains(serviceContext.GetEntityDescriptor(entity)))
                {
                    serviceContext.AttachTo(parameters.EntitySet, entity);
                }
                serviceContext.DeleteObject(entity);
            }
            serviceContext.SaveChanges();
            return null;
        }
    }
}