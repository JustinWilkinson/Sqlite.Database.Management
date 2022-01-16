using Sqlite.Database.Management.Attributes;
using Sqlite.Database.Management.Enumerations;
using Sqlite.Database.Management.Exceptions;
using Sqlite.Database.Management.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Maps an object of type T to a SQLite table.
    /// </summary>
    /// <typeparam name="T">Type of object to map to.</typeparam>
    public interface IObjectMapper<T>
    {
        /// <summary>
        /// Creates a new record in the table corresponding to T in the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database">Database to create record in.</param>
        /// <param name="instance">Instance to convert to record.</param>
        void Insert(DatabaseBase database, T instance);

        /// <summary>
        /// Asynchronously creates a new record in the table corresponding to T in the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database">Database to create record in.</param>
        /// <param name="instance">Instance to convert to record.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>>A task representing the insert operation.</returns>
        Task InsertAsync(DatabaseBase database, T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a record in the table corresponding to the instance of T in the database note that the relevant table must already exist, have a primary key, and the primary key must be provided.
        /// </summary>
        /// <param name="database">Database to update record in.</param>
        /// <param name="instance">Instance to convert to record.</param>
        void Update(DatabaseBase database, T instance);

        /// <summary>
        /// Asynchronously updates a record in the table corresponding to T in the database. Note that the relevant table must already exist, have a primary key, and the primary key must be provided.
        /// </summary>
        /// <param name="database">Database to update record in.</param>
        /// <param name="instance">Instance to convert to record.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>>A task representing the update operation.</returns>
        Task UpdateAsync(DatabaseBase database, T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a record in the table corresponding to T in the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database">Database to delete record from.</param>
        /// <param name="instance">Instance to convert to record.</param>
        void Delete(DatabaseBase database, T instance);

        /// <summary>
        /// Asynchronously deletes a record in the table corresponding to T in the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database">Database to delete record from.</param>
        /// <param name="instance">Instance to convert to record.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>A task representing the delete operation.</returns>
        Task DeleteAsync(DatabaseBase database, T instance, CancellationToken cancellationToken = default);

        /// <summary>
        /// Selects a single record from the database that has the provided id. Note that the relevant table must already exist, and have a primary key.
        /// </summary>
        /// <typeparam name="TId">The type of the id column.</typeparam>
        /// <param name="database">Database to select from</param>
        /// <param name="id">Id to select record for.</param>
        /// <returns>A mapped record pulled from the database.</returns>
        T Select<TId>(DatabaseBase database, TId id);

        /// <summary>
        /// Asynchronously selects a single record from the database that has the provided id. Note that the relevant table must already exist, and have a primary key.
        /// </summary>
        /// <typeparam name="TId">The type of the id column.</typeparam>
        /// <param name="database">Database to select from</param>
        /// <param name="id">Id to select record for.</param>
        /// <returns>A mapped record pulled from the database.</returns>
        Task<T> SelectAsync<TId>(DatabaseBase database, TId id);

        /// <summary>
        /// Selects records as an <see cref="IEnumerable{T}"/> from the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database">Database to select from</param>
        /// <returns>An <see cref="IEnumerable{T}"/> converted from the database records.</returns>
        IEnumerable<T> Select(DatabaseBase database);

#if !NETSTANDARD2_0
        /// <summary>
        /// Asynchronosly selects records as an <see cref="IAsyncEnumerable{T}"/> from the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database">Database to select from</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>An <see cref="IAsyncEnumerable{T}"/> converted from the database records.</returns>
        IAsyncEnumerable<T> SelectAsync(DatabaseBase database, CancellationToken cancellationToken = default);
#else
        /// <summary>
        /// Asynchronosly selects records as an <see cref="IEnumerable{T}"/> from the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database">Database to select from.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> converted from the database records.</returns>
        Task<IEnumerable<T>> SelectAsync(DatabaseBase database, CancellationToken cancellationToken = default);
#endif
    }

    /// <summary>
    /// Maps an object of type T to a SQLite table.
    /// </summary>
    /// <typeparam name="T">Type of object to map to.</typeparam>
    public class ObjectMapper<T> : IObjectMapper<T>
    {
#region Static Readonly
        private static readonly Dictionary<string, Func<T, object>> Getters = new();
        private static readonly Dictionary<string, Action<T, object>> Setters = new();
        private static readonly Func<T> Constructor;

        private static readonly HashSet<Type> IntegerTypes = new()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        };

        private static readonly HashSet<Type> RealTypes = new()
        {
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        private static readonly Type PrimaryKeyType;
#endregion

        /// <summary>
        /// The table to map.
        /// </summary>
        public static Table Table { get; }

        /// <summary>
        /// Constructs a row mapper and caches a function.
        /// </summary>
        static ObjectMapper()
        {
            var type = typeof(T);
            var properties = type.GetPublicInstanceProperties();
            Table = new Table(type.Name) { Columns = new List<Column>() };

            foreach (var property in properties)
            {
                if (IsPrimaryKey(property))
                {
                    Table.PrimaryKey = property.Name;
                    PrimaryKeyType = property.PropertyType;
                }

                if (!IsIgnored(property))
                {
                    Getters.Add(property.Name, property.GetGetter<T>());
                    Setters.Add(property.Name, property.GetSetter<T>());
                    Table.Columns.Add(GetColumn(property));
                }
            }

            if (Table.PrimaryKey is null)
            {
                foreach (var property in properties)
                {
                    if (property.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) || property.Name.Equals($"{Table.Name}Id", StringComparison.OrdinalIgnoreCase))
                    {
                        Table.PrimaryKey = property.Name;
                        PrimaryKeyType = property.PropertyType;
                    }
                }
            }

            if (type == typeof(string))
            {
                Constructor = Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();
            }
            else if (type.IsValueType || type.GetConstructor(Type.EmptyTypes) is not null)
            {
                Constructor = Expression.Lambda<Func<T>>(Expression.New(type)).Compile();
            }
            else
            {
                Constructor = () => (T)FormatterServices.GetUninitializedObject(type);
            }
        }

        /// <summary>
        /// Gets the corresponding SQLite <see cref="ColumnType"/> from a <see cref="Type"/>.
        /// </summary>
        /// <param name="type">Type to get column type for.</param>
        /// <returns>The appropriate column type for objects of type.</returns>
        public static Column GetColumn(PropertyInfo property)
        {
            var type = property.PropertyType;
            var name = property.Name;

            if (type.IsValueType)
            {
                if (IntegerTypes.Contains(type))
                {
                    return new Column(name, ColumnType.Integer) { Nullable = false };
                }
                else if (RealTypes.Contains(type))
                {
                    return new Column(name, ColumnType.Real) { Nullable = false };
                }
                else if (type == typeof(bool))
                {
                    return new Column(name, ColumnType.Integer) { Nullable = false, CheckExpression = "IN (0, 1)" };
                }
            }
            else
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType is not null)
                {
                    if (IntegerTypes.Contains(underlyingType))
                    {
                        return new Column(name, ColumnType.Integer);
                    }
                    if (RealTypes.Contains(underlyingType))
                    {
                        return new Column(name, ColumnType.Real);
                    }
                    else if (underlyingType == typeof(bool))
                    {
                        return new Column(name, ColumnType.Integer) { CheckExpression = "IN (0, 1, NULL)" };
                    }
                }
            }

            if (type.IsAssignableFrom(typeof(IEnumerable<byte>)))
            {
                return new Column(property.Name, ColumnType.Blob);
            }

            return new Column(property.Name);
        }

        /// <inheritdoc/>
        public void Insert(DatabaseBase database, T instance) => Execute(database, GetInsertCommand(instance));

        /// <inheritdoc/>
        public Task InsertAsync(DatabaseBase database, T instance, CancellationToken cancellationToken = default) => ExecuteAsync(database, GetInsertCommand(instance), cancellationToken);

        /// <inheritdoc/>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table does not have a primary key.</exception>
        public void Update(DatabaseBase database, T instance) => Execute(database, GetUpdateCommand(instance));

        /// <inheritdoc/>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table does not have a primary key.</exception>
        public Task UpdateAsync(DatabaseBase database, T instance, CancellationToken cancellationToken = default) => ExecuteAsync(database, GetUpdateCommand(instance), cancellationToken);

        /// <inheritdoc/>
        public void Delete(DatabaseBase database, T instance) => Execute(database, GetDeleteCommand(instance));

        /// <inheritdoc/>
        public Task DeleteAsync(DatabaseBase database, T instance, CancellationToken cancellationToken = default) => ExecuteAsync(database, GetDeleteCommand(instance), cancellationToken);

        /// <inheritdoc/>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table does not have a primary key.</exception>
        public T Select<TId>(DatabaseBase database, TId id)
        {
            using var command = GetSelectCommand(id);
            return database.Execute(command, Map).Single();
        }

