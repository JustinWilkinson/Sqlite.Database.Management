using System;

namespace Sqlite.Database.Management.Exceptions
{
    /// <summary>
    /// Thrown when a table has more than one Table of the same name (case-insensitve).
    /// </summary>
    public class DuplicateTableException : Exception
    {
        /// <summary>
        /// A new DuplicateTableException instance with a basic message. 
        /// </summary>
        public DuplicateTableException() : base("Duplicate table name found in database!")
        {

        }

        /// <summary>
        /// A new DuplicateTableException instance with a message containing the table and database names.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="databaseName">The name of the database.</param>
        public DuplicateTableException(string tableName, string databaseName) : base($"Duplicate table name '{tableName}' found in {databaseName}!")
        {

        }
    }
}