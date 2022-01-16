using Sqlite.Database.Management.Extensions;
using System;
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
            var asyncEnumerable = new[] { 1, 2, 3, 4, 5 }.ToAsyncEnumerable();

            // Act
            var result = await asyncEnumerable.WhereAsync(x => x >= 3).ToListAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Collection(result, x => Assert.Equal(3, x), x => Assert.Equal(4, x), x => Assert.Equal(5, x));
        }

        [Fact]
        public async Task SelectAsync_AnAsyncEnumerable_ProjectsAccordingToSelector()
        {
            // Arrange
            var asyncEnumerable = new[] { 1, 2, 3 }.ToAsyncEnumerable();

            // Act
            var result = await asyncEnumerable.SelectAsync(x => $"x = {x}").ToListAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Collection(result, x => Assert.Equal("x = 1", x), x => Assert.Equal("x = 2", x), x => Assert.Equal("x = 3", x));
        }

        [Fact]
        public async Task SingleAsync_AnAsyncEnumerableOfOneItem_TakesSingleItem()
        {
            // Arrange
            var asyncEnumerable = new[] { 1 }.ToAsyncEnumerable();

            // Act
            var result = await asyncEnumerable.SingleAsync();

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task SingleAsync_AnAsyncEnumerableOfZeroItems_ThrowsInvalidOperationExceptionWithCorrectMessage()
        {
            // Arrange
            var asyncEnumerable = Array.Empty<int>().ToAsyncEnumerable();

            // Act/Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => asyncEnumerable.SingleAsync());
            Assert.Equal("Collection contains no elements!", ex.Message);
        }

        [Fact]
        public async Task SingleAsync_AnAsyncEnumerableOfMoreThanOneItem_ThrowsInvalidOperationExceptionWithCorrectMessage()
        {
            // Arrange
            var asyncEnumerable = new[] { 1, 2 }.ToAsyncEnumerable();

            // Act/Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => asyncEnumerable.SingleAsync());
            Assert.Equal("Collection contains more than one element!", ex.Message);
        }

        [Fact]
        public async Task ToListAsync_AnAsyncEnumerable_ConvertsAsyncEnumerableToList()
        {
            // Arrange
            var list = new List<int>(3) { 1, 2, 3 };
            var asyncEnumerable = list.ToAsyncEnumerable();

            // Act
            var toList = await asyncEnumerable.ToListAsync();

            // Assert
            Assert.Equal(list, toList);
        }
    }
}