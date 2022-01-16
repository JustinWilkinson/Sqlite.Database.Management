using Sqlite.Database.Management.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

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
        string ConnectionString { get; }

        /// <summary>
        /// Gets the Data Source for the database.
        /// </summary>
        string DataSource { get; }

        /// <summary>
        /// Gets or sets the collection of tables in the database.
        /// </summary>
        ICollection<Table> Tables { get; set; }

        /// <summary>
        /// Creates the database. Also creates any tables in the Tables collection at this time.
        /// </summary>
        void Create();

        /// <summary>
        /// Creates the database asynchronously. Also creates any tables in the Tables collection at this time.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        Task CreateAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the database.
        /// </summary>
        void Delete();

#if !NETSTANDARD2_0
        /// <summary>
        /// Deletes the database asynchronously.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        ValueTask DeleteAsync(CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// Gets a new SQLiteConnection and opens it.
        /// </summary>
        /// <returns>An open SQLiteConnection to the database.</returns>
        SQLiteConnection GetOpenConnection();

#if !NETSTANDARD2_0
        /// <summary>
        /// Gets a new SQLiteConnection and opens it asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to observe.</param>
        /// <returns>An open SQLiteConnection to the database.</returns>
        ValueTask<SQLiteConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);
#else
        /// <summary>
        /// Gets a new SQLiteConnection and opens it asynchronously.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to observe.</param>
        /// <returns>An open SQLiteConnection to the database.</returns>
        Task<SQLiteConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// Creates a <see cref="SQLiteCommand"/> from the provided string and executes it using a new connection.
        /// </summary>
        /// <param name="commandString">Command text to execute</param>
        void Execute(string commandString);

        /// <summary>
        /// Creates a <see cref="SQLiteCommand"/> from the provided string and executes it using a new connection asynchronously.
        /// </summary>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        Task ExecuteAsync(string commandString, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the provided command using a new connection.
        /// </summary>
        /// <param name="command">Command to execute</param>
        void Execute(SQLiteCommand command);

        /// <summary>
        /// Executes the provided command using a new connection asyncrhonously.
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        Task ExecuteAsync(SQLiteCommand command, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a <see cref="SQLiteCommand"/> from the provided string and executes it using a new connection, returning a scalar value using the specified converter.
        /// </summary>
        /// <typeparam name="T">Type to return the scalar as</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for returned value</param>
        /// <returns>An instance of T created by converting the returned value with the specified converter.</returns>
        T ExecuteScalar<T>(string commandString, Func<object, T> converter = null);

        /// <summary>
        /// Creates a <see cref="SQLiteCommand"/> from the provided string and executes it using a new connection, returning a scalar value using the specified converter asyncrhonously.
        /// </summary>
        /// <typeparam name="T">Type to return the scalar as</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for returned value</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>An instance of T created by converting the returned value with the specified converter.</returns>
        Task<T> ExecuteScalarAsync<T>(string commandString, Func<object, T> converter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection, returning a scalar value using the specified converter.
        /// </summary>
        /// <typeparam name="T">Type to return the scalar as</typeparam>
        /// <param name="command">Command text to execute</param>
        /// <param name="converter">Conversion method for returned value</param>
        /// <returns>An instance of T created by converting the returned value with the specified converter.</returns>
        T ExecuteScalar<T>(SQLiteCommand command, Func<object, T> converter = null);

        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection, returning a scalar value using the specified converter asyncrhonously.
        /// </summary>
        /// <typeparam name="T">Type to return the scalar as</typeparam>
        /// <param name="command">Command text to execute</param>
        /// <param name="converter">Conversion method for returned value</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>An instance of T created by converting the returned value with the specified converter.</returns>
        Task<T> ExecuteScalarAsync<T>(SQLiteCommand command, Func<object, T> converter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a <see cref="SQLiteCommand"/> from the provided string and executes it using a new connection, returning an IEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        IEnumerable<T> Execute<T>(string commandString, Func<SQLiteDataReader, T> converter);

        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection, returning an IEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        IEnumerable<T> Execute<T>(SQLiteCommand command, Func<SQLiteDataReader, T> converter);

#if NETSTANDARD2_0
        /// <summary>
        /// Creates a <see cref="SQLiteCommand"/> from the provided string and executes it using a new connection asyncrhonously, returning an IEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        Task<IEnumerable<T>> ExecuteAsync<T>(string commandString, Func<SQLiteDataReader, T> converter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection asyncrhonously, returning an IEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        Task<IEnumerable<T>> ExecuteAsync<T>(SQLiteCommand command, Func<SQLiteDataReader, T> converter, CancellationToken cancellationToken = default);
#else
        /// <summary>
        /// Creates a <see cref="SQLiteCommand"/> from the provided string and executes it using a new connection asyncrhonously, returning an IAsyncEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="commandString">Command text to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        IAsyncEnumerable<T> ExecuteAsync<T>(string commandString, Func<SQLiteDataReader, T> converter, CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes the provided SQLiteCommand using a new connection asyncrhonously, returning an IAsyncEnumerable of T generated from each row in the result set using the specified converter.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="command">Command to execute</param>
        /// <param name="converter">Conversion method for rows</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <returns>An IEnumerable of T created by converting the returned rows to T using the specified converter.</returns>
        IAsyncEnumerable<T> ExecuteAsync<T>(SQLiteCommand command, Func<SQLiteDataReader, T> converter, CancellationToken cancellationToken = default);
#endif

        /// <summary>
        /// Wraps an action in a transaction which is committed on success, or rolled back on error - note that no additional connections should be opened within this to prevent deadlocks.
        /// This will throw any exceptions that occur whilst executing the action.
        /// </summary>
        /// <param name="action">Action to run in transaction.</param>
        /// <param name="isolationLevel">Isolation Level of the transaction, defaults to Serializable.</param>
        void ExecuteInTransaction(Action<SQLiteConnection> action, IsolationLevel isolationLevel = IsolationLevel.Serializable);

        /// <summary>
        /// Wraps an asyncrhonous action in a transaction which is committed on success, or rolled back on error - note that no additional connections should be opened within this to prevent deadlocks.
        /// This will throw any exceptions that occur whilst executing the action.
        /// </summary>
        /// <param name="action">Action to run in transaction.</param>
        /// <param name="isolationLevel">Isolation Level of the transaction, defaults to Serializable.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        Task ExecuteInTransactionAsync(Func<SQLiteConnection, Task> action, IsolationLevel isolationLevel = IsolationLevel.Serializable, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves the value from the named column, and converts it to an instance of T using the specified function.
        /// </summary>
        /// <typeparam name="T">Type to convert to</typeparam>
        /// <param name="columnName">Name of the column</param>
        /// <param name="converter">Conversion function, uses basic cast if null or not provided.</param>
        /// <returns>Value from named column as type T</returns>
        Func<SQLiteDataReader, T> GetColumnValue<T>(string columnName, Func<object, T> converter = null);

        /// <summary>
        /// Creates a table in the database.
        /// </summary>
        /// <param name="table">Table to create.</param>
        /// <param name="createIfNotExists">Include an IF NOT EXISTS clause in the create statement, defults to true.</param>
        void Create(Table table, bool createIfNotExists = true);

        /// <summary>
        /// Creates a table in the database asyncrhonously.
        /// </summary>
        /// <param name="table">Table to create.</param>
        /// <param name="createIfNotExists">Include an IF NOT EXISTS clause in the create statement, defults to true.</param>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        Task CreateAsync(Table table, bool createIfNotExists = true, CancellationToken cancellationToken = default);
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
            if (Tables is not null && Tables.Count > 0)
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
        public virtual async Task CreateAsync(CancellationToken cancellationToken = default)
        {
            if (Tables is not null && Tables.Count > 0)
            {
                var createTasks = new List<Task>();
                var tablesCreated = new HashSet<string>();
                foreach (var table in Tables)
                {
                    var loweredName = table.Name.ToLower();
                    if (!tablesCreated.Contains(loweredName))
                    {
                        createTasks.Add(CreateAsync(table, cancellationToken: cancellationToken));
                        tablesCreated.Add(loweredName);
                    }
                    else
                    {
                        throw new DuplicateTableException(table.Name, DataSource);
                    }
                }

                await Task.WhenAll(createTasks).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        public abstract void Delete();

#if !NETSTANDARD2_0
        /// <inheritdoc/>
        public abstract ValueTask DeleteAsync(CancellationToken cancellationToken = default);
#endif

        /// <inheritdoc/>
        public virtual SQLiteConnection GetOpenConnection()
        {
            var connection = new SQLiteConnection(ConnectionString);
            connection.Open();
            return connection;
        }

#if!NETSTANDARD2_0
        /// <inheritdoc/>
        public virtual async ValueTask<SQLiteConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SQLiteConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
#else
        /// <inheritdoc/>
        public virtual async Task<SQLiteConnection> GetOpenConnectionAsync(CancellationToken cancellationToken = default)
        {
            var connection = new SQLiteConnection(ConnectionString);
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return connection;
        }
#endif

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public void Execute(string commandString) => Execute(new SQLiteCommand(commandString));

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public Task ExecuteAsync(string commandString, CancellationToken cancellationToken = default) => ExecuteAsync(new SQLiteCommand(commandString), cancellationToken);

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
        public async Task ExecuteAsync(SQLiteCommand command, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                command.Connection = connection;
                await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                await CleanUpAsync(connection).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public T ExecuteScalar<T>(string commandString, Func<object, T> converter = null) => ExecuteScalar(new SQLiteCommand(commandString), converter);

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public Task<T> ExecuteScalarAsync<T>(string commandString, Func<object, T> converter = null, CancellationToken cancellationToken = default)
            => ExecuteScalarAsync(new SQLiteCommand(commandString), converter, cancellationToken);

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
                return converter is not null ? converter(scalar) : (T)scalar;
            }
            finally
            {
                CleanUp(connection);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command is null.</exception>
        public async Task<T> ExecuteScalarAsync<T>(SQLiteCommand command, Func<object, T> converter = null, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                command.Connection = connection;
                var scalar = await command.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
                return converter is not null ? converter(scalar) : (T)scalar;
            }
            finally
            {
                await CleanUpAsync(connection).ConfigureAwait(false);
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

#if NETSTANDARD2_0
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        public Task<IEnumerable<T>> ExecuteAsync<T>(string commandString, Func<SQLiteDataReader, T> converter, CancellationToken cancellationToken = default)
            => ExecuteAsync(new SQLiteCommand(commandString), converter, cancellationToken);

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        public async Task<IEnumerable<T>> ExecuteAsync<T>(SQLiteCommand command, Func<SQLiteDataReader, T> converter, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            ThrowHelper.ThrowIfArgumentNull(converter);
            var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var results = new List<T>();
                command.Connection = connection;
                using var reader = command.ExecuteReader();
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    results.Add(converter(reader));
                }

                return results;
            }
            finally
            {
                await CleanUpAsync(connection).ConfigureAwait(false);
            }
        }
#else
        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        public IAsyncEnumerable<T> ExecuteAsync<T>(string commandString, Func<SQLiteDataReader, T> converter, CancellationToken cancellationToken = default)
            => ExecuteAsync(new SQLiteCommand(commandString), converter, cancellationToken);

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown when command or converter is null.</exception>
        public async IAsyncEnumerable<T> ExecuteAsync<T>(SQLiteCommand command, Func<SQLiteDataReader, T> converter, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfArgumentNull(command);
            ThrowHelper.ThrowIfArgumentNull(converter);
            var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                command.Connection = connection;
                using var reader = command.ExecuteReader(); // ExecuteReaderAsync() returns a DbDataReader rather than a SQLiteDataReader.
                while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    yield return converter(reader);
                }
            }
            finally
            {
                await CleanUpAsync(connection).ConfigureAwait(false);
            }
        }
#endif

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
        /// <exception cref="ArgumentNullException">Thrown when action is null.</exception>
        public async Task ExecuteInTransactionAsync(Func<SQLiteConnection, Task> action, IsolationLevel isolationLevel = IsolationLevel.Serializable, CancellationToken cancellationToken = default)
        {
            ThrowHelper.ThrowIfArgumentNull(action);

            var connection = await GetOpenConnectionAsync(cancellationToken).ConfigureAwait(false);
#if NETSTANDARD2_0
            var transaction = connection.BeginTransaction(isolationLevel);
#else
            var transaction = await connection.BeginTransactionAsync(isolationLevel, cancellationToken).ConfigureAwait(false);
#endif
            try
            {
                await action(connection).ConfigureAwait(false);

#if NETSTANDARD2_0
                transaction.Commit();
#else
                await transaction.CommitAsync(cancellationToken).ConfigureAwait(false);
#endif
            }
            catch
            {
#if NETSTANDARD2_0
                transaction.Rollback();
#else
                await transaction.RollbackAsync(cancellationToken).ConfigureAwait(false);
#endif
                throw;
            }
            finally
            {
                await CleanUpAsync(connection).ConfigureAwait(false);
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown if columnName is null</exception>
        /// <exception cref="ArgumentException">Thrown if columnName is all whitespace</exception>
        public Func<SQLiteDataReader, T> GetColumnValue<T>(string columnName, Func<object, T> converter = null)
        {
            ThrowHelper.ThrowIfArgumentNullOrWhitespace(columnName);
            return reader => converter is not null ? converter(reader[columnName]) : (T)reader[columnName];
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown if the table is null</exception>
        public void Create(Table table, bool createIfNotExists = true)
        {
            ThrowHelper.InvalidIfNull(table);
            Execute(table.GetCreateStatement(createIfNotExists));
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">Thrown if the table is null</exception>
        public Task CreateAsync(Table table, bool createIfNotExists = true, CancellationToken cancellationToken = default)
        {
            ThrowHelper.InvalidIfNull(table);
            return ExecuteAsync(table.GetCreateStatement(createIfNotExists), cancellationToken);
        }

        private void CleanUp(SQLiteConnection connection)
        {
            if (_closeConnections)
            {
                connection.Dispose();
            }
        }

#if NETSTANDARD2_0
        private Task CleanUpAsync(SQLiteConnection connection)
        {
            if (_closeConnections)
            {
                connection.Dispose();
            }

            return Task.CompletedTask;
        }
#else
        private ValueTask CleanUpAsync(SQLiteConnection connection)
        {
            if (_closeConnections)
            {
                return connection.DisposeAsync();
            }

#if NETSTANDARD2_1
            return new ValueTask();
#else
            return ValueTask.CompletedTask;
#endif
        }
#endif
    }
}