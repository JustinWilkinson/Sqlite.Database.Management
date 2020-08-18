using Sqlite.Database.Management.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Defines basic methods/properties common to databases.
    /// Implemented by the <see cref="Database"/> and <see cref="InMemoryDatabase"/> classes.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the Data Source for the database.
        /// </summary>
        public string DataSource { get; }

        /// <summary>
        /// Gets or sets the collection of tables in the database.
        /// </summary>
        public ICollection<Table> Tables { get; set; }

        /// <summary>
        /// Creates the database. Should also creates any tables in the Tables collection at this time.
        /// </summary>
        public void Create();

        /// <summary>
        /// Deletes the database.
        /// </summary>
        public void Delete();

        /// <summary>
        /// Gets a new SQLiteConnection and opens it.
        /// </summary>
        /// <returns>An open SQLiteConnection to the database.</returns>
        public SQLiteConnection GetOpenConnection();

        /// <summary>
        /// Creates a SQLiteCommand from the provided string and executes it using a new connection.
        /// </summary>
        /// <param name="commandString">Command text to execute</param>
        public void Execute(string commandString);

        /// <summary>
        /// Executes the provided command using a new connection.
        /// </summary>
        /// <param name="command">Command to execute</param>
        public void Execute(SQLiteCommand command);

        /// <summary>
        /// Creates a SQLiteCommand from the provided string and executes it using a new connection, returning a scalar value using the specified converter.
        /// </summary>
        /// <typeparam name="T">Type to return the scalar as</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for returned value</param>
        /// <returns>An instance of T created by converting the returned value with the specified converter.</returns>
        public T ExecuteScalar<T>(string commandString, Func<object, T> converter = null);

        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection, returning a scalar value using the specified converter.
        /// </summary>
        /// <typeparam name="T">Type to return the scalar as</typeparam>
        /// <param name="command">Command text to execute</param>
        /// <param name="converter">Conversion method for returned value</param>
        /// <returns>An instance of T created by converting the returned value with the specified converter.</returns>
        public T ExecuteScalar<T>(SQLiteCommand command, Func<object, T> converter = null);

        /// <summary>
        /// Creates a SQLiteCommand from the provided string and executes it using a new connection, returning an IEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        public IEnumerable<T> Execute<T>(string commandString, Func<SQLiteDataReader, T> converter);

        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection, returning an IEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="command">Command to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        public IEnumerable<T> Execute<T>(SQLiteCommand command, Func<SQLiteDataReader, T> converter);

        /// <summary>
        /// Wraps an action in a transaction which is committed on success, or rolled back on error - note that no additional connections should be opened within this to prevent deadlocks.
        /// This will throw any exceptions that occur whilst executing the action.
        /// </summary>
        /// <param name="action">Action to run in transaction.</param>
        /// <param name="isolationLevel">Isolation Level of the transaction, defaults to Serializable.</param>
        public void ExecuteInTransaction(Action<SQLiteConnection> action, IsolationLevel isolationLevel = IsolationLevel.Serializable);

        /// <summary>
        /// Retrieves the value from the named column, and converts it to an instance of T using the specified function.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="columnName">Name of the column</param>
        /// <param name="converter">Conversion function, uses basic cast if null or not provided.</param>
        /// <returns>Value from named column as type T</returns>
        public Func<SQLiteDataReader, T> GetColumnValue<T>(string columnName, Func<object, T> converter = null);

        /// <summary>
        /// Creates a table in the database.
        /// </summary>
        /// <param name="table">Table to create.</param>
        /// <param name="createIfNotExists">Include an IF NOT EXISTS clause in the create statement, defults to true.</param>
        public void Create(Table table, bool createIfNotExists = true);
    }

    /// <summary>
    /// Defines the base class for databases.
    /// <see cref="Database"/> and <see cref="InMemoryDatabase"/> both inherit from this class.
    /// </summary>
    public abstract class DatabaseBase : IDatabase
    {
        private readonly bool _closeConnections;

        /// <inheritdoc/>
        public string ConnectionString { get; }

        /// <inheritdoc/>
        public string DataSource { get; }

        /// <inheritdoc/>
        public ICollection<Table> Tables { get; set; }

        /// <summary>
        /// Creates a new instance of a Database and assigns it the provided connection string, also initialises an empty collection of tables.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <exception cref="ArgumentNullException">Thrown if connectionString is null</exception>
        /// <exception cref="ArgumentException">Thrown if connectionString is all whitespace</exception>
        protected DatabaseBase(string connectionString)
        {
            ThrowHelper.ThrowIfArgumentNullOrWhitespace(connectionString);
            ConnectionString = connectionString;
            DataSource = new SQLiteConnectionStringBuilder(connectionString).DataSource;
            Tables = new List<Table>();
            _closeConnections = this is Database || (this is InMemoryDatabase inMemory && inMemory.IsShareable);
        }

        /// <inheritdoc/>
        public virtual void Create()
        {
            if (Tables != null && Tables.Count > 0)
            {
                var tablesCreated = new HashSet<string>();
                foreach (var table in Tables)
                {
                    var loweredName = table.Name.ToLower();
                    if (!tablesCreated.Contains(loweredName))
                    {
                        Create(table);
                        tablesCreated.Add(loweredName);
                    }
                    else
                    {
                        throw new DuplicateTableException(table.Name, DataSource);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public abstract void Delete();

        /// <inheritdoc/>
        public virtual SQLiteConnection GetOpenConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public void Execute(string commandString) => Execute(new SQLiteCommand(commandString));


        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public void Execute(SQLiteCommand command)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            var connection = GetOpenConnection();
            try
            {
                command.Connection = connection;
                command.ExecuteNonQuery();
            }
            finally
            {
                CleanUp(connection);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public T ExecuteScalar<T>(string commandString, Func<object, T> converter = null) => ExecuteScalar(new SQLiteCommand(commandString), converter);

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public T ExecuteScalar<T>(SQLiteCommand command, Func<object, T> converter = null)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            var connection = GetOpenConnection();
            try
            {
                command.Connection = connection;
                var scalar = command.ExecuteScalar();
                return converter != null ? converter(scalar) : (T)scalar;
            }
            finally
            {
                CleanUp(connection);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        public IEnumerable<T> Execute<T>(string commandString, Func<SQLiteDataReader, T> converter) => Execute(new SQLiteCommand(commandString), converter);


        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        public IEnumerable<T> Execute<T>(SQLiteCommand command, Func<SQLiteDataReader, T> converter)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            ThrowHelper.ThrowIfArgumentNull(converter);
            var connection = GetOpenConnection();
            try
            {
                command.Connection = connection;
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    yield return converter(reader);
                }
            }
            finally
            {
                CleanUp(command.Connection);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        public void ExecuteInTransaction(Action<SQLiteConnection> action, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            ThrowHelper.ThrowIfArgumentNull(action);

            var connection = GetOpenConnection();
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
            finally
            {
                CleanUp(connection);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown if columnName is null</exception>
        /// <exception cref="ArgumentException">Thrown if columnName is all whitespace</exception>
        public Func<SQLiteDataReader, T> GetColumnValue<T>(string columnName, Func<object, T> converter = null)
        {
            ThrowHelper.ThrowIfArgumentNullOrWhitespace(columnName);
            return reader => converter != null ? converter(reader[columnName]) : (T)reader[columnName];
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown if the table is null</exception>
        public void Create(Table table, bool createIfNotExists = true)
        {
            ThrowHelper.InvalidIfNull(table);
            Execute(table.GetCreateStatement(createIfNotExists));
        }

        private void CleanUp(SQLiteConnection connection)
        {
            if (_closeConnections)
            {
                connection.Dispose();
            }
        }
    }
}