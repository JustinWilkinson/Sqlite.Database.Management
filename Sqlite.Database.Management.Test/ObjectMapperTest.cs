using Sqlite.Database.Management.Mapping;
using System.Data.SQLite;
using Xunit;

namespace Sqlite.Database.Management.Test
{
    public class ObjectMapperTest
    {
        [Fact]
        public void Map_MapsSqliteDataReaderToObject_Successful()
        {
            // Arrange
            var mapper = new ObjectMapper<TestObject>();
            using var database = new InMemoryDatabase();
            database.Create();

            database.Execute("CREATE TABLE Table1 (StringProperty TEXT, IntProperty INTEGER, BoolProperty INTEGER)");
            database.Execute("INSERT INTO Table1 VALUES('Value 1', 1, 1)");

            // Act
            using var reader = new SQLiteCommand("SELECT * FROM Table1", database.GetOpenConnection()).ExecuteReader();
            reader.Read();
            var result = mapper.Map(reader);


            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestObject>(result);
            Assert.Equal("Value 1", result.StringProperty);
            Assert.Equal(1, result.IntProperty);
            Assert.True(result.BoolProperty);
        }

        [Fact]
        public void Insert_InsertsObjectWithValues_Successful()
        {
            // Arrange
            var recordToInsert = new TestObject { StringProperty = "Hello", IntProperty = 7, BoolProperty = false };
            var mapper = new ObjectMapper<TestObject>();
            using var database = new InMemoryDatabase();
            database.Tables.Add(ObjectMapper<TestObject>.Table);
            database.Create();

            // Act
            mapper.Insert(database, recordToInsert);

            // Assert
            using var reader = new SQLiteCommand("SELECT * FROM TestObject", database.GetOpenConnection()).ExecuteReader();
            reader.Read();
            Assert.Equal(recordToInsert.StringProperty, (string)reader["StringProperty"]);
            Assert.Equal(recordToInsert.IntProperty, (int)(long)reader["IntProperty"]);
            Assert.Equal(recordToInsert.BoolProperty, (long)reader["BoolProperty"] == 1);
        }
    }
}