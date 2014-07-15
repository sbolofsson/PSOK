using System.Data.Entity;
using PSOK.Kernel.Services;
using PSOK.PublishSubscribe.Messages.Requests.DataRequest;

namespace PSOK.PublishSubscribe.Services
{
    /// <summary>
    /// A data context owned by an <see cref="IPeer" />.
    /// </summary>
    public interface IDataContext : IServiceHost
    {
        /// <summary>
        /// Registers an <see cref="IDataServiceBase{T}" />.
        /// All data requests are automatically subscribed to.
        /// Other peers can access the service by publishing <see cref="DataRequest" />s.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RegisterDataService<T>() where T : IDataServiceBase<DbContext>, new();

        /// <summary>
        /// Registers a <see cref="DbContext" />.
        /// All data requests are automatically subscribed to.
        /// Other peers can access the entities by publishing <see cref="DataRequest" />s.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void RegisterDbContext<T>() where T : DbContext, new();
    }
}