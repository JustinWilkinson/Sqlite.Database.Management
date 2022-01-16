using Sqlite.Database.Management.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Sqlite.Database.Management.Test.Extensions
{
    public class LinqExtensionsTest
    {
        [Fact]
        public void ForEach_PerformsActionForEachRecord_Successful()
        {
            // Arrange
            var enumerable = new int[] { 1, 2, 3, 4, 5 };
            var sum = 0;

            // Act
            enumerable.ForEach(x => sum += x);

            // Assert
            Assert.Equal(15, sum);
        }

        [Fact]
        public void AsList_AnyObject_CreatesListOfSingleObject()
        {
            // Arrange
            var obj = new object();

            // Act
            var list = obj.AsList();

            // Assert
            var item = Assert.Single(list);
            Assert.Equal(obj, item);
        }

        [Fact]
        public async Task WhereAsync_AnAsyncEnumerable_FiltersAccordingToPredicate()
        {
            // Arrange
            var asyncEnuemrable = new[] { 1, 2, 3, 4, 5 }.ToAsyncEnumerable();

            // Act
            var result = await asyncEnuemrable.WhereAsync(x => x >= 3).ToListAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Collection(result, x => Assert.Equal(3, x), x => Assert.Equal(4, x), x => Assert.Equal(5, x));
        }

        [Fact]
        public async Task SelectAsync_AnAsyncEnumerable_ProjectsAccordingToSelector()
        {
            // Arrange
            var asyncEnuemrable = new[] { 1, 2, 3 }.ToAsyncEnumerable();

            // Act
            var result = await asyncEnuemrable.SelectAsync(x => $"x = {x}").ToListAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Collection(result, x => Assert.Equal("x = 1", x), x => Assert.Equal("x = 2", x), x => Assert.Equal("x = 3", x));
        }

        [Fact]
        public async Task ToListAsync_AnAsyncEnumerable_ConvertsAsyncEnumerableToList()
        {
            // Arrange
            var list = new List<int>(3) { 1, 2, 3 };
            var asyncEnuemrable = list.ToAsyncEnumerable();

            // Act
            var toList = await asyncEnuemrable.ToListAsync();

            // Assert
            Assert.Equal(list, toList);
        }
    }
}