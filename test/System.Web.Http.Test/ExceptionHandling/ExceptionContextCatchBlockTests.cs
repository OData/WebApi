// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionContextCatchBlockTests
    {
        [Fact]
        public void Constructor_IfNameIsNull_Throws()
        {
            // Arrange
            string name = null;
            bool isTopLevel = false;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(name, isTopLevel), "name");
        }

        [Fact]
        public void Name_IsSpecifiedInstance()
        {
            // Arrange
            string expectedName = "TheName";
            bool isTopLevel = true;
            ExceptionContextCatchBlock product = CreateProductUnderTest(expectedName, isTopLevel);

            // Act
            string name = product.Name;

            // Assert
            Assert.Same(expectedName, name);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsTopLevel_IsSpecifiedValue(bool expectedIsTopLevel)
        {
            // Arrange
            string name = "IgnoreName";
            ExceptionContextCatchBlock product = CreateProductUnderTest(name, expectedIsTopLevel);

            // Act
            bool isTopLevel = product.IsTopLevel;

            // Assert
            Assert.Equal(expectedIsTopLevel, isTopLevel);
        }

        [Fact]
        public void ToString_ReturnsName()
        {
            // Arrange
            string expectedName = "TheName";
            bool isTopLevel = false;
            object product = CreateProductUnderTest(expectedName, isTopLevel);

            // Act
            string value = product.ToString();

            // Assert
            Assert.Same(expectedName, value);
        }

        [Fact]
        public void DebuggerDisplayAttribute_IsSpecifiedValue()
        {
            // Act
            DebuggerDisplayAttribute attribute = (DebuggerDisplayAttribute)Attribute.GetCustomAttribute(
                typeof(ExceptionContextCatchBlock), typeof(DebuggerDisplayAttribute));

            // Assert
            Assert.NotNull(attribute);
            string value = attribute.Value;
            Assert.Equal("Name: {Name}, IsTopLevel: {IsTopLevel}", value);
        }

        private static ExceptionContextCatchBlock CreateProductUnderTest(string name, bool isTopLevel)
        {
            return new ExceptionContextCatchBlock(name, isTopLevel);
        }
    }
}
