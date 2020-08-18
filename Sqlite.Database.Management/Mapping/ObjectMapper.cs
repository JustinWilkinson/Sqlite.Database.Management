using Sqlite.Database.Management.Enumerations;
using Sqlite.Database.Management.Exceptions;
using Sqlite.Database.Management.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Sqlite.Database.Management.Mapping
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
        public void Insert(DatabaseBase database, T instance);

        /// <summary>
        /// Updates a record in the table corresponding to the instance of T in the database note that the relevant table must already exist, have a primary key, and the primary key must be provided.
        /// </summary>
        /// <param name="database">Database to create record in.</param>
        /// <param name="instance">Instance to convert to record.</param>
        public void Update(DatabaseBase database, T instance);

        /// <summary>
        /// Deletes a record in the table corresponding to T in the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database"></param>
        /// <param name="instance"></param>
        public void Delete(DatabaseBase database, T instance);

        /// <summary>
        /// Selects records as an IEnumerable of T from the database. Note that the relevant table must already exist.
        /// </summary>
        /// <param name="database">Database to select from</param>
        /// <returns>An IEnumerable of T converted from the database records.</returns>
        public IEnumerable<T> Select(DatabaseBase database);
    }

    /// <summary>
    /// Maps an object of type T to a SQLite table.
    /// </summary>
    /// <typeparam name="T">Type of object to map to.</typeparam>
    public class ObjectMapper<T> : IObjectMapper<T>
    {
        #region Static Readonly
        private static readonly Dictionary<string, Func<T, object>> _getters = new Dictionary<string, Func<T, object>>();
        private static readonly Dictionary<string, Action<T, object>> _setters = new Dictionary<string, Action<T, object>>();
        private static readonly Func<T> _constructor;

        private static readonly HashSet<Type> _integerTypes = new HashSet<Type>
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

        private static readonly HashSet<Type> _realTypes = new HashSet<Type>
        {
            typeof(float),
            typeof(double),
            typeof(decimal)
        };
        #endregion

        public static Table Table { get; }

        /// <summary>
        /// Constructs a row mapper and caches a function.
        /// </summary>
        static ObjectMapper()
        {
            Type type = typeof(T);
            Table = new Table(nameof(T)) { Columns = new List<Column>() };

            foreach (var property in type.GetPublicInstanceProperties())
            {
                _getters.Add(property.Name, property.GetGetter<T>());
                _setters.Add(property.Name, property.GetSetter<T>());
                Table.Columns.Add(GetColumn(property));
            }

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public).Where(x => !x.IsInitOnly))
            {
                _setters.Add(field.Name, (instance, value) => field.SetValue(instance, value));
            }

            if (type == typeof(string))
            {
                _constructor = Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();
            }
            else if (type.IsValueType || type.GetConstructor(Type.EmptyTypes) != null)
            {
                _constructor = Expression.Lambda<Func<T>>(Expression.New(type)).Compile();
            }
            else
            {
                _constructor = () => (T)FormatterServices.GetUninitializedObject(type);
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

            if (Table.PrimaryKey == null && (name.Equals("Id", StringComparison.OrdinalIgnoreCase) || name.Equals($"{Table.Name}Id", StringComparison.OrdinalIgnoreCase)))
            {
                Table.PrimaryKey = name;
            }

            if (type.IsValueType)
            {
                if (_integerTypes.Contains(type))
                {
                    return new Column(name, ColumnType.Integer) { Nullable = false };
                }
                else if (_realTypes.Contains(type))
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
                if (underlyingType != null)
                {
                    if (_integerTypes.Contains(underlyingType))
                    {
                        return new Column(name, ColumnType.Integer);
                    }
                    if (_realTypes.Contains(underlyingType))
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
        public void Insert(DatabaseBase database, T instance)
        {
            var command = new SQLiteCommand($"INSERT INTO {Table.Name} ({string.Join(",", Table.Columns.Select(c => c.Name))}) VALUES({string.Join(",", Table.Columns.Select(c => $"@{c.Name}"))})");
            Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", _getters[c.Name](instance)));
            database.Execute(command);
        }

        /// <inheritdoc/>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table does not have a primary key.</exception>
        public void Update(DatabaseBase database, T instance)
        {
            ThrowHelper.RequirePrimaryKey(Table);

            var command = new SQLiteCommand($"UPDATE {Table.Name} SET {string.Join(", ", Table.Columns.Select(c => $"{c.Name} = @{c.Name}"))} WHERE {Table.PrimaryKey} = @value");
            command.AddParameter("@value", _getters[Table.PrimaryKey](instance));
            Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", _getters[c.Name](instance)));
            database.Execute(command);
        }

        /// <inheritdoc/>
        public void Delete(DatabaseBase database, T instance)
        {
            if (Table.PrimaryKey != null)
            {
                var command = new SQLiteCommand($"DELETE FROM {Table.Name} WHERE {Table.PrimaryKey} = @value");
                command.AddParameter("@value", _getters[Table.PrimaryKey](instance));
                database.Execute(command);
            }
            else
            {
                var command = new SQLiteCommand($"DELETE FROM {Table.Name} WHERE {string.Join(" AND ", Table.Columns.Select(c => $"{c.Name} = @{c.Name}"))}");
                Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", _getters[c.Name](instance)));
                database.Execute(command);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> Select(DatabaseBase database) => database.Execute(new SQLiteCommand($"SELECT * FROM {Table.Name}"), reader => Map(reader));

        /// <summary>
        /// Maps a SQLiteDataReader to an object of type T.
        /// </summary>
        /// <param name="reader">SQLiteDataReader to populate object from.</param>
        /// <returns>A new instance of type T populated from the SQLiteDataReader.</returns>
        public T Map(SQLiteDataReader reader)
        {
            var instance = _constructor();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (_setters.TryGetValue(reader.GetName(i), out var setter))
                {
                    setter(instance, reader[i]);
                }
            }

            return instance;
        }
    }
}