using Sqlite.Database.Management.Attributes;

namespace Sqlite.Database.Management.Test
{
    public record TestObject
    {
        public string StringProperty { get; init; }

        [SqlitePrimaryKey]
        public int IntProperty { get; init; }

        public bool BoolProperty { get; init; }

        [SqliteIgnore]
        public string Ignored { get; init; }
    }
}