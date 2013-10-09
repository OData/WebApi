// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionLoggerContextTests
    {
        [Fact]
        public void ExceptionContextSet_UpdatesValue()
        {
            // Arrange
            ExceptionLoggerContext product = CreateProductUnderTest();
            ExceptionContext expectedContext = CreateContext();

            // Act
            product.ExceptionContext = expectedContext;

            // Assert
            ExceptionContext context = product.ExceptionContext;
            Assert.Same(expectedContext, context);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void CanBeHandledSet_UpdatesValue(bool expectedCanBeHandled)
        {
            // Arrange
            ExceptionLoggerContext product = CreateProductUnderTest();

            // Act
            product.CanBeHandled = expectedCanBeHandled;

            // Assert
            bool canBeHandled = product.CanBeHandled;
            Assert.Equal(expectedCanBeHandled, canBeHandled);
        }

        private static ExceptionContext CreateContext()
        {
            return new ExceptionContext();
        }

        private static ExceptionLoggerContext CreateProductUnderTest()
        {
            return new ExceptionLoggerContext();
        }
    }
}
