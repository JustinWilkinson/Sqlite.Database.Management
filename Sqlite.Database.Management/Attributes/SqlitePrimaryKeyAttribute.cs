using System;

namespace Sqlite.Database.Management.Attributes
{
    /// <summary>
    /// Atrribute that denotes that the property is the PrimaryKey for this record, composite keys are not supported.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SqlitePrimaryKeyAttribute : Attribute
    {
        /// <summary>
        /// Denotes that the property is the PrimaryKey.
        /// </summary>
        public SqlitePrimaryKeyAttribute()
        {
        }
    }
}
