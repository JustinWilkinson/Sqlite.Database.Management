using Sqlite.Database.Management.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Represents a collection of databases with the same schema.
    /// </summary>
    public class DatabaseCollection : IEnumerable<Database>
    {
        /// <summary>
        /// Internal list of databases.
        /// </summary>
        private readonly Dictionary<string, Database> _databases;

        /// <summary>
        /// Instantiates and creates a new DatabaseCollection with the (distinct) connectionStrings and tables.
        /// </summary>
        /// <param name="connectionStrings">Non-empty enumerable of connection strings.</param>
        /// <param name="tables">Tables to assign to each.</param>
        /// <exception cref="ArgumentNullException">Thrown if connectionStrings or tables is null, or if any connection string is empty.</exception>
        /// <exception cref="ArgumentException">Thrown if connectionStrings or tables is empty, or if any connection string is all whitespace.</exception>
        public DatabaseCollection(IEnumerable<string> connectionStrings, ICollection<Table> tables)
        {
            ThrowHelper.ThrowIfArgumentNullOrEmpty(connectionStrings);
            ThrowHelper.ThrowIfArgumentNullOrEmpty(tables);

            foreach (var connectionString in connectionStrings.Distinct())
            {
                var database = new Database(connectionString) { Tables = tables };
                database.Create();
                _databases.Add(connectionString, database);
            }
        }

        /// <summary>
        /// Returns the database for the provided connection string, or null if not present.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>The database for the provided connection string, or null if not present</returns>
        public Database this[string connectionString] => _databases.TryGetValue(connectionString, out Database database) ? database : null;

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>An enumerator over the collection.</returns>
        public IEnumerator<Database> GetEnumerator() => _databases.Values.GetEnumerator();

        /// <summary>
        /// Gets an enumerator for the collection.
        /// </summary>
        /// <returns>An enumerator over the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}