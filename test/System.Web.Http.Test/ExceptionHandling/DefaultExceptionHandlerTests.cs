// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Results;
using Microsoft.TestCommon;

namespace System.Web.Http.ExceptionHandling
{
    public class DefaultExceptionHandlerTests
    {
        [Fact]
        public void HandleAsync_IfContextIsNull_Throws()
        {
            // Arrange
            IExceptionHandler product = CreateProductUnderTest();
            ExceptionHandlerContext context = null;
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgumentNull(() => product.HandleAsync(context, cancellationToken), "context");
        }

        [Fact]
        public void HandleAsync_IfExceptionContextIsNull_Throws()
        {
            // Arrange
            IExceptionHandler product = CreateProductUnderTest();
            ExceptionHandlerContext context = new ExceptionHandlerContext();
            Assert.Null(context.ExceptionContext); // Guard
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                "ExceptionHandlerContext.ExceptionContext must not be null.");
        }

        [Fact]
        public void HandleAsync_IfExceptionIsNull_Throws()
        {
            // Arrange
            IExceptionHandler product = CreateProductUnderTest();
            ExceptionHandlerContext context = new ExceptionHandlerContext()
            {
                ExceptionContext = new ExceptionContext()
            };
            Assert.Null(context.ExceptionContext.Exception); // Guard
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                "ExceptionContext.Exception must not be null.");
        }

        [Fact]
        public void HandleAsync_IfRequestIsNull_Throws()
        {
            // Arrange
            IExceptionHandler product = CreateProductUnderTest();
            ExceptionHandlerContext context = new ExceptionHandlerContext()
            {
                ExceptionContext = new ExceptionContext
                {
                    Exception = CreateException()
                }
            };
            Assert.Null(context.ExceptionContext.Request); // Guard
            CancellationToken cancellationToken = CancellationToken.None;

            // Act & Assert
            Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                "ExceptionContext.Request must not be null.");
        }

        [Fact]
        public void HandleAsync_HandlesExceptionViaCreateErrorResponse()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            {
                expectedRequest.SetRequestContext(new HttpRequestContext { IncludeErrorDetail = true });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest);
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                Task task = product.HandleAsync(context, cancellationToken);
                task.WaitUntilCompleted();

                // Assert
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                IHttpActionResult result = context.Result;
                Assert.IsType(typeof(ResponseMessageResult), result);
                ResponseMessageResult typedResult = (ResponseMessageResult)result;
                using (HttpResponseMessage response = typedResult.Response)
                {
                    Assert.NotNull(response);

                    using (HttpResponseMessage expectedResponse = expectedRequest.CreateErrorResponse(
                        HttpStatusCode.InternalServerError, context.ExceptionContext.Exception))
                    {
                        AssertErrorResponse(expectedResponse, response);
                    }

                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsIExceptionFilter_LeavesExceptionUnhandled()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ExceptionHandlerContext context = CreateValidContext(request, ExceptionCatchBlocks.IExceptionFilter);
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                Task task = product.HandleAsync(context, cancellationToken);
                task.WaitUntilCompleted();

                // Assert
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                Assert.Null(context.Result);
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_HandlesWithCustomException()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage originalResponse = CreateResponse())
            {
                originalResponse.Content = new StringContent("Error");
                originalResponse.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
                expectedRequest.SetRequestContext(new HttpRequestContext { IncludeErrorDetail = true });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest,
                    "HttpControllerHandler.WriteBufferedResponseContentAsync");
                context.ExceptionContext.Response = originalResponse;
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                Task task = product.HandleAsync(context, cancellationToken);
                task.WaitUntilCompleted();

                // Assert
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                IHttpActionResult result = context.Result;
                Assert.IsType(typeof(ResponseMessageResult), result);
                ResponseMessageResult typedResult = (ResponseMessageResult)result;

                using (HttpResponseMessage response = typedResult.Response)
                using (HttpResponseMessage expectedResponse = expectedRequest.CreateErrorResponse(
                    HttpStatusCode.InternalServerError, new InvalidOperationException("The 'StringContent' type " +
                        "failed to serialize the response body for content type 'text/plain'.",
                        context.ExceptionContext.Exception)))
                {
                    AssertErrorResponse(expectedResponse, response);
                }
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_WithoutContentType_HandlesWithCustomException()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage originalResponse = CreateResponse())
            {
                originalResponse.Content = new StringContent("Error");
                originalResponse.Content.Headers.ContentType = null;
                expectedRequest.SetRequestContext(new HttpRequestContext { IncludeErrorDetail = true });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest,
                    "HttpControllerHandler.WriteBufferedResponseContentAsync");
                context.ExceptionContext.Response = originalResponse;
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                Task task = product.HandleAsync(context, cancellationToken);
                task.WaitUntilCompleted();

                // Assert
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                IHttpActionResult result = context.Result;
                Assert.IsType(typeof(ResponseMessageResult), result);
                ResponseMessageResult typedResult = (ResponseMessageResult)result;

                using (HttpResponseMessage response = typedResult.Response)
                using (HttpResponseMessage expectedResponse = expectedRequest.CreateErrorResponse(
                    HttpStatusCode.InternalServerError, new InvalidOperationException(
                        "The 'StringContent' type failed to serialize the response body.",
                        context.ExceptionContext.Exception)))
                {
                    AssertErrorResponse(expectedResponse, response);
                }
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_WithFailedNegotiation_HandlesWithCustomException()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage originalResponse = CreateResponse())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                configuration.Formatters.Clear();

                originalResponse.Content = new StringContent("Error");
                expectedRequest.SetRequestContext(new HttpRequestContext
                {
                    IncludeErrorDetail = true,
                    Configuration = configuration
                });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest,
                    "HttpControllerHandler.WriteBufferedResponseContentAsync");
                context.ExceptionContext.Response = originalResponse;
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                Task task = product.HandleAsync(context, cancellationToken);
                task.WaitUntilCompleted();

                // Assert
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                IHttpActionResult result = context.Result;
                Assert.IsType(typeof(ResponseMessageResult), result);
                ResponseMessageResult typedResult = (ResponseMessageResult)result;
                using (HttpResponseMessage response = typedResult.Response)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Null(response.Content);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_WithCreateException_HandlesWithCustomException()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage expectedRequest = CreateRequest())
            using (HttpResponseMessage originalResponse = CreateResponse())
            using (HttpConfiguration configuration = CreateConfiguration())
            {
                configuration.Services.Clear(typeof(IContentNegotiator));

                originalResponse.Content = new StringContent("Error");
                expectedRequest.SetRequestContext(new HttpRequestContext
                {
                    IncludeErrorDetail = true,
                    Configuration = configuration
                });
                ExceptionHandlerContext context = CreateValidContext(expectedRequest,
                    "HttpControllerHandler.WriteBufferedResponseContentAsync");
                context.ExceptionContext.Response = originalResponse;
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                Task task = product.HandleAsync(context, cancellationToken);
                task.WaitUntilCompleted();

                // Assert
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                IHttpActionResult result = context.Result;
                Assert.IsType(typeof(ResponseMessageResult), result);
                ResponseMessageResult typedResult = (ResponseMessageResult)result;
                using (HttpResponseMessage response = typedResult.Response)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                    Assert.Null(response.Content);
                    Assert.Same(expectedRequest, response.RequestMessage);
                }
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_AndResponseIsNull_LeavesExceptionUnhandled()
        {
            IExceptionHandler product = CreateProductUnderTest();

            // Arrange
            using (HttpRequestMessage request = CreateRequest())
            {
                ExceptionHandlerContext context = CreateValidContext(request,
                    "HttpControllerHandler.WriteBufferedResponseContentAsync");
                Assert.Null(context.ExceptionContext.Response); // Guard
                CancellationToken cancellationToken = CancellationToken.None;

                // Act
                Task task = product.HandleAsync(context, cancellationToken);
                task.WaitUntilCompleted();

                // Assert
                Assert.Equal(TaskStatus.RanToCompletion, task.Status);
                Assert.Null(context.Result);
            }
        }

        [Fact]
        public void HandleAsync_IfCatchBlockIsWebHostBufferedContent_AndResponseContentIsNull_Throws()
        {
            // Arrange
            IExceptionHandler product = CreateProductUnderTest();

            using (HttpRequestMessage request = CreateRequest())
            using (HttpResponseMessage response = CreateResponse())
            {
                ExceptionHandlerContext context = CreateValidContext(request,
                    "HttpControllerHandler.WriteBufferedResponseContentAsync");
                context.ExceptionContext.Response = response;
                Assert.Null(context.ExceptionContext.Request.Content); // Guard
                CancellationToken cancellationToken = CancellationToken.None;

                // Act & Assert
                Assert.ThrowsArgument(() => product.HandleAsync(context, cancellationToken), "context",
                    "HttpResponseMessage.Content must not be null.");
            }
        }

        private static void AssertErrorResponse(HttpResponseMessage expected, HttpResponseMessage actual)
        {
            Assert.NotNull(expected); // Guard
            Assert.IsType(typeof(ObjectContent<HttpError>), expected.Content); // Guard
            ObjectContent<HttpError> expectedContent = (ObjectContent<HttpError>)expected.Content;
            Assert.NotNull(expectedContent.Formatter); // Guard

            Assert.NotNull(actual);
            Assert.Equal(expected.StatusCode, actual.StatusCode);
            Assert.IsType(typeof(ObjectContent<HttpError>), actual.Content);
            ObjectContent<HttpError> actualContent = (ObjectContent<HttpError>)actual.Content;
            Assert.NotNull(actualContent.Formatter);
            Assert.Same(expectedContent.Formatter.GetType(), actualContent.Formatter.GetType());
            Assert.Equal(Flatten(expectedContent.Value), Flatten(actualContent.Value));
            Assert.Same(expected.RequestMessage, actual.RequestMessage);
        }

        private static object Flatten(object obj)
        {
            IDictionary<string, object> dictionary = obj as IDictionary<string, object>;

            if (dictionary == null)
            {
                return obj;
            }

            IDictionary<string, object> flattened = new Dictionary<string, object>();
            AddValues(dictionary, null, flattened);
            return flattened;
        }

        private static void AddValues(IDictionary<string, object> source, string prefix,
            IDictionary<string, object> destination)
        {
            foreach (string key in source.Keys)
            {
                object value = source[key];
                IDictionary<string, object> dictionaryValue = value as IDictionary<string, object>;

                string prefixedKey = prefix != null ? prefix + "." + key : key;

                if (dictionaryValue != null)
                {
                    destination.Add(prefixedKey, "<Flattened>");
                    AddValues(dictionaryValue, prefixedKey, destination);
                }
                else
                {
                    destination.Add(prefixedKey, value);
                }
            }
        }

        private static HttpConfiguration CreateConfiguration()
        {
            return new HttpConfiguration();
        }

        private static Exception CreateException()
        {
            return new NotSupportedException();
        }

        private static DefaultExceptionHandler CreateProductUnderTest()
        {
            return new DefaultExceptionHandler();
        }

        private static HttpRequestMessage CreateRequest()
        {
            return new HttpRequestMessage();
        }

        private static HttpResponseMessage CreateResponse()
        {
            return new HttpResponseMessage();
        }

        private static ExceptionHandlerContext CreateValidContext(HttpRequestMessage request)
        {
            return new ExceptionHandlerContext
            {
                ExceptionContext = new ExceptionContext
                {
                    Exception = CreateException(),
                    Request = request
                }
            };
        }

        private static ExceptionHandlerContext CreateValidContext(HttpRequestMessage request, string catchBlock)
        {
            return new ExceptionHandlerContext
            {
                ExceptionContext = new ExceptionContext
                {
                    Exception = CreateException(),
                    Request = request,
                    CatchBlock = catchBlock
                }
            };
        }
    }
}
