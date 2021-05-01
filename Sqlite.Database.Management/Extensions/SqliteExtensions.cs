using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Sqlite.Database.Management.Extensions
{
    /// <summary>
    /// Contains useful extensions to System.Data.SQLite.
    /// </summary>
    public static class SqliteExtensions
    {
        private static readonly HashSet<Type> _booleanTypes = new() { typeof(bool), typeof(bool?) };

        /// <summary>
        /// Adds a parameter to the specified command.
        /// </summary>
        /// <param name="command">Command to add parameter to</param>
        /// <param name="parameterName">The name to give to the parameter</param>
        /// <param name="parameterValue">The value to assign to the parameter</param>
        /// <param name="dbType">The DbType of the parameter, defaults to DbType.String</param>
        public static void AddParameter(this SQLiteCommand command, string parameterName, object parameterValue, DbType dbType = DbType.String)
        {
            var type = parameterValue.GetType();
            if (_booleanTypes.Contains(type))
            {
                var nullableBool = parameterValue as bool?;
                command.Parameters.Add(new SQLiteParameter(parameterName, nullableBool.HasValue ? (nullableBool.Value ? 1 : 0) : new int?()) { DbType = DbType.Int32 });
            }
            else
            {
                command.Parameters.Add(new SQLiteParameter(parameterName, parameterValue) { DbType = dbType });
            }
        }
    }
}