using System;

namespace Sqlite.Database.Management.Exceptions
{
    /// <summary>
    /// Thrown when a table has more than one Table of the same name (case-insensitve).
    /// </summary>
    public class PrimaryKeyMissingException : Exception
    {
        /// <summary>
        /// A new PrimaryKeyMissingException instance with a basic message. 
        /// </summary>
        public PrimaryKeyMissingException() : base("Primary key for table not found in database!")
        {
        }

        /// <summary>
        /// A new PrimaryKeyMissingException instance with a message containing the table name.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public PrimaryKeyMissingException(Table table) : base($"Primary key for table '{table.Name}' not found in database!")
        {
        }

        /// <summary>
        /// A new PrimaryKeyMissingException instance with a message containing the table name.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public PrimaryKeyMissingException(string tableName) : base($"Primary key for table '{tableName}' not found in database!")
        {
        }

        /// <summary>
        /// A new PrimaryKeyMissingException instance with a message containing the table and database names.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="databaseName">The name of the database.</param>
        public PrimaryKeyMissingException(string tableName, string databaseName) : base($"Primary key for table '{tableName}' not found in {databaseName}!")
        {
        }
    }
}