using Sqlite.Database.Management.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Maps a row in a table to an object of T.
    /// </summary>
    /// <typeparam name="T">Type of object to map to.</typeparam>
    internal interface IRowMapper<T>
    {
        /// <summary>
        /// Maps a SQLiteDataReader to an object of type T.
        /// </summary>
        /// <param name="reader">SQLiteDataReader to populate object from.</param>
        /// <returns>A new instance of type T populated from the SQLiteDataReader.</returns>
        public T Map(SQLiteDataReader reader);
    }

    /// <summary>
    /// Maps a row in a table to an object of T.
    /// </summary>
    /// <typeparam name="T">Type of object to map to.</typeparam>
    public class RowMapper<T> : IRowMapper<T>
    {
        private static readonly Dictionary<string, Action<T, object>> _setters = new Dictionary<string, Action<T, object>>();
        private static readonly Func<T> _constructor;

        /// <summary>
        /// Constructs a row mapper and caches a function.
        /// </summary>
        static RowMapper()
        {
            Type type = typeof(T);

            foreach (var property in type.GetPublicInstanceProperties())
            {
                _setters.Add(property.Name, property.GetSetter<T>());
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

        /// <inheritdoc/>
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