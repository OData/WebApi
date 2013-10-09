// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionHandlerContextTests
    {
        [Fact]
        public void ExceptionContextSet_UpdatesValue()
        {
            // Arrange
            ExceptionHandlerContext product = CreateProductUnderTest();
            ExceptionContext expectedContext = CreateContext();

            // Act
            product.ExceptionContext = expectedContext;

            // Assert
            ExceptionContext context = product.ExceptionContext;
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
            return new ExceptionHandlerContext();
        }
    }
}
