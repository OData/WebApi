// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.WebHost.Routing
{
    public class HttpWebRouteTests
    {
        [Fact]
        public void GetRouteData_IfHttpRouteGetRouteDataThrows_CallsExceptionServices()
        {
            // Arrange
            Exception expectedException = CreateException();
            IHttpRoute route = CreateThrowingRoute(expectedException);

            HttpWebRoute product = CreateProductUnderTest(route);

            HttpRequestBase requestBase = CreateStubRequestBase("GET");
            HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
            HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);

            Mock<IExceptionLogger> loggerMock = CreateStubExceptionLoggerMock();
            IExceptionLogger logger = loggerMock.Object;
            Mock<IExceptionHandler> handlerMock = CreateStubExceptionHandlerMock();
            IExceptionHandler handler = handlerMock.Object;

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            {
                contextBase.SetHttpRequestMessage(expectedRequest);

                // Act
                Exception exception = Assert.Throws(expectedException.GetType(),
                    () => product.GetRouteData(contextBase, logger, handler));

                // Assert
                Assert.Same(expectedException, exception);

                string expectedCatchBlock = WebHostExceptionCatchBlocks.HttpWebRoute;
                Func<ExceptionContext, bool> exceptionContextMatches = (c) =>
                    c != null
                    && c.Exception == expectedException
                    && c.CatchBlock == expectedCatchBlock
                    && c.IsTopLevelCatchBlock == true
                    && c.Request == expectedRequest;

                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.CanBeHandled == true && exceptionContextMatches(c.ExceptionContext)),
                    CancellationToken.None), Times.Once());
                handlerMock.Verify(l => l.HandleAsync(It.Is<ExceptionHandlerContext>(c =>
                    exceptionContextMatches(c.ExceptionContext)), CancellationToken.None), Times.Once());
            }
        }

        [Fact]
        public void GetRouteData_IfHttpRouteGetRouteDataThrowsAndHandlerHandles_UsesHandlerResult()
        {
            // Arrange
            HttpStatusCode expectedStatusCode = HttpStatusCode.Ambiguous;
            IHttpRoute route = CreateThrowingRoute(CreateException());

            HttpWebRoute product = CreateProductUnderTest(route);

            HttpRequestBase requestBase = CreateStubRequestBase("GET");
            int statusCode = 0;
            Mock<HttpResponseBase> responseBaseMock = new Mock<HttpResponseBase>();
            responseBaseMock.Setup(r => r.OutputStream).Returns(Stream.Null);
            responseBaseMock.SetupSet(r => r.StatusCode = It.IsAny<int>()).Callback<int>((c) => statusCode = c);
            HttpResponseBase responseBase = responseBaseMock.Object;
            HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);

            IExceptionLogger logger = CreateStubExceptionLogger();
            Mock<IExceptionHandler> handlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            handlerMock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                {
                    c.Result = new StatusCodeResult(expectedStatusCode, new HttpRequestMessage());
                    return Task.FromResult(0);
                });
            IExceptionHandler handler = handlerMock.Object;

            // Act
            product.GetRouteData(contextBase, logger, handler);

            // Assert
            Assert.Equal((int)expectedStatusCode, statusCode);

        }

        [Fact]
        public void GetRouteData_IfHttpRouteGetRouteDataThrowsAndHandlerDoesNotHandle_PropagatesException()
        {
            // Arrange
            Exception expectedException = CreateException();

            IHttpRoute route = CreateThrowingRoute(expectedException);

            HttpWebRoute product = CreateProductUnderTest(route);

            HttpRequestBase requestBase = CreateStubRequestBase("GET");
            HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
            HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);

            IExceptionLogger logger = CreateStubExceptionLogger();
            IExceptionHandler handler = CreateStubExceptionHandler();

            // Act
            Exception exception = Assert.Throws(expectedException.GetType(), () => product.GetRouteData(
                contextBase, logger, handler));

            // Assert
            Assert.Same(expectedException, exception);
            Assert.NotNull(exception.StackTrace);
            string[] stackTraceLines = exception.StackTrace.Split(new string[] { Environment.NewLine },
                StringSplitOptions.RemoveEmptyEntries);
            Assert.True(stackTraceLines.Any(l => l.TrimStart().StartsWith(
                "at Castle.Proxies.IHttpRouteProxy.GetRouteData")));
        }

        [Fact]
        public void GetRouteData_IfCopyToAsyncOnErrorResponseThrows_CallsExceptionLogger()
        {
            // Arrange
            Exception expectedOriginalException = CreateException();
            IHttpRoute route = CreateThrowingRoute(expectedOriginalException);

            HttpWebRoute product = CreateProductUnderTest(route);

            HttpRequestBase requestBase = CreateStubRequestBase("GET");
            HttpResponseBase responseBase = CreateStubResponseBase(Stream.Null);
            HttpContextBase contextBase = CreateStubContextBase(requestBase, responseBase);

            Exception expectedErrorException = CreateException();

            using (HttpRequestMessage expectedRequest = new HttpRequestMessage())
            using (HttpResponseMessage expectedErrorResponse = new HttpResponseMessage())
            {
                contextBase.SetHttpRequestMessage(expectedRequest);
                expectedErrorResponse.Content = CreateFaultingContent(expectedErrorException);

                Mock<IExceptionLogger> loggerMock = CreateStubExceptionLoggerMock();
                IExceptionLogger logger = loggerMock.Object;
                Mock<IExceptionHandler> handlerMock = new Mock<IExceptionHandler>(MockBehavior.Strict);
                handlerMock
                    .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                    .Returns<ExceptionHandlerContext, CancellationToken>((c, i) =>
                    {
                        c.Result = new ResponseMessageResult(expectedErrorResponse);
                        return Task.FromResult(0);
                    });
                IExceptionHandler handler = handlerMock.Object;

                // Act
                product.GetRouteData(contextBase, logger, handler);

                // Assert
                string expectedOriginalCatchBlock = WebHostExceptionCatchBlocks.HttpWebRoute;
                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.CanBeHandled == true
                    && c.ExceptionContext != null
                    && c.ExceptionContext.Exception == expectedOriginalException
                    && c.ExceptionContext.CatchBlock == expectedOriginalCatchBlock
                    && c.ExceptionContext.IsTopLevelCatchBlock == true
                    && c.ExceptionContext.Request == expectedRequest),
                    CancellationToken.None), Times.Once());
                string expectedErrorCatchBlock =
                    WebHostExceptionCatchBlocks.HttpControllerHandlerWriteErrorResponseContentAsync;
                loggerMock.Verify(l => l.LogAsync(It.Is<ExceptionLoggerContext>(c =>
                    c.CanBeHandled == false
                    && c.ExceptionContext != null
                    && c.ExceptionContext.Exception == expectedErrorException
                    && c.ExceptionContext.CatchBlock == expectedErrorCatchBlock
                    && c.ExceptionContext.IsTopLevelCatchBlock == true
                    && c.ExceptionContext.Request == expectedRequest
                    && c.ExceptionContext.Response == expectedErrorResponse),
                    CancellationToken.None), Times.Once());
            }
        }

        private static Exception CreateException()
        {
            return new NotFiniteNumberException();
        }

        private static Exception CreateExceptionWithCallStack()
        {
            try
            {
                throw CreateException();
            }
            catch (Exception exception)
            {
                return exception;
            }
        }

        private static HttpContent CreateFaultingContent(Exception exception)
        {
            return new FaultingHttpContent(exception);
        }

        private static IExceptionHandler CreateStubExceptionHandler()
        {
            return CreateStubExceptionHandlerMock().Object;
        }

        private static Mock<IExceptionHandler> CreateStubExceptionHandlerMock()
        {
            Mock<IExceptionHandler> mock = new Mock<IExceptionHandler>(MockBehavior.Strict);
            mock
                .Setup(h => h.HandleAsync(It.IsAny<ExceptionHandlerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }

        private static IExceptionLogger CreateStubExceptionLogger()
        {
            return CreateStubExceptionLoggerMock().Object;
        }

        private static Mock<IExceptionLogger> CreateStubExceptionLoggerMock()
        {
            Mock<IExceptionLogger> mock = new Mock<IExceptionLogger>(MockBehavior.Strict);
            mock
                .Setup(l => l.LogAsync(It.IsAny<ExceptionLoggerContext>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(0));
            return mock;
        }

        private static HttpWebRoute CreateProductUnderTest(IHttpRoute httpRoute)
        {
            return new HttpWebRoute(null, null, null, null, null, httpRoute);
        }

        private static HttpContextBase CreateStubContextBase(HttpRequestBase request, HttpResponseBase response)
        {
            Mock<HttpContextBase> contextMock = new Mock<HttpContextBase>();
            contextMock.SetupGet(m => m.Request).Returns(request);
            contextMock.SetupGet(m => m.Response).Returns(response);
            IDictionary items = new Dictionary<object, object>();
            contextMock.SetupGet(m => m.Items).Returns(items);
            return contextMock.Object;
        }

        private static HttpRequestBase CreateStubRequestBase(string httpMethod)
        {
            Mock<HttpRequestBase> mock = new Mock<HttpRequestBase>();
            mock.SetupGet(m => m.HttpMethod).Returns(httpMethod);
            mock.SetupGet(m => m.Headers).Returns(new NameValueCollection());
            return mock.Object;
        }

        private static HttpResponseBase CreateStubResponseBase(Stream outputStream)
        {
            Mock<HttpResponseBase> responseBaseMock = new Mock<HttpResponseBase>();
            responseBaseMock.Setup(r => r.OutputStream).Returns(outputStream);
            return responseBaseMock.Object;
        }

        private static IHttpRoute CreateThrowingRoute(Exception exception)
        {
            Mock<IHttpRoute> mock = new Mock<IHttpRoute>(MockBehavior.Strict);
            mock
                .Setup(m => m.GetRouteData(It.IsAny<string>(), It.IsAny<HttpRequestMessage>()))
                .Throws(exception);
            return mock.Object;
        }

        private static IHttpRoute CreateThrowingRoute(ExceptionDispatchInfo exceptionInfo)
        {
            Mock<IHttpRoute> mock = new Mock<IHttpRoute>(MockBehavior.Strict);
            mock
                .Setup(m => m.GetRouteData(It.IsAny<string>(), It.IsAny<HttpRequestMessage>()))
                .Callback<string, HttpRequestMessage>((i1, i2) =>
                {
                    exceptionInfo.Throw();
                });
            return mock.Object;
        }

        private class FaultingHttpContent : HttpContent
        {
            private readonly Exception _exception;

            public FaultingHttpContent(Exception exception)
            {
                Contract.Assert(exception != null);
                _exception = exception;
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
            {
                return CreateFaultedTask(_exception);
            }

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }

            private static Task CreateFaultedTask(Exception exception)
            {
                TaskCompletionSource<object> source = new TaskCompletionSource<object>();
                source.SetException(exception);
                return source.Task;
            }
        }
    }
}
