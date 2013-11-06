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

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(context), "exceptionContext");
        }

        [Fact]
        public void ExceptionContextGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            ExceptionContext expectedContext = CreateContext();
            ExceptionLoggerContext product = CreateProductUnderTest(expectedContext);

            // Act
            ExceptionContext context = product.ExceptionContext;

            // Assert
            Assert.Same(expectedContext, context);
        }

        private static ExceptionContext CreateContext()
        {
            return new ExceptionContext();
        }

        private static ExceptionLoggerContext CreateProductUnderTest(ExceptionContext exceptionContext)
        {
            return new ExceptionLoggerContext(exceptionContext);
        }
    }
}
