using Sqlite.Database.Management.Enumerations;
using Sqlite.Database.Management.Extensions;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Sqlite.Database.Management.Test.Extensions
{
    public class DatabaseExtensionsTest
    {
        private readonly DatabaseBase _database;

        public DatabaseExtensionsTest()
        {
            _database = new InMemoryDatabase();
            _database.Tables.Add(new Table("TestObject")
            {
                PrimaryKey = "IntProperty",
                Columns = new List<Column>
                {
                    new Column("StringProperty"),
                    new Column("IntProperty", ColumnType.Integer) { Nullable = false },
                    new Column("BoolProperty", ColumnType.Integer) { Nullable = false, CheckExpression = "IN (0, 1)" }
                }
            });
            _database.Create();
        }

        [Fact]
        public void Insert_InsertsObjectWithValues_Successful()
        {
            // Arrange
            var recordToInsert = new TestObject { StringProperty = "Hello", IntProperty = 7, BoolProperty = false };

            // Act
            _database.Insert(recordToInsert);

            // Assert
            using var reader = new SQLiteCommand("SELECT * FROM TestObject", _database.GetOpenConnection()).ExecuteReader();
            reader.Read();
            Assert.Equal(recordToInsert.StringProperty, (string)reader["StringProperty"]);
            Assert.Equal(recordToInsert.IntProperty, (int)(long)reader["IntProperty"]);
            Assert.Equal(recordToInsert.BoolProperty, (long)reader["BoolProperty"] == 1);
        }

        [Fact]
        public async Task InsertAsync_InsertsObjectWithValues_Successful()
        {
            // Arrange
            var recordToInsert = new TestObject { StringProperty = "Hello", IntProperty = 7, BoolProperty = false };

            // Act
            await _database.InsertAsync(recordToInsert);

            // Assert
            using var reader = await new SQLiteCommand("SELECT * FROM TestObject", _database.GetOpenConnection()).ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.Equal(recordToInsert.StringProperty, (string)reader["StringProperty"]);
            Assert.Equal(recordToInsert.IntProperty, (int)(long)reader["IntProperty"]);
            Assert.Equal(recordToInsert.BoolProperty, (long)reader["BoolProperty"] == 1);
        }

        [Fact]
        public void Update_UpdatesObjectWithValues_Successful()
        {
            // Arrange
            var updatedRecord = new TestObject { StringProperty = "New Value", IntProperty = 1, BoolProperty = true };
            _database.Execute("INSERT INTO TestObject VALUES ('Value 1', 1, 1)");

            // Act
            _database.Update(updatedRecord);

            // Assert
            using var reader = new SQLiteCommand("SELECT * FROM TestObject", _database.GetOpenConnection()).ExecuteReader();
            reader.Read();
            Assert.Equal(updatedRecord.StringProperty, (string)reader["StringProperty"]);
            Assert.Equal(updatedRecord.IntProperty, (int)(long)reader["IntProperty"]);
            Assert.Equal(updatedRecord.BoolProperty, (long)reader["BoolProperty"] == 1);
        }


        [Fact]
        public async Task UpdateAsync_UpdatesObjectWithValues_Successful()
        {
            // Arrange
            var updatedRecord = new TestObject { StringProperty = "New Value", IntProperty = 1, BoolProperty = true };
            _database.Execute("INSERT INTO TestObject VALUES ('Value 1', 1, 1)");

            // Act
            await _database.UpdateAsync(updatedRecord);

            // Assert
            using var reader = await new SQLiteCommand("SELECT * FROM TestObject", _database.GetOpenConnection()).ExecuteReaderAsync();
            await reader.ReadAsync();
            Assert.Equal(updatedRecord.StringProperty, (string)reader["StringProperty"]);
            Assert.Equal(updatedRecord.IntProperty, (int)(long)reader["IntProperty"]);
            Assert.Equal(updatedRecord.BoolProperty, (long)reader["BoolProperty"] == 1);
        }

        [Fact]
        public void Delete_DeletesObjectWithValues_Successful()
        {
            // Arrange
            var recordToDelete = new TestObject { StringProperty = "Value 1", IntProperty = 1, BoolProperty = true };
            _database.Execute("INSERT INTO TestObject VALUES ('Value 1', 1, 1),('Value 2', 2, 1)");

            // Act
            _database.Delete(recordToDelete);

            // Assert
            var result = new SQLiteCommand("SELECT COUNT(*) FROM TestObject", _database.GetOpenConnection()).ExecuteScalar();
            Assert.Equal(1L, result);
        }

        [Fact]
        public async Task DeleteAsync_DeletesObjectWithValues_Successful()
        {
            // Arrange
            var recordToDelete = new TestObject { StringProperty = "Value 1", IntProperty = 1, BoolProperty = true };
            _database.Execute("INSERT INTO TestObject VALUES ('Value 1', 1, 1),('Value 2', 2, 1)");

            // Act
            await _database.DeleteAsync(recordToDelete);

            // Assert
            var result = await new SQLiteCommand("SELECT COUNT(*) FROM TestObject", _database.GetOpenConnection()).ExecuteScalarAsync();
            Assert.Equal(1L, result);
        }

        [Fact]
        public void Select_WithId_Successful()
        {
            // Arrange
            _database.Execute("INSERT INTO TestObject (StringProperty, IntProperty, BoolProperty) VALUES ('Value 1', 1, 1),('Value 2', 2, 0)");

            // Act
            var result = _database.Select<TestObject, int>(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Value 1", result.StringProperty);
            Assert.Equal(1, result.IntProperty);
            Assert.True(result.BoolProperty);
        }

        [Fact]
        public async Task SelectAsync_WithId_Successful()
        {
            // Arrange
            _database.Execute("INSERT INTO TestObject (StringProperty, IntProperty, BoolProperty) VALUES ('Value 1', 1, 1),('Value 2', 2, 0)");

            // Act
            var result = await _database.SelectAsync<TestObject, int>(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Value 1", result.StringProperty);
            Assert.Equal(1, result.IntProperty);
            Assert.True(result.BoolProperty);
        }

        [Fact]
        public void Select_SelectsObjectsAndConverts_Successful()
        {
            // Arrange
            _database.Execute("INSERT INTO TestObject VALUES ('Value 1', 1, 1),('Value 2', 2, 0)");

            // Act
            var results = _database.Select<TestObject>().ToList();

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

        [Fact]
        public async Task SelectAsync_SelectsObjectsAndConverts_Successful()
        {
            // Arrange
            _database.Execute("INSERT INTO TestObject VALUES ('Value 1', 1, 1),('Value 2', 2, 0)");

            // Act
            var results = await _database.SelectAsync<TestObject>().ToListAsync();

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
    }
}