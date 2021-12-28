using System;

namespace Sqlite.Database.Management.Exceptions
{
    /// <summary>
    /// Thrown when a table has more than one column of the same name (case-insensitve).
    /// </summary>
    public class DuplicateColumnException : Exception
    {
        /// <summary>
        /// A new DuplicateColumnException instance with a basic message. 
        /// </summary>
        public DuplicateColumnException() : base("Duplicate column found in table!")
        {
        }

        /// <summary>
        /// A new DuplicateColumnException instance with a message containing the table and column names.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="columnName">The name of the column.</param>
        public DuplicateColumnException(string tableName, string columnName) : base($"Duplicate column '{columnName}' found in {tableName}!")
        {
        }
    }
}