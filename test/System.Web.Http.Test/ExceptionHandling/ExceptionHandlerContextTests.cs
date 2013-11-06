// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
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

        [Fact]
        public void ExceptionGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            Exception expectedException = new InvalidOperationException();
            ExceptionContext context = new ExceptionContext
            {
                Exception = expectedException
            };
            ExceptionHandlerContext product = CreateProductUnderTest(context);

            // Act
            Exception exception = product.Exception;

            // Assert
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void CatchBlockGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            ExceptionContextCatchBlock expectedCatchBlock = new ExceptionContextCatchBlock("IgnoreName", false, false);
            ExceptionContext context = new ExceptionContext
            {
                CatchBlock = expectedCatchBlock
            };
            ExceptionHandlerContext product = CreateProductUnderTest(context);

            // Act
            ExceptionContextCatchBlock catchBlock = product.CatchBlock;

            // Assert
            Assert.Same(expectedCatchBlock, catchBlock);
        }

        [Fact]
        public void RequestGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            {
                ExceptionContext context = new ExceptionContext
                {
                    Request = expectedRequest
                };
                ExceptionHandlerContext product = CreateProductUnderTest(context);

                // Act
                HttpRequestMessage request = product.Request;

                // Assert
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void RequestContextGet_ReturnsSpecifiedInstance()
        {
            // Arrange
            HttpRequestContext expectedRequestContext = new HttpRequestContext();
            ExceptionContext context = new ExceptionContext
            {
                RequestContext = expectedRequestContext
            };
            ExceptionHandlerContext product = CreateProductUnderTest(context);

            // Act
            HttpRequestContext requestContext = product.RequestContext;

            // Assert
            Assert.Same(expectedRequestContext, requestContext);
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