#if !NETSTANDARD2_0
        /// <inheritdoc/>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table does not have a primary key.</exception>
        public async Task<T> SelectAsync<TId>(DatabaseBase database, TId id)
        {
            var command = GetSelectCommand(id);
            await using (command.ConfigureAwait(false))
            {
                return await database.ExecuteAsync(command, Map).SingleAsync();
            }
        }
#else
        /// <inheritdoc/>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table does not have a primary key.</exception>
        public async Task<T> SelectAsync<TId>(DatabaseBase database, TId id)
        {
            using var command = GetSelectCommand(id);
            return (await database.ExecuteAsync(command, Map)).Single();
        }
#endif

        /// <inheritdoc/>
        public IEnumerable<T> Select(DatabaseBase database)
        {
            using var command = GetSelectCommand();
            foreach (var record in database.Execute(command, Map))
            {
                yield return record;
            }
        }

#if !NETSTANDARD2_0
        /// <inheritdoc/>
        public async IAsyncEnumerable<T> SelectAsync(DatabaseBase database, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var command = GetSelectCommand();
            await using (command.ConfigureAwait(false))
            {
                await foreach (var record in database.ExecuteAsync(command, Map, cancellationToken).ConfigureAwait(false))
                {
                    yield return record;
                }
            }
        }
