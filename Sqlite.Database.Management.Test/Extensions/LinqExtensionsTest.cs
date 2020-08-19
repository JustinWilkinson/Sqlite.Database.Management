using Sqlite.Database.Management.Extensions;
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
    }
}