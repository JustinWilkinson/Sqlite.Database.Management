using System;
using System.Data.SQLite;
using System.Linq;
using Xunit;

namespace Sqlite.Database.Management.Test
{
    public class ObjectMapperTest : IDisposable
    {
        private readonly DatabaseBase _database;
        private readonly ObjectMapper<TestObject> _mapper;

        public ObjectMapperTest()
        {
            _mapper = new ObjectMapper<TestObject>();
            var table = ObjectMapper<TestObject>.Table;
            table.PrimaryKey = "IntProperty";
            _database = new InMemoryDatabase();
            _database.Tables.Add(table);
            _database.Create();
        }

        [Fact]
        public void Map_MapsSqliteDataReaderToObject_Successful()
        {
            // Arrange
            _database.Execute("INSERT INTO TestObject (StringProperty, IntProperty, BoolProperty) VALUES ('Value 1', 1, 1)");

            // Act
            using var reader = new SQLiteCommand("SELECT * FROM TestObject", _database.GetOpenConnection()).ExecuteReader();
            reader.Read();
            var result = _mapper.Map(reader);

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

            // Act
            _mapper.Insert(_database, recordToInsert);

            // Assert
            using var reader = new SQLiteCommand("SELECT * FROM TestObject", _database.GetOpenConnection()).ExecuteReader();
            reader.Read();
            Assert.Equal(recordToInsert.StringProperty, (string)reader["StringProperty"]);
            Assert.Equal(recordToInsert.IntProperty, (int)(long)reader["IntProperty"]);
            Assert.Equal(recordToInsert.BoolProperty, (long)reader["BoolProperty"] == 1);
        }

        [Fact]
        public void Update_UpdatesObjectWithValues_Successful()
        {
            // Arrange
            var updatedRecord = new TestObject { StringProperty = "New Value", IntProperty = 1, BoolProperty = true };
            _database.Execute("INSERT INTO TestObject (StringProperty, IntProperty, BoolProperty) VALUES ('Value 1', 1, 1)");

            // Act
            _mapper.Update(_database, updatedRecord);

            // Assert
            using var reader = new SQLiteCommand("SELECT * FROM TestObject", _database.GetOpenConnection()).ExecuteReader();
            reader.Read();
            Assert.Equal(updatedRecord.StringProperty, (string)reader["StringProperty"]);
            Assert.Equal(updatedRecord.IntProperty, (int)(long)reader["IntProperty"]);
            Assert.Equal(updatedRecord.BoolProperty, (long)reader["BoolProperty"] == 1);
        }

        [Fact]
        public void Delete_DeletesObjectWithValues_Successful()
        {
            // Arrange
            var recordToDelete = new TestObject { StringProperty = "Value 1", IntProperty = 1, BoolProperty = true };
            _database.Execute("INSERT INTO TestObject (StringProperty, IntProperty, BoolProperty) VALUES ('Value 1', 1, 1),('Value 2', 2, 1)");

            // Act
            _mapper.Delete(_database, recordToDelete);

            // Assert
            var result = new SQLiteCommand("SELECT COUNT(*) FROM TestObject", _database.GetOpenConnection()).ExecuteScalar();
            Assert.Equal(1L, result);
        }

        [Fact]
        public void Select_SelectsObjectsAndConverts_Successful()
        {
            // Arrange
            _database.Execute("INSERT INTO TestObject (StringProperty, IntProperty, BoolProperty) VALUES ('Value 1', 1, 1),('Value 2', 2, 0)");

            // Act
            var results = _mapper.Select(_database).ToList();

            // Assert
            Assert.NotNull(results);
            Assert.Equal(2, results.Count);
            Assert.Equal("Value 1", results[0].StringProperty);
            Assert.Equal(1, results[0].IntProperty);
            Assert.True(results[0].BoolProperty);
            Assert.Equal("Value 2", results[1].StringProperty);
            Assert.Equal(2, results[1].IntProperty);
            Assert.False(results[1].BoolProperty);
        }

        public void Dispose()
        {
            _database.Delete();
            GC.SuppressFinalize(this);
        }
    }
}