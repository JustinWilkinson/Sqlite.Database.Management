using Sqlite.Database.Management.Exceptions;
using System;
using System.Data.SQLite;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Represents an in memory database, for file persisted databases, use <see cref="Database"/>.
    /// </summary>
    public class InMemoryDatabase : DatabaseBase, IDisposable
    {
        /// <summary>
        /// The master connection of the database, when this is closed the databsae will be deleted.
        /// </summary>
        private SQLiteConnection _masterConnection;

        /// <summary>
        /// The name of the database.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Determines if the database can be shared between connections.
        /// </summary>
        public bool IsShareable { get; }

        /// <summary>
        /// Instantiates a new in memory database with the provided name that cannot be shared between connections.
        /// </summary>
        public InMemoryDatabase() : base("DataSource=:memory:")
        {
            Name = ":memory:";
            IsShareable = false;
        }

        /// <summary>
        /// Instantiates a new shareable in memory database with the provided name.
        /// </summary>
        /// <param name="name">Name of the database, if not provided the database will use :memory:</param>
        /// <exception cref="ArgumentNullException">Thrown if name is null.</exception>
        /// <exception cref="ArgumentException">Thrown if name is whitsepace.</exception>
        public InMemoryDatabase(string name) : base($"DataSource={name};Mode=Memory;Cache=Shared;")
        {
            ThrowHelper.ThrowIfArgumentNullOrWhitespace(name);
            Name = name;
            IsShareable = true;
        }

        /// <summary>
        /// Creates the in memory database. Also creates any tables in the Tables collection at this time.
        /// </summary>
        public override void Create()
        {
            _masterConnection = new SQLiteConnection(ConnectionString);
            _masterConnection.Open();
            base.Create();
        }

        /// <summary>
        /// Deletes the in memory database.
        /// </summary>
        public override void Delete() => _masterConnection.Dispose();

        /// <summary>
        /// If the database is shareable, this method gets a new SQLiteConnection and opens it, if not it returns the master connection.
        /// Note that if the master connection is closed, the database will be deleted.
        /// </summary>
        /// <returns>An open connection to the database.</returns>
        public override SQLiteConnection GetOpenConnection() => IsShareable ? base.GetOpenConnection() : _masterConnection;

        public void Dispose()
        {
            Delete();
            GC.SuppressFinalize(this);
        }
    }
}
