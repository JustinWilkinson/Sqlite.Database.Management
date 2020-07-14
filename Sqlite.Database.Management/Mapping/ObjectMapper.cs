using Sqlite.Database.Management.Enumerations;
using Sqlite.Database.Management.Exceptions;
using Sqlite.Database.Management.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;

namespace Sqlite.Database.Management.Mapping
{
    /// <summary>
    /// Maps an object of type T to a SQLite table.
    /// </summary>
    /// <typeparam name="T">Type of object to map to.</typeparam>
    public interface IObjectMapper<T>
    {

    }

    /// <summary>
    /// Maps an object of type T to a SQLite table.
    /// </summary>
    /// <typeparam name="T">Type of object to map to.</typeparam>
    public class ObjectMapper<T> : IObjectMapper<T>
    {
        #region Static Readonly
        private static readonly Dictionary<string, Func<T, object>> _getters = new Dictionary<string, Func<T, object>>();

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
            Table table = new Table(nameof(T)) { Columns = new List<Column>() };

            foreach (var property in typeof(T).GetPublicInstanceProperties())
            {
                _getters.Add(property.Name, (Func<T, object>)Delegate.CreateDelegate(typeof(Func<T, object>), property.GetMethod));
                table.Columns.Add(GetColumn(property));
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

        /// <summary>
        /// Gets an insert statement for an instance of T.
        /// </summary>
        /// <param name="instance">Instance to insert.</param>
        /// <returns>A SQLiteCommand which when executed will insert the instance to the database.</returns>
        public SQLiteCommand GetInsertStatement(T instance)
        {
            var command = new SQLiteCommand($"INSERT INTO {Table.Name} ({string.Join(",", Table.Columns.Select(c => c.Name))}) VALUES({string.Join(",", Table.Columns.Select(c => $"@{c.Name}"))})");
            Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", _getters[c.Name](instance)));
            return command;
        }

        /// <summary>
        /// Gets an update statement for an instance of T. This method requires that a primary key exists on the table.
        /// </summary>
        /// <param name="instance">Instance to update.</param>
        /// <returns>A SQLiteCommand which when executed will update the instance in the database.</returns>
        /// <exception cref="PrimaryKeyMissingException">Thrown when the table does not have a primary key.</exception>
        public SQLiteCommand GetUpdateStatement(T instance)
        {
            ThrowHelper.RequirePrimaryKey(Table);

            var command = new SQLiteCommand($"UPDATE {Table.Name} SET {string.Join(", ", Table.Columns.Select(c => $"{c.Name} = @{c.Name}"))} WHERE {Table.PrimaryKey} = @value");
            command.AddParameter("@value", _getters[Table.PrimaryKey](instance));
            Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", _getters[c.Name](instance)));
            return command;
        }

        /// <summary>
        /// Gets a delete statement for an instance of T.
        /// </summary>
        /// <param name="instance">Instance to delete.</param>
        /// <returns>A SQLiteCommand which when executed will delete the instance from the database.</returns>
        public SQLiteCommand GetDeleteStatement(T instance)
        {
            if (Table.PrimaryKey != null)
            {
                var command = new SQLiteCommand($"DELETE FROM {Table.Name} WHERE {Table.PrimaryKey} = @value");
                command.AddParameter("@value", _getters[Table.PrimaryKey](instance));
                return command;
            }
            else
            {
                var command = new SQLiteCommand($"DELETE FROM {Table.Name} WHERE {string.Join(" AND ", Table.Columns.Select(c => $"{c.Name} = @{c.Name}"))}");
                Table.Columns.ForEach(c => command.AddParameter($"@{c.Name}", _getters[c.Name](instance)));
                return command;
            }
        }
    }
}