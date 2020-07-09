using Sqlite.Database.Management.Extensions;
using System;
using Xunit;

namespace Sqlite.Database.Management.Test.Extensions
{
    public class ReflectionExtensionsTest
    {
        [Fact]
        public void GetSetter_StringProperty_GetsUsablePropertySetter()
        {
            // Arrange
            var obj = new TestObject { StringProperty = "Original" };
            var propertyInfo = obj.GetType().GetProperty("StringProperty");

            // Act
            var setter = propertyInfo.GetSetter<TestObject>();
            setter(obj, "New");

            // Assert
            Assert.Equal("New", obj.StringProperty);
        }

        [Fact]
        public void GetSetter_IntProperty_GetsUsablePropertySetter()
        {
            // Arrange
            var obj = new TestObject { IntProperty = 100 };
            var propertyInfo = obj.GetType().GetProperty("IntProperty");

            // Act
            var setter = propertyInfo.GetSetter<TestObject>();
            setter(obj, 200);

            // Assert
            Assert.Equal(200, obj.IntProperty);
        }

        [Fact]
        public void GetSetter_BoolProperty_GetsUsablePropertySetter()
        {
            // Arrange
            var obj = new TestObject { BoolProperty = false };
            var propertyInfo = obj.GetType().GetProperty("BoolProperty");

            // Act
            var setter = propertyInfo.GetSetter<TestObject>();
            setter(obj, true);

            // Assert
            Assert.True(obj.BoolProperty);
        }
    }
}