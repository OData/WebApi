// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class ExceptionContextTests
    {
        [Fact]
        public void RequestSet_UpdatesValue()
        {
            // Arrange
            ExceptionContext product = CreateProductUnderTest();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                // Act
                product.Request = expectedRequest;

                // Assert
                HttpRequestMessage request = product.Request;
                Assert.Same(expectedRequest, request);
            }
        }

        [Fact]
        public void RequestContextSet_UpdatesValue()
        {
            // Arrange
            ExceptionContext product = CreateProductUnderTest();
            HttpRequestContext expectedRequestContext = CreateRequestContext();

            // Act
            product.RequestContext = expectedRequestContext;

            // Assert
            HttpRequestContext requestContext = product.RequestContext;
            Assert.Same(expectedRequestContext, requestContext);
        }

        [Fact]
        public void ControllerContextSet_UpdatesValue()
        {
            // Arrange
            ExceptionContext product = CreateProductUnderTest();
            HttpControllerContext expectedControllerContext = CreateControllerContext();

            // Act
            product.ControllerContext = expectedControllerContext;

            // Assert
            HttpControllerContext controllerContext = product.ControllerContext;
            Assert.Same(expectedControllerContext, controllerContext);
        }

        [Fact]
        public void ActionContextSet_UpdatesValue()
        {
            // Arrange
            ExceptionContext product = CreateProductUnderTest();
            HttpActionContext expectedActionContext = CreateActionContext();

            // Act
            product.ActionContext = expectedActionContext;

            // Assert
            HttpActionContext actionContext = product.ActionContext;
            Assert.Same(expectedActionContext, actionContext);
        }

        [Fact]
        public void ResponseSet_UpdatesValue()
        {
            // Arrange
            ExceptionContext product = CreateProductUnderTest();

            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                // Act
                product.Response = expectedResponse;

                // Assert
                HttpResponseMessage response = product.Response;
                Assert.Same(expectedResponse, response);
            }
        }

        [Fact]
        public void ConstructorWithoutArguments_SetsPropertiesToSpecifiedValues()
        {
            // Act
            ExceptionContext product = CreateProductUnderTest();

            // Assert
            Assert.Null(product.Exception);
            Assert.Null(product.ActionContext);
            Assert.Null(product.ControllerContext);
            Assert.Null(product.RequestContext);
            Assert.Null(product.Request);
            Assert.Null(product.CatchBlock);
            Assert.False(product.IsTopLevelCatchBlock);
            Assert.Null(product.Response);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConstructorWithActionContext_SetsPropertiesToSpecifiedValues(bool expectedIsTopLevelCatchBlock)
        {
            // Arrange
            Exception expectedException = CreateException();
            HttpRequestContext expectedRequestContext = CreateRequestContext();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                HttpControllerContext expectedControllerContext = CreateControllerContext(expectedRequestContext,
                    expectedRequest);
                HttpActionContext expectedActionContext = CreateActionContext(expectedControllerContext);
                string expectedCatchBlock = CreateCatchBlock();

                // Act
                ExceptionContext product = CreateProductUnderTest(expectedException, expectedActionContext,
                    expectedCatchBlock, expectedIsTopLevelCatchBlock);

                // Assert
                Assert.Same(expectedException, product.Exception);
                Assert.Same(expectedActionContext, product.ActionContext);
                Assert.Same(expectedControllerContext, product.ControllerContext);
                Assert.Same(expectedRequestContext, product.RequestContext);
                Assert.Same(expectedRequest, product.Request);
                Assert.Same(expectedCatchBlock, product.CatchBlock);
                Assert.Equal(expectedIsTopLevelCatchBlock, product.IsTopLevelCatchBlock);
                Assert.Null(product.Response);
            }
        }

        [Fact]
        public void ConstructorWithActionContext_IfExceptionIsNull_Throws()
        {
            // Arrange
            Exception exception = null;
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpControllerContext controllerContext = CreateControllerContext(requestContext, request);
                HttpActionContext actionContext = CreateActionContext(controllerContext);
                string catchBlock = CreateCatchBlock();

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, actionContext, catchBlock, true),
                    "exception");
            }
        }

        [Fact]
        public void ConstructorWithActionContext_IfActionContextIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            HttpActionContext actionContext = null;
            string catchBlock = CreateCatchBlock();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, actionContext, catchBlock, true),
                "actionContext");
        }

        [Fact]
        public void ConstructorWithActionContext_IfControllerContextIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            HttpActionContext actionContext = new HttpActionContext();
            Assert.Null(actionContext.ControllerContext); // Guard
            string catchBlock = CreateCatchBlock();

            // Act & Assert
            Assert.ThrowsArgument(() => CreateProductUnderTest(exception, actionContext, catchBlock, true),
                "actionContext", "HttpActionContext.ControllerContext must not be null.");
        }

        [Fact]
        public void ConstructorWithActionContext_IfRequestIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            HttpRequestContext requestContext = CreateRequestContext();
            HttpControllerContext controllerContext = new HttpControllerContext
            {
                RequestContext = requestContext
            };
            Assert.Null(controllerContext.Request); // Guard
            HttpActionContext actionContext = CreateActionContext(controllerContext);
            string catchBlock = CreateCatchBlock();

            // Act & Assert
            Assert.ThrowsArgument(() => CreateProductUnderTest(exception, actionContext, catchBlock, true),
                "actionContext", "HttpControllerContext.Request must not be null");
        }

        [Fact]
        public void ConstructorWithActionContext_IfCatchBlockIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpControllerContext controllerContext = CreateControllerContext(requestContext, request);
                HttpActionContext actionContext = CreateActionContext(controllerContext);
                string catchBlock = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, actionContext, catchBlock, true),
                    "catchBlock");
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConstructorWithRequest_SetsPropertiesToSpecifiedValues(bool expectedIsTopLevelCatchBlock)
        {
            // Arrange
            Exception expectedException = CreateException();
            HttpRequestContext expectedRequestContext = CreateRequestContext();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                expectedRequest.SetRequestContext(expectedRequestContext);
                string expectedCatchBlock = CreateCatchBlock();

                // Act
                ExceptionContext product = CreateProductUnderTest(expectedException, expectedRequest,
                    expectedCatchBlock, expectedIsTopLevelCatchBlock);

                // Assert
                Assert.Same(expectedException, product.Exception);
                Assert.Same(expectedRequest, product.Request);
                Assert.Same(expectedRequestContext, product.RequestContext);
                Assert.Same(expectedCatchBlock, product.CatchBlock);
                Assert.Equal(expectedIsTopLevelCatchBlock, product.IsTopLevelCatchBlock);
                Assert.Null(product.ControllerContext);
                Assert.Null(product.ActionContext);
                Assert.Null(product.Response);
            }
        }

        [Fact]
        public void ConstructorWithRequest_IfExceptionIsNull_Throws()
        {
            // Arrange
            Exception exception = null;
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                request.SetRequestContext(requestContext);
                string catchBlock = CreateCatchBlock();

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, request, catchBlock, true),
                    "exception");
            }
        }

        [Fact]
        public void ConstructorWithRequest_IfRequestIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            HttpRequestMessage request = null;
            string catchBlock = CreateCatchBlock();

            // Act & Assert
            Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, request, catchBlock, true),
                "request");
        }

        [Fact]
        public void ConstructorWithRequest_IfRequestContextIsNull_IgnoresRequestContext()
        {
            // Arrange
            Exception exception = CreateException();

            using (HttpRequestMessage request = CreateRequest())
            {
                Assert.Null(request.GetRequestContext()); // Guard
                string catchBlock = CreateCatchBlock();

                // Act
                ExceptionContext product = CreateProductUnderTest(exception, request, catchBlock, true);

                // Assert
                Assert.Null(product.RequestContext);
            }
        }

        [Fact]
        public void ConstructorWithRequest_IfCatchBlockIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            {
                request.SetRequestContext(requestContext);
                string catchBlock = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, request, catchBlock, true),
                    "catchBlock");
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConstructorWithRequestAndResponse_SetsPropertiesToSpecifiedValues(bool expectedIsTopLevelCatchBlock)
        {
            // Arrange
            Exception expectedException = CreateException();
            HttpRequestContext expectedRequestContext = CreateRequestContext();

            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage expectedResponse = CreateResponse())
            {
                expectedRequest.SetRequestContext(expectedRequestContext);
                string expectedCatchBlock = CreateCatchBlock();

                // Act
                ExceptionContext product = CreateProductUnderTest(expectedException, expectedRequest, expectedResponse,
                    expectedCatchBlock, expectedIsTopLevelCatchBlock);

                // Assert
                Assert.Same(expectedException, product.Exception);
                Assert.Same(expectedRequest, product.Request);
                Assert.Same(expectedRequestContext, product.RequestContext);
                Assert.Same(expectedResponse, product.Response);
                Assert.Same(expectedCatchBlock, product.CatchBlock);
                Assert.Equal(expectedIsTopLevelCatchBlock, product.IsTopLevelCatchBlock);
                Assert.Null(product.ControllerContext);
                Assert.Null(product.ActionContext);
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfExceptionIsNull_Throws()
        {
            // Arrange
            Exception exception = null;
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage response = CreateResponse())
            {
                request.SetRequestContext(requestContext);
                string catchBlock = CreateCatchBlock();

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, request, response, catchBlock, true),
                    "exception");
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfRequestIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            HttpRequestMessage request = null;

            using (HttpResponseMessage response = CreateResponse())
            {
                string catchBlock = CreateCatchBlock();

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, request, response, catchBlock, true),
                    "request");
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfRequestContextIsNull_IgnoresRequestContext()
        {
            // Arrange
            Exception exception = CreateException();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage response = CreateResponse())
            {
                Assert.Null(request.GetRequestContext()); // Guard
                string catchBlock = CreateCatchBlock();

                // Act
                ExceptionContext product = CreateProductUnderTest(exception, request, response, catchBlock, true);

                // Assert
                Assert.Null(product.RequestContext);
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfResponseIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();

            using (HttpRequestMessage request = CreateRequest())
            {
                HttpResponseMessage response = null;
                string catchBlock = CreateCatchBlock();

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, request, response, catchBlock, true),
                    "response");
            }
        }

        [Fact]
        public void ConstructorWithRequestAndResponse_IfCatchBlockIsNull_Throws()
        {
            // Arrange
            Exception exception = CreateException();
            HttpRequestContext requestContext = CreateRequestContext();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage response = CreateResponse())
            {
                request.SetRequestContext(requestContext);
                string catchBlock = null;

                // Act & Assert
                Assert.ThrowsArgumentNull(() => CreateProductUnderTest(exception, request, response, catchBlock, true),
                    "catchBlock");
            }
        }

        [Fact]
        public void ExceptionSet_UpdatesValue()
        {
            // Arrange
            ExceptionContext product = CreateProductUnderTest();
            Exception expectedException = CreateException();

            // Act
            product.Exception = expectedException;

            // Assert
            Exception exception = product.Exception;
            Assert.Same(expectedException, exception);
        }

        [Fact]
        public void CatchBlockSet_UpdatesValue()
        {
            // Arrange
            ExceptionContext product = CreateProductUnderTest();
            string expectedCatchBlock = CreateCatchBlock();

            // Act
            product.CatchBlock = expectedCatchBlock;

            // Assert
            string catchBlock = product.CatchBlock;
            Assert.Same(expectedCatchBlock, catchBlock);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsTopLevelCatchBlockSet_UpdatesValue(bool expectedIsTopLevelCatchBlock)
        {
            // Arrange
            ExceptionContext product = CreateProductUnderTest();

            // Act
            product.IsTopLevelCatchBlock = expectedIsTopLevelCatchBlock;

            // Assert
            bool isTopLevelCatchBlock = product.IsTopLevelCatchBlock;
            Assert.Equal(expectedIsTopLevelCatchBlock, isTopLevelCatchBlock);
        }

        private static HttpActionContext CreateActionContext()
        {
            return new HttpActionContext();
        }

        private static HttpActionContext CreateActionContext(HttpControllerContext controllerContext)
        {
            return new HttpActionContext
            {
                ControllerContext = controllerContext
            };
        }

        private static string CreateCatchBlock()
        {
            return "IgnoreCaughtAt";
        }

        private static HttpControllerContext CreateControllerContext()
        {
            return new HttpControllerContext();
        }

        private static HttpControllerContext CreateControllerContext(HttpRequestContext requestContext,
            HttpRequestMessage request)
        {
            return new HttpControllerContext
            {
                RequestContext = requestContext,
                Request = request
            };
        }

        private static Exception CreateException()
        {
            return new DivideByZeroException();
        }

        private static ExceptionContext CreateProductUnderTest()
        {
            return new ExceptionContext();
        }

        private static ExceptionContext CreateProductUnderTest(Exception exception, HttpActionContext actionContext,
            string catchBlock, bool isTopLevelCatchBlock)
        {
            return new ExceptionContext(exception, actionContext, catchBlock, isTopLevelCatchBlock);
        }

        private static ExceptionContext CreateProductUnderTest(Exception exception, HttpRequestMessage request,
            string catchBlock, bool isTopLevelCatchBlock)
        {
            return new ExceptionContext(exception, request, catchBlock, isTopLevelCatchBlock);
        }

        private static ExceptionContext CreateProductUnderTest(Exception exception, HttpRequestMessage request,
            HttpResponseMessage response, string catchBlock, bool isTopLevelCatchBlock)
        {
            return new ExceptionContext(exception, request, response, catchBlock, isTopLevelCatchBlock);
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpRequestContext CreateRequestContext()
        {
            return new HttpRequestContext();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }
    }
}
