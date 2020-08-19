using Sqlite.Database.Management.Mapping;
using System;
using System.Collections.Generic;

namespace Sqlite.Database.Management.Extensions
{
    /// <summary>
    /// Contains extension methods on the <see cref="DatabaseBase"/> class which act as a wrapper to the <see cref="ObjectMapper{T}"/>.
    /// </summary>
    public static class DatabaseExtensions
    {
        private static readonly Dictionary<Type, object> _mappers = new Dictionary<Type, object>();

        /// <summary>
        /// Inserts a new record into the database table with the same name as the type (this table must already exist).
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to insert value into.</param>
        /// <param name="instance">Instance to insert into database.</param>
        public static void Insert<T>(this DatabaseBase database, T instance) => GetMapper<T>().Insert(database, instance);

        /// <summary>
        /// Updates a record in the table corresponding to the instance of T in the database note that the relevant table must already exist, have a primary key, and the primary key must be provided.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to update value in.</param>
        /// <param name="instance">Instance to update in the database.</param>
        public static void Update<T>(this DatabaseBase database, T instance) => GetMapper<T>().Update(database, instance);

        /// <summary>
        /// Deletes a record in the table corresponding to T in the database. Note that the relevant table must already exist.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        /// <param name="instance">Instance to delete in the database.</param>
        public static void Delete<T>(this DatabaseBase database, T instance) => GetMapper<T>().Delete(database, instance);

        /// <summary>
        /// Selects records as an IEnumerable of T from the database. Note that the relevant table must already exist.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        public static IEnumerable<T> Select<T>(this DatabaseBase database) => GetMapper<T>().Select(database);

        /// <summary>
        /// Gets an object mapper for type T, and ensures it is cached for performance.
        /// </summary>
        /// <typeparam name="T">Type parameter for object mapper</typeparam>
        /// <returns>An object mapper for type T.</returns>
        private static ObjectMapper<T> GetMapper<T>()
        {
            var type = typeof(T);
            if (!_mappers.TryGetValue(type, out var mapper))
            {
                mapper = new ObjectMapper<T>();
                _mappers.Add(type, mapper);
            }

            return mapper as ObjectMapper<T>;
        }
    }
}