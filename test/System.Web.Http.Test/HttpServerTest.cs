// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Dispatcher;
using Microsoft.TestCommon;
using Moq;
using Moq.Protected;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class HttpServerTest
    {
        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties<HttpServer, DelegatingHandler>(TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsDisposable);
        }

        [Fact]
        public void DefaultConstructor()
        {
            Assert.NotNull(new HttpServer());
        }

        [Fact]
        public void ConstructorConfigThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => new HttpServer((HttpConfiguration)null), "configuration");
        }

        [Fact]
        public void ConstructorConfigSetsUpProperties()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();

            // Act
            HttpServer server = new HttpServer(config);

            // Assert
            Assert.Same(config, server.Configuration);
        }

        [Fact]
        public void ConstructorDispatcherThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => new HttpServer((HttpControllerDispatcher)null), "dispatcher");
        }

        [Fact]
        public void ConstructorDispatcherSetsUpProperties()
        {
            // Arrange
            Mock<HttpControllerDispatcher> controllerDispatcherMock = new Mock<HttpControllerDispatcher>();

            // Act
            HttpServer server = new HttpServer(controllerDispatcherMock.Object);

            // Assert
            Assert.Same(controllerDispatcherMock.Object, server.Dispatcher);
        }

        [Fact]
        public void ConstructorThrowsOnNull()
        {
            Mock<HttpControllerDispatcher> controllerDispatcherMock = new Mock<HttpControllerDispatcher>();
            Assert.ThrowsArgumentNull(() => new HttpServer((HttpConfiguration)null, controllerDispatcherMock.Object), "configuration");
            Assert.ThrowsArgumentNull(() => new HttpServer(new HttpConfiguration(), null), "dispatcher");
        }

        [Fact]
        public void ConstructorSetsUpProperties()
        {
            // Arrange
            HttpConfiguration config = new HttpConfiguration();
            Mock<HttpControllerDispatcher> controllerDispatcherMock = new Mock<HttpControllerDispatcher>();

            // Act
            HttpServer server = new HttpServer(config, controllerDispatcherMock.Object);

            // Assert
            Assert.Same(config, server.Configuration);
            Assert.Same(controllerDispatcherMock.Object, server.Dispatcher);
        }

        [Fact]
        public Task<HttpResponseMessage> DisposedReturnsServiceUnavailable()
        {
            // Arrange
            Mock<HttpControllerDispatcher> dispatcherMock = new Mock<HttpControllerDispatcher>();
            HttpServer server = new HttpServer(dispatcherMock.Object);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);
            server.Dispose();
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            return invoker.SendAsync(request, CancellationToken.None).ContinueWith(
                (reqTask) =>
                {
                    // Assert
                    dispatcherMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Never(), request, CancellationToken.None);
                    Assert.Equal(HttpStatusCode.ServiceUnavailable, reqTask.Result.StatusCode);
                    return reqTask.Result;
                }
            );
        }

        [Fact]
        public Task<HttpResponseMessage> RequestGetsConfigurationAsParameter()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            Mock<HttpControllerDispatcher> dispatcherMock = new Mock<HttpControllerDispatcher>();
            dispatcherMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", request, CancellationToken.None).
                Returns(TaskHelpers.FromResult<HttpResponseMessage>(request.CreateResponse()));

            HttpConfiguration config = new HttpConfiguration();
            HttpServer server = new HttpServer(config, dispatcherMock.Object);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            // Act
            return invoker.SendAsync(request, CancellationToken.None).ContinueWith(
                (reqTask) =>
                {
                    // Assert
                    dispatcherMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), request, CancellationToken.None);
                    Assert.Same(config, request.GetConfiguration());
                    return reqTask.Result;
                }
            );
        }

        [Fact]
        public Task<HttpResponseMessage> RequestGetsSyncContextAsParameter()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            Mock<HttpControllerDispatcher> dispatcherMock = new Mock<HttpControllerDispatcher>();
            dispatcherMock.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", request, CancellationToken.None).
                Returns(TaskHelpers.FromResult<HttpResponseMessage>(request.CreateResponse()));

            HttpConfiguration config = new HttpConfiguration();
            HttpServer server = new HttpServer(config, dispatcherMock.Object);
            HttpMessageInvoker invoker = new HttpMessageInvoker(server);

            SynchronizationContext syncContext = new SynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(syncContext);

            // Act
            return invoker.SendAsync(request, CancellationToken.None).ContinueWith(
                (reqTask) =>
                {
                    // Assert
                    dispatcherMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Once(), request, CancellationToken.None);
                    Assert.Same(syncContext, request.GetSynchronizationContext());
                    return reqTask.Result;
                }
            );
        }

        [Fact, RestoreThreadPrincipal]
        public Task SendAsync_SetsGenericPrincipalWhenThreadPrincipalIsNullAndCleansUpAfterward()
        {
            // Arrange
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            var dispatcherMock = new Mock<HttpControllerDispatcher>();
            var server = new HttpServer(config, dispatcherMock.Object);
            var invoker = new HttpMessageInvoker(server);
            IPrincipal callbackPrincipal = null;
            Thread.CurrentPrincipal = null;
            dispatcherMock.Protected()
                          .Setup<Task<HttpResponseMessage>>("SendAsync", request, CancellationToken.None)
                          .Callback(() => callbackPrincipal = Thread.CurrentPrincipal)
                          .Returns(TaskHelpers.FromResult<HttpResponseMessage>(request.CreateResponse()));

            // Act
            return invoker.SendAsync(request, CancellationToken.None)
                          .ContinueWith(req =>
                          {
                              // Assert
                              Assert.NotNull(callbackPrincipal);
                              Assert.False(callbackPrincipal.Identity.IsAuthenticated);
                              Assert.Empty(callbackPrincipal.Identity.Name);
                              Assert.Null(Thread.CurrentPrincipal);
                          });
        }

        [Fact, RestoreThreadPrincipal]
        public Task SendAsync_DoesNotChangeExistingThreadPrincipal()
        {
            // Arrange
            var config = new HttpConfiguration();
            var request = new HttpRequestMessage();
            var dispatcherMock = new Mock<HttpControllerDispatcher>();
            var server = new HttpServer(config, dispatcherMock.Object);
            var invoker = new HttpMessageInvoker(server);
            var principal = new GenericPrincipal(new GenericIdentity("joe"), new string[0]);
            Thread.CurrentPrincipal = principal;
            IPrincipal callbackPrincipal = null;
            dispatcherMock.Protected()
                          .Setup<Task<HttpResponseMessage>>("SendAsync", request, CancellationToken.None)
                          .Callback(() => callbackPrincipal = Thread.CurrentPrincipal)
                          .Returns(TaskHelpers.FromResult<HttpResponseMessage>(request.CreateResponse()));

            // Act
            return invoker.SendAsync(request, CancellationToken.None)
                          .ContinueWith(req =>
                          {
                              // Assert
                              Assert.Same(principal, callbackPrincipal);
                              Assert.Same(principal, Thread.CurrentPrincipal);
                          });
        }
    }
}
