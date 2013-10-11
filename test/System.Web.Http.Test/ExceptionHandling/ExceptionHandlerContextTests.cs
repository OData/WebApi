// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionHandlerContextTests
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
            ExceptionHandlerContext product = CreateProductUnderTest(expectedContext);

            // Act
            ExceptionContext context = product.ExceptionContext;

            // Assert
            Assert.Same(expectedContext, context);
        }

        [Fact]
        public void ResultSet_UpdatesValue()
        {
            // Arrange
            ExceptionHandlerContext product = CreateProductUnderTest();
            IHttpActionResult expectedResult = CreateDummyResult();

            // Act
            product.Result = expectedResult;

            // Assert
            IHttpActionResult result = product.Result;
            Assert.Same(expectedResult, result);
        }

        private static ExceptionContext CreateContext()
        {
            return new ExceptionContext();
        }

        private static IHttpActionResult CreateDummyResult()
        {
            return new Mock<IHttpActionResult>(MockBehavior.Strict).Object;
        }

        private static ExceptionHandlerContext CreateProductUnderTest()
        {
            return CreateProductUnderTest(CreateContext());
        }

        private static ExceptionHandlerContext CreateProductUnderTest(ExceptionContext exceptionContext)
        {
            return new ExceptionHandlerContext(exceptionContext);
        }
    }
}
