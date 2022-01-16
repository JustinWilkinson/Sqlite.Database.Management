using Sqlite.Database.Management.Exceptions;
using System;
using System.Data.SQLite;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Represents a SQLite Database (in a file), use <see cref="InMemoryDatabase"/> for in memory databases.
    /// </summary>
    public class Database : DatabaseBase
    {
        /// <inheritdoc/>
        public Database(string connectionString) : base(connectionString)
        {
        }

        /// <summary>
        /// Creates the database file if it doesn't exist. Also creates any tables in the Tables collection at this time.
        /// If an exception is thrown the database is deleted.
        /// </summary>
        /// <exception cref="DuplicateTableException">Thrown if more than one table of the same name is provided.</exception>
        public override void Create()
        {
            try
            {
                if (!File.Exists(DataSource))
                {
                    SQLiteConnection.CreateFile(DataSource);
                }

                base.Create();
            }
            catch
            {
                File.Delete(DataSource);
                throw;
            }
        }

        /// <summary>
        /// Deletes the database file.
        /// </summary>
        /// <exception cref="ArgumentException">path is a zero-length string, contains only white space, or contains one or more invalid characters as defined by System.IO.Path.InvalidPathChars.</exception>
        /// <exception cref="ArgumentNullException">path is null.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid.</exception>
        /// <exception cref="IOException">The specified file is in use.</exception>
        /// <exception cref="NotSupportedException">path is in an invalid format.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="UnauthorizedAccessException">Cannot access the required file.</exception>
        public override void Delete()
        {
            if (File.Exists(DataSource))
            {
                File.Delete(DataSource);
            }
        }

#if !NETSTANDARD2_0
        /// <summary>
        /// Deletes the database file.
        /// </summary>
        /// <param name="cancellationToken">CancellationToken to observe.</param>
        /// <exception cref="ArgumentException">path is a zero-length string, contains only white space, or contains one or more invalid characters as defined by System.IO.Path.InvalidPathChars.</exception>
        /// <exception cref="ArgumentNullException">path is null.</exception>
        /// <exception cref="DirectoryNotFoundException">The specified path is invalid.</exception>
        /// <exception cref="IOException">The specified file is in use.</exception>
        /// <exception cref="NotSupportedException">path is in an invalid format.</exception>
        /// <exception cref="PathTooLongException">The specified path, file name, or both exceed the system-defined maximum length.</exception>
        /// <exception cref="UnauthorizedAccessException">Cannot access the required file.</exception>
        public override ValueTask DeleteAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (File.Exists(DataSource))
            {
                cancellationToken.ThrowIfCancellationRequested();
                File.Delete(DataSource);
            }

#if NET5_0_OR_GREATER
            return ValueTask.CompletedTask;
#else
            return new ValueTask();
#endif
        }
#endif
    }
}