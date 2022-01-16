using System;

namespace Sqlite.Database.Management.Attributes
{
    /// <summary>
    /// Denotes that the property should be ignored by Sqlite.Database.Management.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqliteIgnoreAttribute : Attribute
    {
        /// <summary>
        /// Denotes that the property should be ignored by Sqlite.Database.Management.
        /// </summary>
        public SqliteIgnoreAttribute()
        {
        }
    }
}
