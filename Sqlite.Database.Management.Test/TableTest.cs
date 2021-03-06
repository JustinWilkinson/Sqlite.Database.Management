﻿using Sqlite.Database.Management.Enumerations;
using Sqlite.Database.Management.Exceptions;
using System;
using System.Collections.Generic;
using Xunit;

namespace Sqlite.Database.Management.Test
{
    public class TableTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void GetCreateStatement_NullOrWhitespaceTableName_ThrowsInvalidOperationException(string tableName)
        {
            Assert.Throws<InvalidOperationException>(() => new Table { Name = tableName }.GetCreateStatement());
        }

        [Fact]
        public void GetCreateStatement_NullColumns_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new Table("Test").GetCreateStatement());
        }

        [Fact]
        public void GetCreateStatement_EmptyColumns_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new Table("Test") { Columns = new List<Column>() }.GetCreateStatement());
        }

        [Fact]
        public void GetCreateStatement_PrimaryKeyNotInColumns_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => new Table("Test")
            {
                Columns = new List<Column>()
                {
                    new Column("Column1")
                },
                PrimaryKey = "NotHere"
            }.GetCreateStatement());
        }

        [Fact]
        public void GetCreateStatement_TwoColumnsWithSameName_ThrowsDuplicateColumnException()
        {
            Assert.Throws<DuplicateColumnException>(() => new Table("Test")
            {
                Columns = new List<Column>()
                {
                    new Column("Column1"),
                    new Column("Column1")
                }
            }.GetCreateStatement());
        }

        [Fact]
        public void GetCreateStatement_DefaultOptions_ReturnsCorrectStatement()
        {
            // Arrange
            var table = new Table("Test")
            {
                Columns = new List<Column>
                {
                    new Column("Column1"),
                    new Column("Column2", ColumnType.Integer)
                }
            };

            // Act
            var result = table.GetCreateStatement();

            // Assert
            Assert.Equal($"CREATE TABLE IF NOT EXISTS Test{Environment.NewLine}({Environment.NewLine}Column1 TEXT,{Environment.NewLine}Column2 INTEGER{Environment.NewLine})", result.CommandText);
        }

        [Fact]
        public void GetCreateStatement_NotCreateIfNotExists_ReturnsCorrectStatement()
        {
            // Arrange
            var table = new Table("Test")
            {
                Columns = new List<Column>
                {
                    new Column("Column1"),
                    new Column("Column2", ColumnType.Integer)
                }
            };

            // Act
            var result = table.GetCreateStatement(false);

            // Assert
            Assert.Equal($"CREATE TABLE Test{Environment.NewLine}({Environment.NewLine}Column1 TEXT,{Environment.NewLine}Column2 INTEGER{Environment.NewLine})", result.CommandText);
        }

        [Fact]
        public void GetCreateStatement_WithSpecifiedPrimaryKey_ReturnsCorrectStatement()
        {
            // Arrange
            var table = new Table("Test")
            {
                Columns = new List<Column>
                {
                    new Column("Column1"),
                    new Column("Column2", ColumnType.Integer)
                },
                PrimaryKey = "Column1"
            };

            // Act
            var result = table.GetCreateStatement();

            // Assert
            Assert.Equal($"CREATE TABLE IF NOT EXISTS Test{Environment.NewLine}({Environment.NewLine}Column1 TEXT PRIMARY KEY,{Environment.NewLine}Column2 INTEGER{Environment.NewLine})", result.CommandText);
        }

        [Fact]
        public void GetCreateStatement_AutoDetectIdPrimaryKey_ReturnsCorrectStatement()
        {
            // Arrange
            var table = new Table("Test")
            {
                Columns = new List<Column>
                {
                    new Column("Id", ColumnType.Integer),
                    new Column("Column1"),
                    new Column("Column2", ColumnType.Integer)
                }
            };

            // Act
            var result = table.GetCreateStatement();

            // Assert
            Assert.Equal($"CREATE TABLE IF NOT EXISTS Test{Environment.NewLine}({Environment.NewLine}Id INTEGER PRIMARY KEY,{Environment.NewLine}Column1 TEXT,{Environment.NewLine}Column2 INTEGER{Environment.NewLine})", result.CommandText);
        }

        [Fact]
        public void GetCreateStatement_AutoDetectTableNameIdPrimaryKey_ReturnsCorrectStatement()
        {
            // Arrange
            var table = new Table("Test")
            {
                Columns = new List<Column>
                {
                    new Column("TestId", ColumnType.Integer),
                    new Column("Column1"),
                    new Column("Column2", ColumnType.Integer)
                }
            };

            // Act
            var result = table.GetCreateStatement();

            // Assert
            Assert.Equal($"CREATE TABLE IF NOT EXISTS Test{Environment.NewLine}({Environment.NewLine}TestId INTEGER PRIMARY KEY,{Environment.NewLine}Column1 TEXT,{Environment.NewLine}Column2 INTEGER{Environment.NewLine})", result.CommandText);
        }

        [Fact]
        public void GetCreateStatement_NonNullableColumn_ReturnsCorrectStatement()
        {
            // Arrange
            var table = new Table("Test")
            {
                Columns = new List<Column>
                {
                    new Column("Column1") { Nullable = false },
                    new Column("Column2", ColumnType.Integer)
                }
            };

            // Act
            var result = table.GetCreateStatement();

            // Assert
            Assert.Equal($"CREATE TABLE IF NOT EXISTS Test{Environment.NewLine}({Environment.NewLine}Column1 TEXT NOT NULL,{Environment.NewLine}Column2 INTEGER{Environment.NewLine})", result.CommandText);
        }

        [Fact]
        public void GetCreateStatement_ColumnWithDefaultValue_ReturnsCorrectStatement()
        {
            // Arrange
            var table = new Table("Test")
            {
                Columns = new List<Column>
                {
                    new Column("Column1") { Default = "'DefaultString'" },
                    new Column("Column2", ColumnType.Integer)
                }
            };

            // Act
            var result = table.GetCreateStatement();

            // Assert
            Assert.Equal($"CREATE TABLE IF NOT EXISTS Test{Environment.NewLine}({Environment.NewLine}Column1 TEXT DEFAULT 'DefaultString',{Environment.NewLine}Column2 INTEGER{Environment.NewLine})", result.CommandText);
        }

        [Fact]
        public void GetCreateStatement_ColumnWithCheck_ReturnsCorrectStatement()
        {
            // Arrange
            var table = new Table("Test")
            {
                Columns = new List<Column>
                {
                    new Column("Column1"),
                    new Column("Column2", ColumnType.Integer) { CheckExpression = "IN (0,1)" }
                }
            };

            // Act
            var result = table.GetCreateStatement();

            // Assert
            Assert.Equal($"CREATE TABLE IF NOT EXISTS Test{Environment.NewLine}({Environment.NewLine}Column1 TEXT,{Environment.NewLine}Column2 INTEGER CHECK(Column2 IN (0,1)){Environment.NewLine})", result.CommandText);
        }
    }
}