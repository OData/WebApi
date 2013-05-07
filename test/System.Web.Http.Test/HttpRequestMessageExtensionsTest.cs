// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Hosting;
using System.Web.Http.Routing;
using System.Web.Http.Services;
using Microsoft.TestCommon;
using Moq;

namespace System.Net.Http
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
            _request.Properties[HttpPropertyKeys.HttpConfigurationKey] = _config;
        }

        [Fact]
        public void GetConfigurationThrowsOnNull()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.GetConfiguration(), "request");
        }

        [Fact]
        public void GetConfiguration()
        {
            // Arrange
            _request.Properties[HttpPropertyKeys.HttpConfigurationKey] = _config;

            // Act
            HttpConfiguration afterConfig = _request.GetConfiguration();

            // Assert
            Assert.Same(_config, afterConfig);
        }

        [Fact]
        public void SetConfiguration_ThrowsArgumentNull_Request()
        {
            HttpRequestMessage request = null;

            Assert.ThrowsArgumentNull(
                () => request.SetConfiguration(_config),
                "request");
        }

        [Fact]
        public void SetConfiguration_ThrowsArgumentNull_Configuration()
        {
            Assert.ThrowsArgumentNull(
                () => _request.SetConfiguration(null),
                "configuration");
        }

        [Fact]
        public void GetSynchronizationContextThrowsOnNull()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.GetSynchronizationContext(), "request");
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
        public void GetRouteData_WhenRequestIsNull_Throws()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act
            Assert.ThrowsArgumentNull(() => request.GetRouteData(), "request");
        }

        [Fact]
        public void SetRouteData_ThrowsArgumentNull_Request()
        {
            HttpRequestMessage request = null;

            Assert.ThrowsArgumentNull(
                () => request.SetRouteData(routeData: new HttpRouteData(new HttpRoute())),
                "request");
        }

        [Fact]
        public void SetRouteData_ThrowsArgumentNull_RouteData()
        {
            Assert.ThrowsArgumentNull(
                () => _request.SetRouteData(null),
                "routeData");
        }

        [Fact]
        public void CreateResponse_DoingConneg_OnNullRequest_ThrowsException()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() =>
            {
                request.CreateResponse(_value);
            }, "request");

            Assert.ThrowsArgumentNull(() =>
            {
                request.CreateResponse(HttpStatusCode.OK, _value);
            }, "request");

            Assert.ThrowsArgumentNull(() =>
            {
                request.CreateResponse(HttpStatusCode.OK, _value, configuration: null);
            }, "request");
        }

        [Fact]
        public void CreateResponse_DoingConneg_OnNullConfiguration_ThrowsException()
        {
            _request.Properties[HttpPropertyKeys.HttpConfigurationKey] = null;

            Assert.Throws<InvalidOperationException>(() =>
            {
                _request.CreateResponse(_value);
            }, "The request does not have an associated configuration object or the provided configuration was null.");

            Assert.Throws<InvalidOperationException>(() =>
            {
                _request.CreateResponse(HttpStatusCode.OK, _value, configuration: null);
            }, "The request does not have an associated configuration object or the provided configuration was null.");
        }

        [Fact]
        public void CreateResponse_DoingConneg_OnlyContent_RetrievesContentNegotiatorFromServiceResolver()
        {
            // Arrange
            Mock<DefaultServices> servicesMock = new Mock<DefaultServices> { CallBase = true };
            servicesMock.Setup(s => s.GetService(typeof(IContentNegotiator)))
                        .Returns(new Mock<IContentNegotiator>().Object)
                        .Verifiable();
            _config.Services = servicesMock.Object;

            // Act
            _request.CreateResponse(_value);

            // Assert
            servicesMock.Verify();
        }

        [Fact]
        public void CreateResponse_DoingConneg_RetrievesContentNegotiatorFromServiceResolver()
        {
            // Arrange
            Mock<DefaultServices> servicesMock = new Mock<DefaultServices> { CallBase = true };
            servicesMock.Setup(s => s.GetService(typeof(IContentNegotiator)))
                        .Returns(new Mock<IContentNegotiator>().Object)
                        .Verifiable();
            _config.Services = servicesMock.Object;

            // Act
            _request.CreateResponse(HttpStatusCode.OK, _value, _config);

            // Assert
            servicesMock.Verify();
        }

        [Fact]
        public void CreateResponse_DoingConneg_WhenNoContentNegotiatorInstanceRegistered_Throws()
        {
            // Arrange
            _config.Services.Clear(typeof(IContentNegotiator));

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _request.CreateResponse(_value),
                "The provided configuration does not have an instance of the 'System.Net.Http.Formatting.IContentNegotiator' service registered.");

            Assert.Throws<InvalidOperationException>(() => _request.CreateResponse(HttpStatusCode.OK, _value, _config),
                "The provided configuration does not have an instance of the 'System.Net.Http.Formatting.IContentNegotiator' service registered.");
        }

        [Fact]
        public void CreateResponse_DoingConneg_WhenContentNegotiatorReturnsNullResult_Throws()
        {
            // Arrange
            _negotiatorMock.Setup(r => r.Negotiate(typeof(string), _request, _config.Formatters)).Returns(value: null);
            _config.Services.Replace(typeof(IContentNegotiator), _negotiatorMock.Object);

            // Act
            var response = _request.CreateResponse<string>(HttpStatusCode.OK, "", _config);

            // Assert
            Assert.Equal(HttpStatusCode.NotAcceptable, response.StatusCode);
            Assert.Same(_request, response.RequestMessage);
        }

        [Fact]
        public void CreateResponse_DoingConneg_PerformsContentNegotiationAndCreatesContentUsingResults()
        {
            // Arrange
            XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();
            _negotiatorMock.Setup(r => r.Negotiate(typeof(string), _request, _config.Formatters))
                        .Returns(new ContentNegotiationResult(formatter, null));
            _config.Services.Replace(typeof(IContentNegotiator), _negotiatorMock.Object);

            // Act
            var response = _request.CreateResponse<string>(HttpStatusCode.NoContent, "42", _config);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Same(_request, response.RequestMessage);
            var objectContent = Assert.IsType<ObjectContent<string>>(response.Content);
            Assert.Equal("42", objectContent.Value);
            Assert.Same(formatter, objectContent.Formatter);
        }

        //[Fact]
        //public void CreateResponse_Uses_Per_Controller_Services()
        //{
        //    // Arrange
        //    XmlMediaTypeFormatter formatter = new XmlMediaTypeFormatter();

        //    HttpConfiguration config = new HttpConfiguration();
        //    HttpControllerDescriptor cd = new HttpControllerDescriptor(config);

        //    cd.Configuration.Formatters.Clear();
        //    cd.Configuration.Formatters.Add(formatter);

        //    var negotiatorMock = new Mock<IContentNegotiator>();
        //    negotiatorMock.Setup(r => r.Negotiate(typeof(string), _request, cd.Configuration.Formatters))
        //                .Returns(new ContentNegotiationResult(formatter, null));

        //    cd.Configuration.Services.Replace(typeof(IContentNegotiator), negotiatorMock.Object);

        //    // Act. Default call uses the controller services if it can find it in the property bag.
        //    _request.Properties[HttpPropertyKeys.HttpControllerDescriptorKey] = cd;
        //    var response = _request.CreateResponse<string>(HttpStatusCode.NoContent, "42");

        //    // Assert
        //    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        //    Assert.Same(_request, response.RequestMessage);
        //    var objectContent = Assert.IsType<ObjectContent<string>>(response.Content);
        //    Assert.Equal("42", objectContent.Value);
        //    Assert.Same(formatter, objectContent.Formatter); // if this failed, then we didn't puck up the formatter from the controller.
        //}

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenRequestIsNull_Throws()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act
            Assert.ThrowsArgumentNull(() => request.CreateResponse(HttpStatusCode.OK, _value, "foo/bar"), "request");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeHeaderIsNull_Throws()
        {
            Assert.ThrowsArgumentNull(() => _request.CreateResponse(HttpStatusCode.OK, _value, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeStringIsNull_Throws()
        {
            Assert.ThrowsArgument(() => _request.CreateResponse(HttpStatusCode.OK, _value, (string)null), "mediaType");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeStringIsEmpty_Throws()
        {
            Assert.ThrowsArgumentNull(() => _request.CreateResponse(HttpStatusCode.OK, _value, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeStringIsInvalidFormat_Throws()
        {
            Assert.Throws<FormatException>(() => _request.CreateResponse(HttpStatusCode.OK, _value, "foo/bar; param=value"), "The format of value 'foo/bar; param=value' is invalid.");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenRequestDoesNotHaveConfiguration_Throws()
        {
            // Arrange
            _request.Properties[HttpPropertyKeys.HttpConfigurationKey] = null;

            // Act
            Assert.Throws<InvalidOperationException>(() => _request.CreateResponse(HttpStatusCode.OK, _value, mediaType: "foo/bar"),
                "The request does not have an associated configuration object or the provided configuration was null.");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_WhenMediaTypeDoesNotMatch_Throws()
        {
            // Arrange
            _request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();

            // Act
            Assert.Throws<InvalidOperationException>(() => _request.CreateResponse(HttpStatusCode.OK, _value, mediaType: "foo/bar"),
                "Could not find a formatter matching the media type 'foo/bar' that can write an instance of 'Object'.");
        }

        [Fact]
        public void CreateResponse_MatchingMediaType_FindsMatchingFormatterAndCreatesResponse()
        {
            // Arrange
            var config = new HttpConfiguration();
            _request.Properties[HttpPropertyKeys.HttpConfigurationKey] = config;
            config.Formatters.Clear();
            Mock<MediaTypeFormatter> formatterMock = new Mock<MediaTypeFormatter> { CallBase = true };
            var formatter = formatterMock.Object;
            formatterMock.Setup(f => f.CanWriteType(typeof(object))).Returns(true).Verifiable();
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("foo/bar"));
            config.Formatters.Add(formatter);

            // Act
            var response = _request.CreateResponse(HttpStatusCode.Gone, _value, mediaType: "foo/bar");

            // Assert
            Assert.Equal(HttpStatusCode.Gone, response.StatusCode);
            var content = Assert.IsType<ObjectContent<object>>(response.Content);
            Assert.Same(_value, content.Value);
            Assert.Same(formatter, content.Formatter);
            Assert.Equal("foo/bar", content.Headers.ContentType.MediaType);
            formatterMock.Verify();
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_WhenRequestIsNull_Throws()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act
            Assert.ThrowsArgumentNull(() => request.CreateResponse(HttpStatusCode.OK, _value, new MockMediaTypeFormatter()), "request");
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_WhenFormatterIsNull_Throws()
        {
            // Act
            Assert.ThrowsArgumentNull(() => _request.CreateResponse(HttpStatusCode.OK, _value, formatter: null), "formatter");
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_CreatesResponseWithDefaultMediaType()
        {
            // Arrange
            var formatter = new MockMediaTypeFormatter { CallBase = true };
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("foo/bar"));

            // Act
            var response = _request.CreateResponse(HttpStatusCode.MultipleChoices, _value, formatter, mediaType: (string)null);

            // Assert
            Assert.Equal(HttpStatusCode.MultipleChoices, response.StatusCode);
            var content = Assert.IsType<ObjectContent<object>>(response.Content);
            Assert.Same(_value, content.Value);
            Assert.Same(formatter, content.Formatter);
            Assert.Equal("foo/bar", content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_WithOverridenMediaTypeString_CreatesResponse()
        {
            // Arrange
            var formatter = new MockMediaTypeFormatter { CallBase = true };
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("foo/bar"));

            // Act
            var response = _request.CreateResponse(HttpStatusCode.MultipleChoices, _value, formatter, mediaType: "bin/baz");

            // Assert
            Assert.Equal("bin/baz", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void CreateResponse_AcceptingFormatter_WithOverridenMediaTypeHeader_CreatesResponse()
        {
            // Arrange
            var formatter = new MockMediaTypeFormatter { CallBase = true };
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("foo/bar"));

            // Act
            var response = _request.CreateResponse(HttpStatusCode.MultipleChoices, _value, formatter, mediaType: new MediaTypeHeaderValue("bin/baz"));

            // Assert
            Assert.Equal("bin/baz", response.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public void RegisterForDispose_WhenRequestParameterIsNull_Throws()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.RegisterForDispose(resource: null), "request");
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
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.DisposeRequestResources(), "request");
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

        [Fact]
        public void GetUrlHelper_WhenRequestParameterIsNull_Throws()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.GetUrlHelper(), "request");
        }

        [Fact]
        public void GetUrlHelper_ReturnsUrlHelper()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();

            UrlHelper urlHelper = request.GetUrlHelper();

            Assert.NotNull(urlHelper);
            Assert.Same(request, urlHelper.Request);
        }

        [Fact]
        public void GetUrlHelper_Caches_UrlHelperInstance()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();

            UrlHelper urlHelper1 = request.GetUrlHelper();
            UrlHelper urlHelper2 = request.GetUrlHelper();

            Assert.NotNull(urlHelper1);
            Assert.Same(urlHelper1, urlHelper2);
        }

        [Fact]
        public void SetUrlHelper_AndThen_GetUrlHelper_Returns_SameInstance()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            UrlHelper urlHelper = new UrlHelper();

            request.SetUrlHelper(urlHelper);
            var retrievedUrlHelper = request.GetUrlHelper();

            Assert.Same(urlHelper, retrievedUrlHelper);
        }

        [Fact]
        public void CreateErrorResponseRangeNotSatisfiable_ThrowsOnNullException()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            Assert.ThrowsArgumentNull(() => request.CreateErrorResponse(invalidByteRangeException: null), "invalidByteRangeException");
        }

        [Fact]
        public void CreateErrorResponseRangeNotSatisfiable_SetsCorrectStatusCodeAndContentRangeHeader()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            ContentRangeHeaderValue expectedContentRange = new ContentRangeHeaderValue(length: 128);
            InvalidByteRangeException invalidByteRangeException = new InvalidByteRangeException(expectedContentRange);

            // Act
            HttpResponseMessage response = request.CreateErrorResponse(invalidByteRangeException);

            // Assert
            Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
            Assert.Same(expectedContentRange, response.Content.Headers.ContentRange);
        }

        [Fact]
        public void IsLocal_When_Request_From_Local_Address()
        {
            // Arrange
            _request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => true));

            // Act
            bool isLocal = _request.IsLocal();

            // Assert
            Assert.True(isLocal);
        }

        [Fact]
        public void IsLocal_When_Request_Not_From_Local_Address()
        {
            // Arrange
            _request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => false));

            // Act
            bool isLocal = _request.IsLocal();

            // Assert
            Assert.False(isLocal);
        }

        [Fact]
        public void IsLocal_With_Property_Value_Null_Returns_False()
        {
            // Arrange
            _request.Properties.Add(HttpPropertyKeys.IsLocalKey, null);

            // Act
            bool isLocal = _request.IsLocal();

            // Assert
            Assert.False(isLocal);
        }

        [Fact]
        public void IsLocal_With_Property_Value_String_Returns_False()
        {
            // Arrange
            _request.Properties.Add(HttpPropertyKeys.IsLocalKey, "Test String");

            // Act
            bool isLocal = _request.IsLocal();

            // Assert
            Assert.False(isLocal);
        }

        [Fact]
        public void IsLocal_WhenRequestIsNull_Throws()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act and Assert
            Assert.ThrowsArgumentNull(() => request.IsLocal(), "request");
        }

        [Theory]
        [InlineData(IncludeErrorDetailPolicy.Default, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, null, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, null, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, true, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Default, false, false, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, null, true, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, null, false, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, true, null, true)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, true, true, true)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, true, false, true)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, false, null, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, false, true, false)]
        [InlineData(IncludeErrorDetailPolicy.LocalOnly, false, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Always, null, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, null, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, null, false, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, true, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, true, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, true, false, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, false, null, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, false, true, true)]
        [InlineData(IncludeErrorDetailPolicy.Always, false, false, true)]
        [InlineData(IncludeErrorDetailPolicy.Never, null, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, null, true, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, null, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, true, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, true, true, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, true, false, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, false, null, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, false, true, false)]
        [InlineData(IncludeErrorDetailPolicy.Never, false, false, false)]
        [InlineData(null, false, false, false)]
        public void ShouldIncludeErrorDetail(IncludeErrorDetailPolicy errorDetail, bool isLocal, bool includeErrorDetail, bool expectedResult)
        {
            // Arrange
            _config.IncludeErrorDetailPolicy = errorDetail;
            _request.Properties.Add(HttpPropertyKeys.IsLocalKey, new Lazy<bool>(() => isLocal));
            _request.Properties.Add(HttpPropertyKeys.IncludeErrorDetailKey, new Lazy<bool>(() => includeErrorDetail));

            // Act
            bool includeError = _request.ShouldIncludeErrorDetail();

            // Assert
            Assert.Equal(includeError, expectedResult);
        }

        [Fact]
        public void ShouldIncludeErrorDetail_Returns_False_WhenConfigIsNull_includeErrorDetail_Null()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();

            // Act
            bool includeError = request.ShouldIncludeErrorDetail();

            // Assert
            Assert.False(includeError);
        }

        [Fact]
        public void ShouldIncludeErrorDetail_Returns_Value_WhenConfigIsNull_includeErrorDetail_HasValue()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage();
            request.Properties.Add(HttpPropertyKeys.IncludeErrorDetailKey, new Lazy<bool>(() => true));

            // Act
            bool includeError = request.ShouldIncludeErrorDetail();

            // Assert
            Assert.True(includeError);
        }

        [Fact]
        public void ShouldIncludeErrorDetail_WhenRequestIsNull_Throws()
        {
            // Arrange
            HttpRequestMessage request = null;

            // Act and Assert
            Assert.ThrowsArgumentNull(() => request.ShouldIncludeErrorDetail(), "request");
        }

        [Fact]
        public void GetRoutingErrorResponse_ThrowsArgumentNull_Request()
        {
            HttpRequestMessage request = null;
            Assert.ThrowsArgumentNull(() => request.GetRoutingErrorResponse(), "request");
        }

        [Fact]
        public void SetRoutingErrorResponse_ThrowsArgumentNull_Request()
        {
            HttpRequestMessage request = null;
            HttpResponseMessage errorResponse = new HttpResponseMessage();

            Assert.ThrowsArgumentNull(() => request.SetRoutingErrorResponse(errorResponse), "request");
        }

        [Fact]
        public void SetRoutingErrorResponse_ThrowsArgumentNull_ErrorResponse()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            Assert.ThrowsArgumentNull(() => request.SetRoutingErrorResponse(errorResponse: null), "errorResponse");
        }

        [Fact]
        public void SetRoutingErrorResponse_AndThen_GetRoutingErrorResponse_Match()
        {
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage errorResponse = new HttpResponseMessage();

            request.SetRoutingErrorResponse(errorResponse);
            var result = request.GetRoutingErrorResponse();

            Assert.Same(errorResponse, result);
        }
    }
}
