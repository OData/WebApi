using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http
{
    public class HttpRequestMessageExtensionsTest
    {
        private readonly HttpRequestMessage _request = new HttpRequestMessage();
        private readonly HttpConfiguration _config = new HttpConfiguration();
        private readonly object _value = new object();
        private readonly Mock<IDisposable> _disposableMock = new Mock<IDisposable>();
        private readonly Mock<IContentNegotiator> _negotiatorMock = new Mock<IContentNegotiator>();
        private readonly IDisposable _disposable;

        public HttpRequestMessageExtensionsTest()
        {
            _disposable = _disposableMock.Object;
        }

        [Fact]
        public void IsCorrectType()
        {
            Assert.Type.HasProperties(typeof(HttpRequestMessageExtensions), TypeAssert.TypeProperties.IsStatic | TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Fact]
        public void GetConfigurationThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpRequestMessageExtensions.GetConfiguration(null), "request");
        }

        [Fact]
        public void GetConfiguration()
        {
            // Arrange
            _request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, _config);

            // Act
            HttpConfiguration afterConfig = _request.GetConfiguration();

            // Assert
            Assert.Same(_config, afterConfig);
        }

        [Fact]
        public void GetSynchronizationContextThrowsOnNull()
        {
            Assert.ThrowsArgumentNull(() => HttpRequestMessageExtensions.GetSynchronizationContext(null), "request");
        }

        [Fact]
        public void GetSynchronizationContext()
        {
            // Arrange
            Mock<SynchronizationContext> syncContextMock = new Mock<SynchronizationContext>();
            SynchronizationContext beforeSyncContext = syncContextMock.Object;
            _request.Properties.Add(HttpPropertyKeys.SynchronizationContextKey, beforeSyncContext);

            // Act
            SynchronizationContext afterSyncContext = _request.GetSynchronizationContext();

            // Assert
            Assert.Same(beforeSyncContext, afterSyncContext);
        }

        [Fact]
        public void GetRouteData()
        {
            // Arrange
            IHttpRouteData routeData = new Mock<IHttpRouteData>().Object;
            _request.Properties.Add(HttpPropertyKeys.HttpRouteDataKey, routeData);

            // Act
            var httpRouteData = _request.GetRouteData();

            // Assert
            Assert.Same(routeData, httpRouteData);
        }

        [Fact]
        public void CreateResponse_OnNullRequest_ThrowsException()
        {
            Assert.ThrowsArgumentNull(() =>
            {
                HttpRequestMessageExtensions.CreateResponse(null, HttpStatusCode.OK, _value);
            }, "request");

            Assert.ThrowsArgumentNull(() =>
            {
                HttpRequestMessageExtensions.CreateResponse(null, HttpStatusCode.OK, _value, configuration: null);
            }, "request");
        }

        [Fact]
        public void CreateResponse_OnNullConfiguration_ThrowsException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                HttpRequestMessageExtensions.CreateResponse(_request, HttpStatusCode.OK, _value, configuration: null);
            }, "The request does not have an associated configuration object or the provided configuration was null.");
        }

        [Fact]
        public void CreateResponse_RetrievesContentNegotiatorFromServiceResolver()
        {
            // Arrange
            Mock<IDependencyResolver> resolverMock = new Mock<IDependencyResolver>();
            _config.ServiceResolver.SetResolver(resolverMock.Object);

            // Act
            HttpRequestMessageExtensions.CreateResponse(_request, HttpStatusCode.OK, _value, _config);

            // Assert
            resolverMock.Verify(r => r.GetService(typeof(IContentNegotiator)), Times.Once());
        }

        [Fact]
        public void CreateResponse_WhenNoContentNegotiatorInstanceRegistered_Throws()
        {
            // Arrange
            _config.ServiceResolver.SetServices(typeof(IContentNegotiator), new object[] { null });

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => HttpRequestMessageExtensions.CreateResponse(_request, HttpStatusCode.OK, _value, _config),
                "The provided configuration does not have an instance of the 'System.Net.Http.Formatting.IContentNegotiator' service registered.");
        }

        [Fact]
        public void CreateResponse_WhenContentNegotiatorReturnsNullResult_Throws()
        {
            // Arrange
            _negotiatorMock.Setup(r => r.Negotiate(typeof(string), _request, _config.Formatters)).Returns(value: null);
            _config.ServiceResolver.SetServices(typeof(IContentNegotiator), _negotiatorMock.Object);

            // Act
            var response = HttpRequestMessageExtensions.CreateResponse<string>(_request, HttpStatusCode.OK, "", _config);

            // Assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
            Assert.Same(_request, response.RequestMessage);
        }

        [Fact]
        public void CreateResponse_PerformsContentNegotiationAndCreatesContentUsingResults()
        {
            // Arrange
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            _negotiatorMock.Setup(r => r.Negotiate(typeof(string), _request, _config.Formatters))
                        .Returns(new ContentNegotiationResult(formatter, null));
            _config.ServiceResolver.SetService(typeof(IContentNegotiator), _negotiatorMock.Object);

            // Act
            var response = HttpRequestMessageExtensions.CreateResponse<string>(_request, HttpStatusCode.NoContent, "42", _config);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Same(_request, response.RequestMessage);
            var objectContent = Assert.IsType<ObjectContent<string>>(response.Content);
            Assert.Equal("42", objectContent.Value);
            Assert.Same(formatter, objectContent.Formatter);
        }

        [Fact]
        public void RegisterForDispose_WhenRequestParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => HttpRequestMessageExtensions.RegisterForDispose(request: null, resource: null), "request");
        }

        [Fact]
        public void RegisterForDispose_WhenResourceParamterIsNull_DoesNothing()
        {
            _request.RegisterForDispose(resource: null);

            Assert.False(_request.Properties.ContainsKey(HttpPropertyKeys.DisposableRequestResourcesKey));
        }

        [Fact]
        public void RegisterForDispose_WhenResourceListDoesNotExist_CreatesListAndAddsResource()
        {
            _request.Properties.Remove(HttpPropertyKeys.DisposableRequestResourcesKey);

            _request.RegisterForDispose(_disposable);

            var list = Assert.IsType<List<IDisposable>>(_request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey]);
            Assert.Equal(1, list.Count);
            Assert.Same(_disposable, list[0]);
        }

        [Fact]
        public void RegisterForDispose_WhenResourceListExists_AddsResource()
        {
            var list = new List<IDisposable>();
            _request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey] = list;

            _request.RegisterForDispose(_disposable);

            Assert.Same(list, _request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey]);
            Assert.Equal(1, list.Count);
            Assert.Same(_disposable, list[0]);
        }

        [Fact]
        public void DisposeRequestResources_WhenRequestParameterIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(
                () => HttpRequestMessageExtensions.DisposeRequestResources(request: null), "request");
        }

        [Fact]
        public void DisposeRequestResources_WhenResourceListDoesNotExists_DoesNothing()
        {
            _request.Properties.Remove(HttpPropertyKeys.DisposableRequestResourcesKey);

            _request.DisposeRequestResources();

            Assert.False(_request.Properties.ContainsKey(HttpPropertyKeys.DisposableRequestResourcesKey));
        }

        [Fact]
        public void DisposeRequestResources_WhenResourceListExists_DisposesResourceAndClearsReferences()
        {
            var list = new List<IDisposable> { _disposable };
            _request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey] = list;

            _request.DisposeRequestResources();

            _disposableMock.Verify(d => d.Dispose());
            Assert.Empty(list);
        }

        [Fact]
        public void DisposeRequestResources_WhenResourcesDisposeMethodThrowsException_IgnoresExceptionsAndContinuesDisposingOtherResources()
        {
            Mock<IDisposable> throwingDisposableMock = new Mock<IDisposable>();
            throwingDisposableMock.Setup(d => d.Dispose()).Throws(new Exception());
            var list = new List<IDisposable> { throwingDisposableMock.Object, _disposable };
            _request.Properties[HttpPropertyKeys.DisposableRequestResourcesKey] = list;

            _request.DisposeRequestResources();

            throwingDisposableMock.Verify(d => d.Dispose());
            _disposableMock.Verify(d => d.Dispose());
            Assert.Empty(list);
        }
    }
}
