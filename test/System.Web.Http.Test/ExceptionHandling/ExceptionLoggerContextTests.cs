// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionLoggerContextTests
    {
        [Fact]
        public void Constructor_IfExceptionContextIsNull_Throws()
        {
            // Arrange
            ExceptionContext context = null;
            bool canBeHandled = true;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(context, canBeHandled), "exceptionContext");
        }

        [Fact]
        public void ExceptionContextGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            ExceptionContext expectedContext = CreateContext();
            bool canBeHandled = false;
            ExceptionLoggerContext product = CreateProductUnderTest(expectedContext, canBeHandled);

            // Act
            ExceptionContext context = product.ExceptionContext;

            // Assert
            Assert.Same(expectedContext, context);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CanBeHandledGet_ReturnsSpecifiedValue(bool expectedCanBeHandled)
        {
            // Arrange
            ExceptionContext context = CreateContext();
            ExceptionLoggerContext product = CreateProductUnderTest(context, expectedCanBeHandled);

            // Act
            bool canBeHandled = product.CanBeHandled;

            // Assert
            Assert.Equal(expectedCanBeHandled, canBeHandled);
        }

        private static ExceptionContext CreateContext()
        {
            return new ExceptionContext();
        }

        private static ExceptionLoggerContext CreateProductUnderTest(ExceptionContext exceptionContext,
            bool canBeHandled)
        {
            return new ExceptionLoggerContext(exceptionContext, canBeHandled);
        }
    }
}
