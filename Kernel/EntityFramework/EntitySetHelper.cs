using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using PSOK.Kernel.Caching;
using PSOK.Kernel.Reflection;

namespace PSOK.Kernel.EntityFramework
{
    /// <summary>
    /// Helper class for working with entity sets
    /// </summary>
    public static class EntitySetHelper
    {
        /// <summary>
        /// Gets the entity set name based on the given entity type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static string GetEntitySetName<T>(Type entityType) where T : DbContext
        {
            if (entityType == null)
                throw new ArgumentNullException("entityType");

            DbContext dbContext = Activator.CreateInstance(typeof(T)) as DbContext;

            if (dbContext == null)
                throw new ArgumentException(string.Format("Could not create an instance of the '{0}' of type '{1}'.",
                    typeof(DbContext).Name, typeof(T).AssemblyQualifiedName()));

            ObjectContext objectContext = (dbContext as IObjectContextAdapter).ObjectContext;
            return GetEntitySetName(objectContext, entityType);
        }

        /// <summary>
        /// Gets all entity set names from the given <see cref="System.Data.Entity.DbContext" /> type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<string> GetEntitySetNames<T>() where T : DbContext
        {
            return GetEntityTypes<T>().Select(GetEntitySetName<T>).ToList();
        }

        /// <summary>
        /// Gets all entity types from the given <see cref="System.Data.Entity.DbContext" /> type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<Type> GetEntityTypes<T>() where T : DbContext
        {
            IEnumerable<Type> entityTypes = typeof(T).GetProperties()
                .Where(
                    x =>
                        x.PropertyType.IsGenericType &&
                        typeof(IDbSet<>).IsAssignableFrom(x.PropertyType.GetGenericTypeDefinition()))
                .Select(x => x.PropertyType.GetGenericArguments().First()).ToList();
            return entityTypes;
        }

        /// <summary>
        /// Gets the entity set name based on the given <see cref="System.Data.Entity.Core.Objects.ObjectContext" /> and entity type.
        /// </summary>
        /// <param name="objectContext"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        public static string GetEntitySetName(ObjectContext objectContext, Type entityType)
        {
            if (objectContext == null)
                throw new ArgumentNullException("objectContext");

            if (entityType == null)
                throw new ArgumentNullException("entityType");

            string cacheKey = entityType.AssemblyQualifiedName;

            return CacheManager.GetOrAdd(cacheKey, () =>
            {
                EntityContainer entityContainer =
                    objectContext.MetadataWorkspace.GetEntityContainer(objectContext.DefaultContainerName,
                        DataSpace.CSpace);
                string entitySetName = entityContainer.BaseEntitySets.Where(x =>
                        string.Equals(x.ElementType.Name, entityType.Name, StringComparison.InvariantCulture))
                        .Select(x => x.Name)
                        .FirstOrDefault();
                return entitySetName;
            }, new CachingOptions<string>
            {
                EnableCaching = true,
                Expiration = new TimeSpan(1, 0, 0)
            });
        }
    }
}