#else
        /// <inheritdoc/>
        public async Task<IEnumerable<T>> SelectAsync(DatabaseBase database, CancellationToken cancellationToken = default)
        {
            using var command = GetSelectCommand();
            return await database.ExecuteAsync(command, Map, cancellationToken).ConfigureAwait(false);
        }
#endif

        /// <summary>
        /// Maps a SQLiteDataReader to an object of type T.
        /// </summary>
        /// <param name="reader">SQLiteDataReader to populate object from.</param>
        /// <returns>A new instance of type T populated from the SQLiteDataReader.</returns>
        public T Map(SQLiteDataReader reader)
        {
            var instance = Constructor();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (Setters.TryGetValue(reader.GetName(i), out var setter))
                {
                    setter(instance, reader[i]);
                }
            }

            return instance;
        }

        #region Attribute Helpers
        private static bool IsPrimaryKey(PropertyInfo property)
        {
            var primaryKeyAttribute = property.GetCustomAttribute<SqlitePrimaryKeyAttribute>();
            if (primaryKeyAttribute is not null)
            {
                if (Table.PrimaryKey is not null)
                {
                    throw new NotSupportedException("Composite primary keys are not yet supported!");
                }

                if (IsIgnored(property))
                {
                    throw new InvalidOperationException("Cannot ignore the PrimaryKey!");
                }

                return true;
            }

            return false;
        }

        private static bool IsIgnored(PropertyInfo property) => property.GetCustomAttribute<SqliteIgnoreAttribute>() is not null;
        #endregion

        #region Execute
        private static void Execute(DatabaseBase database, SQLiteCommand command)
        {
            using (command)
            {
                database.Execute(command);
            }
        }

        private static async Task ExecuteAsync(DatabaseBase database, SQLiteCommand command, CancellationToken cancellationToken)
        {
#if !NETSTANDARD2_0
            await using (command.ConfigureAwait(false))
#else
            using (command)
#endif
            {
                await database.ExecuteAsync(command, cancellationToken).ConfigureAwait(false);
            }
        }
        #endregion

        #region Commands
        private static SQLiteCommand GetInsertCommand(T instance)
        {
            var command = new SQLiteCommand($"INSERT INTO {Table.Name} ({string.Join(",", Table.Columns.Select(c => c.Name))}) VALUES({string.Join(",", Table.Columns.Select(c => $"@{c.Name}"))})");
            Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", Getters[c.Name](instance)));
            return command;
        }

        private static SQLiteCommand GetUpdateCommand(T instance)
        {
            ThrowHelper.RequirePrimaryKey(Table);

            var command = new SQLiteCommand($"UPDATE {Table.Name} SET {string.Join(", ", Table.Columns.Select(c => $"{c.Name} = @{c.Name}"))} WHERE {Table.PrimaryKey} = @value");
            command.AddParameter("@value", Getters[Table.PrimaryKey](instance));
            Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", Getters[c.Name](instance)));
            return command;
        }

        private static SQLiteCommand GetDeleteCommand(T instance)
        {
            SQLiteCommand command;
            if (Table.PrimaryKey is not null)
            {
                command = new SQLiteCommand($"DELETE FROM {Table.Name} WHERE {Table.PrimaryKey} = @value");
                command.AddParameter("@value", Getters[Table.PrimaryKey](instance));
            }
            else
            {
                command = new SQLiteCommand($"DELETE FROM {Table.Name} WHERE {string.Join(" AND ", Table.Columns.Select(c => $"{c.Name} = @{c.Name}"))}");
                Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", Getters[c.Name](instance)));
            }

            return command;
        }

        private static SQLiteCommand GetSelectCommand<TId>(TId id)
        {
            ThrowHelper.ThrowIfArgumentNull(id);
            ThrowHelper.RequirePrimaryKey(Table);

#if NET5_0_OR_GREATER
            if (!typeof(TId).IsAssignableTo(PrimaryKeyType))
            {
                throw new ArgumentException($"Invalid value for PrimaryKey: \"{id}\" is not assignable to {PrimaryKeyType.Name}", nameof(id));
            }
#else
            if (typeof(TId) != PrimaryKeyType)
            {
                throw new ArgumentException($"Invalid value for PrimaryKey: \"{id}\" is not of type {PrimaryKeyType.Name}", nameof(id));
            } 
#endif

            return new($"SELECT * FROM {Table.Name} WHERE {Table.PrimaryKey} = {id}");
        }

        private static SQLiteCommand GetSelectCommand() => new($"SELECT * FROM {Table.Name}");
#endregion
    }
}