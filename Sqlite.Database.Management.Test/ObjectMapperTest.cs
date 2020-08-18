using Sqlite.Database.Management.Mapping;
using System.Data.SQLite;
using Xunit;

namespace Sqlite.Database.Management.Test
{
    public class ObjectMapperTest
    {
        private readonly string _connectionString = new SQLiteConnectionStringBuilder { DataSource = ":memory:" }.ToString();

        [Fact]
        public void Map_MapsSqliteDataReaderToObject_Successful()
        {
            // Arrange
            var mapper = new ObjectMapper<TestObject>();
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            new SQLiteCommand("CREATE TABLE Table1 (StringProperty TEXT, IntProperty INTEGER, BoolProperty INTEGER)", connection).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO Table1 VALUES('Value 1', 1, 1)", connection).ExecuteNonQuery();

            // Act
            using var reader = new SQLiteCommand("SELECT * FROM Table1", connection).ExecuteReader();
            reader.Read();
            var result = mapper.Map(reader);


            // Assert
            Assert.NotNull(result);
            Assert.IsType<TestObject>(result);
            Assert.Equal("Value 1", result.StringProperty);
            Assert.Equal(1, result.IntProperty);
            Assert.True(result.BoolProperty);
        }
    }
}