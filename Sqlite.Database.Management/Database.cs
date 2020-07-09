using Sqlite.Database.Management.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Represents a SQLite Database.
    /// </summary>
    public class Database
    {
        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets or sets the collection of tables in the database.
        /// </summary>
        public ICollection<Table> Tables { get; set; }

        /// <summary>
        /// Creates a new instance of a Database and assigns it the provided connection string, also initialises an empty collection of tables.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <exception cref="ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="ArgumentException">Thrown if connectionString is all whitespace</exception>
        public Database(string connectionString)
        {
            ThrowHelper.ThrowIfArgumentNullOrWhitespace(connectionString);
            ConnectionString = connectionString;
            Tables = new List<Table>();
        }

        /// <summary>
        /// Creates the database if it doesn't exist, and stores the connection string in memory.
        /// Also creates any tables in the Tables collection at this time.
        /// </summary>
        /// <exception cref="DuplicateTableException">Thrown if more than one table of the same name is provided.</exception>
        public void Create()
        {
            var path = new SQLiteConnectionStringBuilder(ConnectionString).DataSource;
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }

            if (Tables != null && Tables.Count > 0)
            {
                var tablesCreated = new HashSet<string>();
                foreach (var table in Tables)
                {
                    try
                    {
                        var loweredName = table.Name.ToLower();
                        if (!tablesCreated.Contains(loweredName))
                        {
                            Create(table);
                            tablesCreated.Add(loweredName);
                        }
                        else
                        {
                            throw new DuplicateTableException(table.Name, path);
                        }
                    }
                    catch
                    {
                        File.Delete(path);
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Returns an open SQLiteConnection.
        /// </summary>
        public SQLiteConnection GetOpenConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <summary>
        /// Creates a SQLiteCommand from the provided string and executes it using a new connection.
        /// </summary>
        /// <param name="commandString">Command text to execute</param>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public void Execute(string commandString) => Execute(new SQLiteCommand(commandString));


        /// <summary>
        /// Executes the provided command using a new connection.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public void Execute(SQLiteCommand command)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            using var connection = GetOpenConnection();
            command.Connection = connection;
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Creates a SQLiteCommand from the provided string and executes it using a new connection, returning a scalar value using the specified converter.
        /// </summary>
        /// <typeparam name="T">Type to return the scalar as</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for returned value</param>
        /// <returns>An instance of T created by converting the returned value with the specified converter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public T ExecuteScalar<T>(string commandString, Func<object, T> converter = null) => ExecuteScalar(new SQLiteCommand(commandString), converter);

        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection, returning a scalar value using the specified converter.
        /// </summary>
        /// <typeparam name="T">Type to return the scalar as</typeparam>
        /// <param name="command">Command text to execute</param>
        /// <param name="converter">Conversion method for returned value</param>
        /// <returns>An instance of T created by converting the returned value with the specified converter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public T ExecuteScalar<T>(SQLiteCommand command, Func<object, T> converter = null)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            using var connection = GetOpenConnection();
            command.Connection = connection;
            var scalar = command.ExecuteScalar();
            return converter != null ? converter(scalar) : (T)scalar;
        }

        /// <summary>
        /// Creates a SQLiteCommand from the provided string and executes it using a new connection, returning an IEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        public IEnumerable<T> Execute<T>(string commandString, Func<SQLiteDataReader, T> converter) => Execute(new SQLiteCommand(commandString), converter);


        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection, returning an IEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="command">Command to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        public IEnumerable<T> Execute<T>(SQLiteCommand command, Func<SQLiteDataReader, T> converter)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            ThrowHelper.ThrowIfArgumentNull(converter);

            using var connection = GetOpenConnection();
            command.Connection = connection;
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                yield return converter(reader);
            }
        }

        /// <summary>
        /// Wraps an action in a transaction which is committed on success, or rolled back on error - note that no additional connections should be opened within this to prevent deadlocks.
        /// </summary>
        /// <param name="action">Action to run in transaction</param>
        /// <param name="isolationLevel">Isolation Level of the transaction, defaults to Serializable</param>
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        public void ExecuteInTransaction(Action<SQLiteConnection> action, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            ThrowHelper.ThrowIfArgumentNull(action);
            using var connection = GetOpenConnection();
            var transaction = connection.BeginTransaction(isolationLevel);
            try
            {
                action(connection);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Retrieves the value from the named column, and converts it to an instance of T using the specified function.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="columnName">Name of the column</param>
        /// <param name="converter">Conversion function, uses basic cast if null or not provided.</param>
        /// <returns>Value from named column as type T</returns>
        /// <exception cref="ArgumentNullException">Thrown if columnName is null</exception>
        /// <exception cref="ArgumentException">Thrown if columnName is all whitespace</exception>
        public Func<SQLiteDataReader, T> GetColumnValue<T>(string columnName, Func<object, T> converter = null)
        {
            ThrowHelper.ThrowIfArgumentNullOrWhitespace(columnName);
            return reader => converter != null ? converter(reader[columnName]) : (T)reader[columnName];
        }

        /// <summary>
        /// Creates a table in the database.
        /// </summary>
        /// <param name="table">Table to create.</param>
        /// <param name="createIfNotExists">Include an IF NOT EXISTS clause in the create statement, defults to true.</param>
        /// <exception cref="ArgumentNullException">Thrown if the table is null</exception>
        public void Create(Table table, bool createIfNotExists = true)
        {
            ThrowHelper.InvalidIfNull(table);
            Execute(table.GetCreateStatement(createIfNotExists));
        }
    }
}