using Sqlite.Database.Management.Exceptions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlite.Database.Management.Extensions
{
    /// <summary>
    /// Contains extension methods on the <see cref="DatabaseBase"/> class which act as a wrapper to the <see cref="ObjectMapper{T}"/>.
    /// </summary>
    public static class DatabaseExtensions
    {
        private static readonly ConcurrentDictionary<Type, object> Mappers = new();

        /// <summary>
        /// Inserts a new record into the database table with the same name as the type (this table must already exist).
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to insert value into.</param>
        /// <param name="instance">Instance to insert into database.</param>
        public static void Insert<T>(this DatabaseBase database, T instance) => GetMapper<T>().Insert(database, instance);

        /// <summary>
        /// Asynchronously inserts a new record into the database table with the same name as the type (this table must already exist).
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to insert value into.</param>
        /// <param name="instance">Instance to insert into database.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>A task representing the insert operation.</returns>
        public static Task InsertAsync<T>(this DatabaseBase database, T instance, CancellationToken cancellationToken = default) => GetMapper<T>().InsertAsync(database, instance, cancellationToken);

        /// <summary>
        /// Updates a record in the table corresponding to the instance of T in the database note that the relevant table must already exist, have a primary key, and the primary key must be provided.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to update value in.</param>
        /// <param name="instance">Instance to update in the database.</param>
        public static void Update<T>(this DatabaseBase database, T instance) => GetMapper<T>().Update(database, instance);

        /// <summary>
        /// Asynchronously updates a record in the table corresponding to the instance of T in the database note that the relevant table must already exist, have a primary key, and the primary key must be provided.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to update value in.</param>
        /// <param name="instance">Instance to update in the database.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>>A task representing the update operation.</returns>
        public static Task UpdateAsync<T>(this DatabaseBase database, T instance, CancellationToken cancellationToken = default) => GetMapper<T>().UpdateAsync(database, instance, cancellationToken);

        /// <summary>
        /// Deletes a record in the table corresponding to T in the database. Note that the relevant table must already exist.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        /// <param name="instance">Instance to delete in the database.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        public static void Delete<T>(this DatabaseBase database, T instance) => GetMapper<T>().Delete(database, instance);

        /// <summary>
        /// Asynchronously deletes a record in the table corresponding to T in the database. Note that the relevant table must already exist.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        /// <param name="instance">Instance to delete in the database.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>A task representing the delete operation.</returns>
        public static Task DeleteAsync<T>(this DatabaseBase database, T instance, CancellationToken cancellationToken = default) => GetMapper<T>().DeleteAsync(database, instance, cancellationToken);

        /// <summary>
        /// Selects records as an as an <see cref="IEnumerable{T}"/> from the database. Note that the relevant table must already exist.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        /// <returns>A mapped <see cref="IEnumerable{T}"/> of the records from the database.</returns>
        public static IEnumerable<T> Select<T>(this DatabaseBase database) => GetMapper<T>().Select(database);

        /// <summary>
        /// Selects records as an as an <see cref="IEnumerable{T}"/> from the database. Note that the relevant table must already exist and have a primary key.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        /// <param name="database">Database to delete value from.</param>
        /// <returns>A mapped <see cref="IAsyncEnumerable{T}"/> of the records from the database.</returns>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table has no primary key.</exception>
        /// <exception cref="ArgumentException">Thrown when the type passed as the primary key does not match the primary key of the table.</exception>
        public static TRow Select<TRow, TId>(this DatabaseBase database, TId id) => GetMapper<TRow>().Select(database, id);

        /// <summary>
        /// Selects records as an as an <see cref="IEnumerable{T}"/> from the database. Note that the relevant table must already exist and have a primary key.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        /// <param name="database">Database to delete value from.</param>
        /// <returns>A mapped <see cref="IAsyncEnumerable{T}"/> of the records from the database.</returns>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table has no primary key.</exception>
        /// <exception cref="ArgumentException">Thrown when the type passed as the primary key does not match the primary key of the table.</exception>
        public static Task<TRow> SelectAsync<TRow, TId>(this DatabaseBase database, TId id) => GetMapper<TRow>().SelectAsync(database, id);

#if !NETSTANDARD2_0
        /// <summary>
        /// Asynchronously elects records as an <see cref="IAsyncEnumerable{T}"/> from the database. Note that the relevant table must already exist.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>A mapped <see cref="IAsyncEnumerable{T}"/> of the records from the database.</returns>
        public static IAsyncEnumerable<T> SelectAsync<T>(this DatabaseBase database, CancellationToken cancellationToken = default) => GetMapper<T>().SelectAsync(database, cancellationToken);
#else
        /// <summary>
        /// Asynchronously elects records as an <see cref="IEnumerable{T}"/> from the database. Note that the relevant table must already exist.
        /// </summary>
        /// <typeparam name="T">Type of record.</typeparam>
        /// <param name="database">Database to delete value from.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>A mapped <see cref="IEnumerable{T}"/> of the records from the database.</returns>
        public static Task<IEnumerable<T>> SelectAsync<T>(this DatabaseBase database, CancellationToken cancellationToken = default) => GetMapper<T>().SelectAsync(database, cancellationToken);
#endif

        /// <summary>
        /// Gets an object mapper for type T, and ensures it is cached for performance.
        /// </summary>
        /// <typeparam name="T">Type parameter for object mapper</typeparam>
        /// <returns>An object mapper for type T.</returns>
        private static ObjectMapper<T> GetMapper<T>()
        {
            var type = typeof(T);
            if (!Mappers.TryGetValue(type, out var mapper))
            {
                mapper = new ObjectMapper<T>();
                Mappers.TryAdd(type, mapper);
            }

            return mapper as ObjectMapper<T>;
        }
    }
}