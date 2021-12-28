using Sqlite.Database.Management.Enumerations;
using Sqlite.Database.Management.Exceptions;

namespace Sqlite.Database.Management
{
    /// <summary>
    /// Represents a table column
    /// </summary>
#if NET5_0_OR_GREATER
    public record Column
#else
    public class Column
#endif
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Allow nulls (defaults to true).
        /// </summary>
        public bool Nullable { get; set; } = true;

        /// <summary>
        /// Default value for column.
        /// </summary>
        public object Default { get; set; }

        /// <summary>
        /// Expression to check for, e.g. Value >= 1 or Value IN (0, 1).
        /// </summary>
        public string CheckExpression { get; set; }

        /// <summary>
        /// Gets or sets the data type of the column.
        /// </summary>
        public ColumnType Type { get; set; }

        /// <summary>
        /// Constructs a new instance of a column and assigns it the provided name and type.
        /// </summary>
        /// <param name="name">Name of the column.</param>
        /// <param name="type">Type of the column. Defaults to Text if not provided.</param>
        /// <exception cref="ArgumentNullException">Thrown if name is null</exception>
        /// <exception cref="ArgumentException">Thrown if name is all whitespace</exception>
        public Column(string name, ColumnType type = ColumnType.Text)
        {
            ThrowHelper.ThrowIfArgumentNullOrWhitespace(name);
            Name = name;
            Type = type;
        }
    }
}