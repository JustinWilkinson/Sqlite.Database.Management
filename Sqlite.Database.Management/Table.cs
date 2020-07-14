using Sqlite.Database.Management.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Represents a database table.
    /// </summary>
    public class Table
    {
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the collection of Columns in the table.
        /// </summary>
        public ICollection<Column> Columns { get; set; }

        /// <summary>
        /// The name of the column that is the primary key.
        /// Note that if this is not provided, but a column has name Id or {TableName}Id then these will be automatically used as primary key.
        /// </summary>
        public string PrimaryKey { get; set; }

        /// <summary>
        /// Constructs a new, unconfigured instance of a Table.
        /// </summary>
        public Table()
        {

        }

        /// <summary>
        /// Creates a new instance of a Table and assigns it the provided name.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <exception cref="ArgumentNullException">Thrown if name is null</exception>
        /// <exception cref="ArgumentException">Thrown if name is all whitespace</exception>
        public Table(string name)
        {
            ThrowHelper.ThrowIfArgumentNullOrWhitespace(name);
            Name = name;
        }

        /// <summary>
        /// Gets a create statement for the table.
        /// </summary>
        /// <param name="createIfNotExists">Indicates whether the statement should include an IF NOT EXISTS clause, defaults to true.</param>
        /// <exception cref="InvalidOperationException">Thrown if Name is null or whitespace or Columns is null, empty or if the primary key specified is not any of the provided columns.</exception>
        /// <exception cref="DuplicateColumnException">Thrown if a two columns share the same name (case-insensitive)</exception>
        public SQLiteCommand GetCreateStatement(bool createIfNotExists = true)
        {
            ThrowHelper.InvalidIfNullOrWhitespace(Name, "Table Name");
            ThrowHelper.InvalidIfNullOrEmpty(Columns, "Table Columns");

            if (PrimaryKey != null)
            {
                ThrowHelper.InvalidIfNotAny(Columns, c => c.Name == PrimaryKey, "Table Columns");
            }

            var columnNamesAdded = new HashSet<string>();
            var pkFound = false;

            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE{(createIfNotExists ? " IF NOT EXISTS" : "")} {Name}\r\n(");
            foreach ((Column column, int index) in Columns.Select((column, index) => (column, index)))
            {
                var loweredName = column.Name.ToLower();
                if (columnNamesAdded.Contains(loweredName))
                {
                    throw new DuplicateColumnException(Name, column.Name);
                }
                else
                {
                    columnNamesAdded.Add(loweredName);
                }

                sb.Append($"{column.Name} {column.Type.ToString().ToUpper()}");

                if (!pkFound)
                {
                    if (PrimaryKey != null)
                    {
                        if (column.Name.Equals(PrimaryKey, StringComparison.OrdinalIgnoreCase))
                        {
                            pkFound = true;
                        }
                    }
                    else if (column.Name.Equals("Id", StringComparison.OrdinalIgnoreCase))
                    {
                        pkFound = true;
                    }
                    else if (column.Name.Equals($"{Name}Id"))
                    {
                        pkFound = true;
                    }

                    if (pkFound)
                    {
                        sb.Append(" PRIMARY KEY");
                    }
                }

                if (!column.Nullable)
                {
                    sb.Append(" NOT NULL");
                }

                if (column.Default != null)
                {
                    sb.Append($" DEFAULT {column.Default}");
                }

                if (column.CheckExpression != null)
                {
                    sb.Append($" CHECK({column.Name} {column.CheckExpression})");
                }

                sb.AppendLine(index < Columns.Count - 1 ? "," : "");
            }
            sb.Append(")");

            return new SQLiteCommand(sb.ToString());
        }
    }
}