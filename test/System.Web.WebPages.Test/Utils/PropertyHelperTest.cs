// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.TestCommon;

namespace System.Web.WebPages.Test
{
    public class PropertyHelperTest
    {
        [Fact]
        public void PropertyHelperReturnsNameCorrectly()
        {
            // Arrange
            var anonymous = new { foo = "bar" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            PropertyHelper helper = new PropertyHelper(property);

            // Assert
            Assert.Equal("foo", property.Name);
            Assert.Equal("foo", helper.Name);
        }

        [Fact]
        public void PropertyHelperReturnsValueCorrectly()
        {
            // Arrange
            var anonymous = new { bar = "baz" };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            PropertyHelper helper = new PropertyHelper(property);

            // Assert
            Assert.Equal("bar", helper.Name);
            Assert.Equal("baz", helper.GetValue(anonymous));
        }

        [Fact]
        public void PropertyHelperReturnsValueCorrectlyForValueTypes()
        {
            // Arrange
            var anonymous = new { foo = 32 };
            PropertyInfo property = anonymous.GetType().GetProperties().First();

            // Act
            PropertyHelper helper = new PropertyHelper(property);

            // Assert
            Assert.Equal("foo", helper.Name);
            Assert.Equal(32, helper.GetValue(anonymous));
        }

        [Fact]
        public void PropertyHelperReturnsCachedPropertyHelper()
        {
            // Arrange
            var anonymous = new { foo = "bar" };

            // Act
            PropertyHelper[] helpers1 = PropertyHelper.GetProperties(anonymous);
            PropertyHelper[] helpers2 = PropertyHelper.GetProperties(anonymous);

            // Assert
            Assert.Equal(1, helpers1.Length);
            Assert.ReferenceEquals(helpers1, helpers2);
            Assert.ReferenceEquals(helpers1[0], helpers2[0]);
        }

        [Fact]
        public void PropertyHelperDoesNotChangeUnderscores()
        {
            // Arrange
            var anonymous = new { bar_baz2 = "foo" };

            // Act
            PropertyHelper helper = PropertyHelper.GetProperties(anonymous).Single();

            // Assert
            Assert.Equal("bar_baz2", helper.Name);
        }

        private class PrivateProperties
        {
            public int Prop1 { get; set; }
            protected int Prop2 { get; set; }
            private int Prop3 { get; set; }
        }

        [Fact]
        public void PropertyHelperDoesNotFindPrivateProperties()
        {
            // Arrange
            var anonymous = new PrivateProperties();

            // Act
            PropertyHelper helper = PropertyHelper.GetProperties(anonymous).Single();

            // Assert
            Assert.Equal("Prop1", helper.Name);
        }

        private class Derived : PrivateProperties
        {
            public int Prop4 { get; set; }
        }

        [Fact]
        public void PropertyHelperDoesNotFindBaseClassProperties()
        {
            // Arrange
            var anonymous = new Derived();

            // Act
            PropertyHelper helper = PropertyHelper.GetProperties(anonymous).Single();

            // Assert
            Assert.Equal("Prop4", helper.Name);
        }

        private class Static
        {
            public static int Prop2 { get; set; }
            public int Prop5 { get; set; }
        }

        [Fact]
        public void PropertyHelperDoesNotFindStaticProperties()
        {
            // Arrange
            var anonymous = new Static();

            // Act
            PropertyHelper helper = PropertyHelper.GetProperties(anonymous).Single();

            // Assert
            Assert.Equal("Prop5", helper.Name);
        }

        private class SetOnly
        {
            public int Prop2 { set { } }
            public int Prop6 { get; set; }
        }

        [Fact]
        public void PropertyHelperDoesNotFindSetOnlyProperties()
        {
            // Arrange
            var anonymous = new SetOnly();

            // Act
            PropertyHelper helper = PropertyHelper.GetProperties(anonymous).Single();

            // Assert
            Assert.Equal("Prop6", helper.Name);
        }

        private struct MyProperties
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }

        [Fact]
        public void PropertyHelperWorksForStruct()
        {
            // Arrange
            var anonymous = new MyProperties();

            anonymous.IntProp = 3;
            anonymous.StringProp = "Five";

            // Act
            PropertyHelper helper1 = PropertyHelper.GetProperties(anonymous).Where(prop => prop.Name == "IntProp").Single();
            PropertyHelper helper2 = PropertyHelper.GetProperties(anonymous).Where(prop => prop.Name == "StringProp").Single();

            // Assert
            Assert.Equal(3, helper1.GetValue(anonymous));
            Assert.Equal("Five", helper2.GetValue(anonymous));
        }
    }
}
