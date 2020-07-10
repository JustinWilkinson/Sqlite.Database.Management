using Sqlite.Database.Management.Enumerations;
using Sqlite.Database.Management.Extensions;
using System;
using System.Collections.Generic;
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
            
            if (type.IsValueType)
            {
                if (_integerTypes.Contains(type))
                {
                    return new Column(property.Name, ColumnType.Integer) { Nullable = false };
                }
                else if (_realTypes.Contains(type))
                {
                    return new Column(property.Name, ColumnType.Real) { Nullable = false };
                }
                else if (type == typeof(bool))
                {
                    return new Column(property.Name, ColumnType.Integer) { Nullable = false, CheckExpression = "IN (0, 1)" };
                }
            }
            else
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                if (underlyingType != null)
                {
                    if (_integerTypes.Contains(underlyingType))
                    {
                        return new Column(property.Name, ColumnType.Integer);
                    }
                    if (_realTypes.Contains(underlyingType))
                    {
                        return new Column(property.Name, ColumnType.Real);
                    }
                    else if (underlyingType == typeof(bool))
                    {
                        return new Column(property.Name, ColumnType.Integer) { CheckExpression = "IN (0, 1, NULL)" };
                    }
                }
            }

            if (type.IsAssignableFrom(typeof(IEnumerable<byte>)))
            {
                return new Column(property.Name, ColumnType.Blob);
            }

            return new Column(property.Name);
        }
    }
}