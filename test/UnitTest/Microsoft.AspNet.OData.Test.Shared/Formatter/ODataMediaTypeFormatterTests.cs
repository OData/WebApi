// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
#else
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNet.OData.Test.Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ODataMediaTypeFormatterTests
    {
#if NETCORE
        private IEnumerable<string> ExpectedSupportedMediaTypes = new string[0];
#else
        private IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes = new MediaTypeHeaderValue[0];
#endif
        private IEnumerable<Encoding> ExpectedSupportedEncodings = new Encoding[0];
        private byte[] ExpectedSampleTypeByteRepresentation =
            Encoding.UTF8.GetBytes(
                "{" +
                    "\"@odata.context\":\"http://localhost/$metadata#sampleTypes/$entity\"," +
                    "\"@odata.type\":\"#Microsoft.AspNet.OData.Test.Formatter.SampleType\"," +
                    "\"@odata.id\":\"http://localhost/sampleTypes(42)\"," +
                    "\"@odata.editLink\":\"http://localhost/sampleTypes(42)\"," +
                    "\"Number\":42" +
                    "}");

        [Fact]
        public void TypeIsCorrect()
        {
#if NETCORE
            TypeAssert.HasProperties<ODataInputFormatter, TextInputFormatter>(TypeAssert.TypeProperties.IsPublicVisibleClass);
            TypeAssert.HasProperties<ODataOutputFormatter, TextOutputFormatter>(TypeAssert.TypeProperties.IsPublicVisibleClass);
#else
            TypeAssert.HasProperties<ODataMediaTypeFormatter, MediaTypeFormatter>(TypeAssert.TypeProperties.IsPublicVisibleClass);
#endif
        }

        [Fact]
        public void SupportedMediaTypes_HeaderValuesAreNotSharedBetweenInstances()
        {
            var formatter1 = CreateOutputFormatter();
            var formatter2 = CreateOutputFormatter();

            foreach (var mediaType1 in formatter1.SupportedMediaTypes)
            {
                var mediaType2 = formatter2.SupportedMediaTypes.Single(m => m.Equals(mediaType1));
                Assert.NotSame(mediaType1, mediaType2);
            }
        }

        [Fact]
        public void SupportEncodings_ValuesAreNotSharedBetweenInstances()
        {
            var formatter1 = CreateOutputFormatter();
            var formatter2 = CreateOutputFormatter();

            foreach (Encoding mediaType1 in formatter1.SupportedEncodings)
            {
                Encoding mediaType2 = formatter2.SupportedEncodings.Single(m => m.Equals(mediaType1));
                Assert.NotSame(mediaType1, mediaType2);
            }
        }

        [Fact]
        public void SupportMediaTypes_DefaultSupportedMediaTypes()
        {
            var formatter = CreateOutputFormatter();
            Assert.True(ExpectedSupportedMediaTypes.SequenceEqual(formatter.SupportedMediaTypes));
        }

        [Fact]
        public void SupportEncoding_DefaultSupportedEncodings()
        {
            var formatter1 = CreateOutputFormatter();
            Assert.True(ExpectedSupportedEncodings.SequenceEqual(formatter1.SupportedEncodings));

            var formatter2 = CreateInputFormatter();
            Assert.True(ExpectedSupportedEncodings.SequenceEqual(formatter2.SupportedEncodings));
        }

        [Fact]
        public void ReadFromStreamAsync_ThrowsOnNull()
        {
            var formatter = CreateInputFormatter();
#if NETCORE
            ExceptionAssert.ThrowsArgumentNull(() => { formatter.ReadRequestBodyAsync(null, Encoding.UTF8).Wait(); }, "context");
#else
            ExceptionAssert.ThrowsArgumentNull(() => { formatter.ReadFromStreamAsync(null, Stream.Null, null, null).Wait(); }, "type");
            ExceptionAssert.ThrowsArgumentNull(() => { formatter.ReadFromStreamAsync(typeof(object), null, null, null).Wait(); }, "readStream");
#endif
        }

#if !NETCORE // TODO #939: Enable this test on AspNetCore.
        [Fact]
        public Task ReadFromStreamAsync_WhenContentLengthIsZero_DoesNotReadStream()
        {
            // Arrange
            var formatter = CreateFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = 0;

            // Act 
            return formatter.ReadFromStreamAsync(typeof(SampleType), mockStream.Object, content, mockFormatterLogger)
                .ContinueWith(
                    readTask =>
                    {
                        // Assert
                        Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                        mockStream.Verify(s => s.Read(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never());
                        mockStream.Verify(s => s.ReadByte(), Times.Never());
                        mockStream.Verify(s => s.BeginRead(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
                    });
        }

        [Fact]
        public Task ReadFromStreamAsync_WhenContentLengthIsZero_DoesNotCloseStream()
        {
            // Arrange
            var formatter = CreateFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = 0;

            // Act 
            return formatter.ReadFromStreamAsync(typeof(SampleType), mockStream.Object, content, mockFormatterLogger)
                .ContinueWith(
                    readTask =>
                    {
                        // Assert
                        Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                        mockStream.Verify(s => s.Close(), Times.Never());
                    });
        }

        [Theory]
        [InlineData(false)]
        [InlineData(0)]
        [InlineData("")]
        public async Task ReadFromStreamAsync_WhenContentLengthIsZero_ReturnsDefaultTypeValue<T>(T value)
        {
            // Arrange
            var formatter = CreateInputFormatter();
            HttpContent content = new StringContent("");

            // Act
            var result = await formatter.ReadFromStreamAsync(typeof(T), await content.ReadAsStreamAsync(),
                content, null);

            // Assert
            Assert.NotNull(value.GetType());
            Assert.Equal(default(T), (T)result);
        }

        [Fact]
        public Task ReadFromStreamAsync_ReadsDataButDoesNotCloseStream()
        {
            // Arrange
            var formatter = CreateFormatter();
            MemoryStream memStream = new MemoryStream(ExpectedSampleTypeByteRepresentation);
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = memStream.Length;
            contentHeaders.ContentType = CreateSupportedMediaType();

            // Act
            return formatter.ReadFromStreamAsync(typeof(SampleType), memStream, content, null).ContinueWith(
                readTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                    Assert.True(memStream.CanRead);

                    var value = Assert.IsType<SampleType>(readTask.Result);
                    Assert.Equal(42, value.Number);
                });
        }

        [Fact]
        public Task ReadFromStreamAsync_WhenContentLengthIsNull_ReadsDataButDoesNotCloseStream()
        {
            // Arrange
            var formatter = CreateFormatter();
            MemoryStream memStream = new MemoryStream(ExpectedSampleTypeByteRepresentation);
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = null;
            contentHeaders.ContentType = CreateSupportedMediaType();

            // Act
            return formatter.ReadFromStreamAsync(typeof(SampleType), memStream, content, null).ContinueWith(
                readTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                    Assert.True(memStream.CanRead);

                    var value = Assert.IsType<SampleType>(readTask.Result);
                    Assert.Equal(42, value.Number);
                });
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsOnNull()
        {
            var formatter = CreateFormatter();
            ExceptionAssert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(null, new object(), Stream.Null, null, null); }, "type");
            ExceptionAssert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(typeof(object), new object(), null, null, null); }, "writeStream");
        }

        [Fact]
        public Task WriteToStreamAsync_WritesDataButDoesNotCloseStream()
        {
            // Arrange
            var formatter = CreateFormatter();
            SampleType sampleType = new SampleType { Number = 42 };
            MemoryStream memStream = new MemoryStream();
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = CreateSupportedMediaType();

            // Act
            return formatter.WriteToStreamAsync(typeof(SampleType), sampleType, memStream, content, null).ContinueWith(
                writeTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, writeTask.Status);
                    Assert.True(memStream.CanRead);

                    byte[] actualSampleTypeByteRepresentation = memStream.ToArray();
                    Assert.NotEmpty(actualSampleTypeByteRepresentation);
                });
        }

        [Fact]
        public virtual async Task Overridden_WriteToStreamAsyncWithoutCancellationToken_GetsCalled()
        {
            // Arrange
            Stream stream = new MemoryStream();
            Mock<ODataMediaTypeFormatter> formatter = CreateMockFormatter();
            ObjectContent<int> content = new ObjectContent<int>(42, formatter.Object);

            formatter
                .Setup(f => f.WriteToStreamAsync(typeof(int), 42, stream, content, null /* transportContext */))
                .Returns(TaskHelpers.Completed())
                .Verifiable();

            // Act
            await content.CopyToAsync(stream);

            // Assert
            formatter.Verify();
        }

        [Fact]
        public virtual async Task Overridden_WriteToStreamAsyncWithCancellationToken_GetsCalled()
        {
            // Arrange
            Stream stream = new MemoryStream();
            Mock<ODataMediaTypeFormatter> formatter = CreateMockFormatter();
            ObjectContent<int> content = new ObjectContent<int>(42, formatter.Object);

            formatter
                .Setup(f => f.WriteToStreamAsync(typeof(int), 42, stream, content, null /* transportContext */, CancellationToken.None))
                .Returns(TaskHelpers.Completed())
                .Verifiable();

            // Act
            await content.CopyToAsync(stream);

            // Assert
            formatter.Verify();
        }

        [Fact]
        public virtual async Task Overridden_ReadFromStreamAsyncWithoutCancellationToken_GetsCalled()
        {
            // Arrange
            Stream stream = new MemoryStream();
            Mock<ODataMediaTypeFormatter> formatter = CreateMockFormatter();
            formatter.Object.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/test"));
            StringContent content = new StringContent(" ", Encoding.Default, "application/test");
            CancellationTokenSource cts = new CancellationTokenSource();

            formatter
                .Setup(f => f.ReadFromStreamAsync(typeof(string), It.IsAny<Stream>(), content, null /*formatterLogger */))
                .Returns(Task.FromResult<object>(null))
                .Verifiable();

            // Act
            await content.ReadAsAsync<string>(new[] { formatter.Object }, cts.Token);

            // Assert
            formatter.Verify();
        }

        [Fact]
        public virtual async Task Overridden_ReadFromStreamAsyncWithCancellationToken_GetsCalled()
        {
            // Arrange
            Stream stream = new MemoryStream();
            Mock<ODataMediaTypeFormatter> formatter = CreateMockFormatter();
            formatter.Object.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/test"));
            StringContent content = new StringContent(" ", Encoding.Default, "application/test");
            CancellationTokenSource cts = new CancellationTokenSource();

            formatter
                .Setup(f => f.ReadFromStreamAsync(typeof(string), It.IsAny<Stream>(), content, null /*formatterLogger */, cts.Token))
                .Returns(Task.FromResult<object>(null))
                .Verifiable();

            // Act
            await content.ReadAsAsync<string>(new[] { formatter.Object }, cts.Token);

            // Assert
            formatter.Verify();
        }

        [Fact]
        public void Ctor_ThrowsArgumentNull_PayloadKinds()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataMediaTypeFormatter(payloadKinds: null),
                "payloadKinds");
        }

        [Fact]
        public void CopyCtor_ThrowsArgumentNull_Request()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[0]);
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataMediaTypeFormatter(formatter, request: null),
                "request");
        }

        [Fact]
        public void CopyCtor_ThrowsArgumentNull_Formatter()
        {
            ExceptionAssert.ThrowsArgumentNull(
                () => new ODataMediaTypeFormatter(formatter: null, request: new HttpRequestMessage()),
                "formatter");
        }

        [Fact]
        public async Task WriteToStreamAsyncReturnsODataRepresentation()
        {
            // Arrange
            ODataConventionModelBuilder modelBuilder = ODataConventionModelBuilderFactory.Create();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)");
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, null, model);
            request.SetConfiguration(configuration);
            IEdmEntitySet entitySet = model.EntityContainer.EntitySets().Single();
            request.ODataProperties().Path = new ODataPath(new EntitySetSegment(entitySet),
                new KeySegment(new[] { new KeyValuePair<string, object>("ID", 10) }, entitySet.EntityType(), entitySet));
            request.EnableODataDependencyInjectionSupport(routeName);

            ODataMediaTypeFormatter formatter = CreateFormatterWithJson(model, request, ODataPayloadKind.Resource);

            // Act
            ObjectContent<WorkItem> content = new ObjectContent<WorkItem>(
                (WorkItem)TypeInitializer.GetInstance(SupportedTypes.WorkItem), formatter);

            // Assert
            JsonAssert.Equal(Resources.WorkItemEntry, await content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("prefix", "http://localhost/prefix")]
        [InlineData("{a}", "http://localhost/prefix")]
        [InlineData("{a}/{b}", "http://localhost/prefix/prefix2")]
        public async Task WriteToStreamAsync_ReturnsCorrectBaseUri(string routePrefix, string baseUri)
        {
            IEdmModel model = ODataConventionModelBuilderFactory.Create().GetEdmModel();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, baseUri);
            HttpConfiguration configuration = new HttpConfiguration();
            string routeName = "Route";
            configuration.MapODataServiceRoute(routeName, routePrefix, model);
            request.SetConfiguration(configuration);
            request.ODataProperties().Path = new ODataPath();
            request.EnableODataDependencyInjectionSupport(routeName);
            HttpRouteData routeData = new HttpRouteData(new HttpRoute());
            routeData.Values.Add("a", "prefix");
            routeData.Values.Add("b", "prefix2");
            request.SetRouteData(routeData);

            ODataMediaTypeFormatter formatter = CreateFormatterWithJson(model, request, ODataPayloadKind.ServiceDocument);
            var content = new ObjectContent<ODataServiceDocument>(new ODataServiceDocument(), formatter);

            string actualContent = await content.ReadAsStringAsync();

            Assert.Contains("\"@odata.context\":\"" + baseUri + "/$metadata\"", actualContent);
        }

        [Fact]
        public async Task WriteToStreamAsync_Throws_WhenBaseUriCannotBeGenerated()
        {
            IEdmModel model = ODataConventionModelBuilderFactory.Create().GetEdmModel();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.EnableODataDependencyInjectionSupport();
            request.GetConfiguration().Routes.MapHttpRoute(Abstraction.HttpRouteCollectionExtensions.RouteName, "{param}");
            request.ODataProperties().Path = new ODataPath();

            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, ODataPayloadKind.ServiceDocument);
            var content = new ObjectContent<ODataServiceDocument>(new ODataServiceDocument(), formatter);

            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => content.ReadAsStringAsync(),
                "The ODataMediaTypeFormatter was unable to determine the base URI for the request. The request must be processed by an OData route for the OData formatter to serialize the response.");
        }

        /// <summary>
        /// Host name used by tests for verifying GetBaseAddress delegate
        /// </summary>
        private const string CustomHost = "www.microsoft.com";

        /// <summary>
        /// Delegate for GetBaseAddress that converts uris to https
        /// </summary>
        /// <param name="httpRequestMessage">The HttpRequestMessage representing this request.</param>
        /// <returns>A custom uri for the base address.</returns>
        private Uri GetCustomBaseAddress(HttpRequestMessage httpRequestMessage)
        {
            Uri baseAddress = ODataMediaTypeFormatter.GetDefaultBaseAddress(httpRequestMessage);

            UriBuilder uriBuilder = new UriBuilder(baseAddress);
            uriBuilder.Scheme = Uri.UriSchemeHttps;
            uriBuilder.Port = -1;
            uriBuilder.Host = CustomHost;
            baseAddress = uriBuilder.Uri;
            return baseAddress;
        }

        [Fact]
        public async Task GetBaseAddress_AllowsBaseAddressOverride()
        {
            // Arrange
            string routeName = "Route";
            string routePrefix = "prefix";
            string baseUri = "http://localhost/prefix";
            IEdmModel model = new EdmModel();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, baseUri);
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.MapODataServiceRoute(routeName, routePrefix, model);
            request.SetConfiguration(configuration);
            request.ODataProperties().Path = new ODataPath();
            request.EnableODataDependencyInjectionSupport(routeName);
            HttpRouteData routeData = new HttpRouteData(new HttpRoute());
            routeData.Values.Add("a", "prefix");
            request.SetRouteData(routeData);

            // Act
            ODataMediaTypeFormatter formatter = CreateFormatterWithJson(model, request, ODataPayloadKind.ServiceDocument);
            formatter.BaseAddressFactory = GetCustomBaseAddress;
            var content = new ObjectContent<ODataServiceDocument>(new ODataServiceDocument(), formatter);
            string actualContent = await content.ReadAsStringAsync();

            // Assert
            Assert.Contains("\"@odata.context\":\"https://" + CustomHost + "/" + routePrefix + "/", actualContent);
        }

        [Fact]
        public void GetDefaultBaseAddress_ThrowsWhenRequestIsNull()
        {
            ExceptionAssert.ThrowsArgumentNull(() => ODataMediaTypeFormatter.GetDefaultBaseAddress(null), "request");
        }

        [Fact]
        public void GetDefaultBaseAddress_ReturnsCorrectBaseAddress()
        {
            // Arrange
            string baseUriText = "http://discovery.contoso.com/";
            string routePrefix = "api/discovery/v21.0";
            string fullUriText = baseUriText + routePrefix + "/Instances";
            string routeName = "Route";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, fullUriText);
            HttpConfiguration configuration = new HttpConfiguration();
            IEdmModel model = new EdmModel();
            configuration.MapODataServiceRoute(routeName, routePrefix, model);
            request.SetConfiguration(configuration);
            request.EnableODataDependencyInjectionSupport(routeName);

            // Act
            Uri baseUri = ODataMediaTypeFormatter.GetDefaultBaseAddress(request);

            // Assert
            Assert.Equal(baseUriText + routePrefix + "/", baseUri.ToString());
        }

        [Theory]
        [InlineData(null, null, "4.0")]
        [InlineData("1.0", null, "4.0")]
        [InlineData("2.0", null, "4.0")]
        [InlineData("3.0", null, "4.0")]
        [InlineData(null, "1.0", "4.0")]
        [InlineData(null, "2.0", "4.0")]
        [InlineData(null, "3.0", "4.0")]
        [InlineData("1.0", "1.0", "4.0")]
        [InlineData("1.0", "2.0", "4.0")]
        [InlineData("1.0", "3.0", "4.0")]
        public void SetDefaultContentHeaders_SetsRightODataServiceVersion(string requestDataServiceVersion, string requestMaxDataServiceVersion, string expectedDataServiceVersion)
        {
            var request = RequestFactory.Create();
            if (requestDataServiceVersion != null)
            {
                request.Headers.TryAddWithoutValidation("OData-Version", requestDataServiceVersion);
            }
            if (requestMaxDataServiceVersion != null)
            {
                request.Headers.TryAddWithoutValidation("OData-MaxVersion", requestMaxDataServiceVersion);
            }

            HttpContentHeaders contentHeaders = new StringContent("").Headers;

            CreateFormatterWithoutRequest()
            .GetPerRequestFormatterInstance(typeof(int), request, MediaTypeHeaderValue.Parse("application/xml"))
            .SetDefaultContentHeaders(typeof(int), contentHeaders, MediaTypeHeaderValue.Parse("application/xml"));

            IEnumerable<string> headervalues;
            Assert.True(contentHeaders.TryGetValues("OData-Version", out headervalues));
            Assert.Equal(new string[] { expectedDataServiceVersion }, headervalues);
        }

        [Theory]
        [InlineData(null, null, "application/json; odata.metadata=minimal")]
        [InlineData(null, "utf-8", "application/json; odata.metadata=minimal; charset=utf-8")]
        [InlineData(null, "utf-16", "application/json; odata.metadata=minimal; charset=utf-16")]
        [InlineData("application/json", null, "application/json; odata.metadata=minimal")]
        [InlineData("application/json", "utf-8", "application/json; odata.metadata=minimal; charset=utf-8")]
        [InlineData("application/json", "utf-16", "application/json; odata.metadata=minimal; charset=utf-16")]
        [InlineData("application/json;odata.metadata=minimal", null, "application/json; odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=minimal", "utf-8", "application/json; odata.metadata=minimal; charset=utf-8")]
        [InlineData("application/json;odata.metadata=minimal", "utf-16", "application/json; odata.metadata=minimal; charset=utf-16")]
        [InlineData("application/json;odata.metadata=full", null, "application/json; odata.metadata=full")]
        [InlineData("application/json;odata.metadata=full", "utf-8", "application/json; odata.metadata=full; charset=utf-8")]
        [InlineData("application/json;odata.metadata=full", "utf-16", "application/json; odata.metadata=full; charset=utf-16")]
        [InlineData("application/json;odata.metadata=none", null, "application/json; odata.metadata=none")]
        [InlineData("application/json;odata.metadata=none", "utf-8", "application/json; odata.metadata=none; charset=utf-8")]
        [InlineData("application/json;odata.metadata=none", "utf-16", "application/json; odata.metadata=none; charset=utf-16")]
        public void SetDefaultContentHeaders_SetsRightContentType(string acceptHeader, string acceptCharset, string contentType)
        {
            // Arrange
            MediaTypeHeaderValue expectedResult = MediaTypeHeaderValue.Parse(contentType);

            // If no accept header is present the content negotiator will pick application/json; odata.metadata=minimal
            // based on CanWriteType
            MediaTypeHeaderValue mediaType = acceptHeader == null ?
                MediaTypeHeaderValue.Parse("application/json; odata.metadata=minimal") :
                MediaTypeHeaderValue.Parse(acceptHeader);

            var request = RequestFactory.Create();
            if (acceptHeader != null)
            {
                request.Headers.TryAddWithoutValidation("Accept", acceptHeader);
            }
            if (acceptCharset != null)
            {
                request.Headers.TryAddWithoutValidation("Accept-Charset", acceptCharset);
                mediaType.CharSet = acceptCharset;
            }

            HttpContentHeaders contentHeaders = new StringContent(String.Empty).Headers;
            contentHeaders.Clear();

            MediaTypeFormatter formatter = ODataMediaTypeFormatters
                .Create()
                .First(f => f.SupportedMediaTypes.Contains(MediaTypeHeaderValue.Parse("application/json")));
            formatter = formatter.GetPerRequestFormatterInstance(typeof(int), request, mediaType);

            // Act
            formatter.SetDefaultContentHeaders(typeof(int), contentHeaders, mediaType);

            // Assert
            Assert.Equal(expectedResult, contentHeaders.ContentType);
        }

        [Fact]
        public void TryGetInnerTypeForDelta_ChangesRefToGenericParameter_ForDeltas()
        {
            Type type = typeof(Delta<Customer>);

            bool success = EdmLibHelpers.TryGetInnerTypeForDelta(ref type);

            Assert.Same(typeof(Customer), type);
            Assert.True(success);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(List<string>))]
        public void TryGetInnerTypeForDelta_ReturnsFalse_ForNonDeltas(Type originalType)
        {
            Type type = originalType;

            bool success = EdmLibHelpers.TryGetInnerTypeForDelta(ref type);

            Assert.Same(originalType, type);
            Assert.False(success);
        }

        [Fact]
        public Task WriteToStreamAsync_WhenObjectIsNull_WritesDataButDoesNotCloseStream()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatterWithRequest();
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.CanWrite).Returns(true);
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Act
            return formatter.WriteToStreamAsync(typeof(SampleType), null, mockStream.Object, content, null).ContinueWith(
                writeTask =>
                {
                    // Assert (OData formatter doesn't support writing nulls)
                    Assert.Equal(TaskStatus.Faulted, writeTask.Status);
                    ExceptionAssert.Throws<SerializationException>(() => writeTask.GetAwaiter().GetResult(), "Cannot serialize a null 'Resource'.");
                    mockStream.Verify(s => s.Close(), Times.Never());
                    mockStream.Verify(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
                });
        }

        [Theory]
        [InlineData("Test content", "utf-8", true)]
        [InlineData("Test content", "utf-16", true)]
        public Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            MediaTypeFormatter formatter = CreateFormatterWithRequest();
            formatter.SupportedEncodings.Add(CreateEncoding(encoding));
            string formattedContent = CreateFormattedContent(content);
            string mediaType = string.Format("application/json; odata.metadata=minimal; charset={0}", encoding);

            // Act & assert
            return ReadContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Theory]
        [InlineData("Test content", "utf-8", true)]
        [InlineData("Test content", "utf-16", true)]
        public Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            MediaTypeFormatter formatter = CreateFormatterWithRequest();
            formatter.SupportedEncodings.Add(CreateEncoding(encoding));
            string formattedContent = CreateFormattedContent(content);
            string mediaType = string.Format("application/json; odata.metadata=minimal; charset={0}", encoding);

            // Act & assert
            return WriteContentUsingCorrectCharacterEncodingHelper(
                formatter, content, formattedContent, mediaType, encoding, isDefaultEncoding);
        }

        [Fact]
        public void ReadFromStreamAsync_ThrowsInvalidOperation_WithoutRequest()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            var formatter = CreateFormatter(builder.GetEdmModel());

            ExceptionAssert.Throws<InvalidOperationException>(
                () => formatter.ReadFromStreamAsync(typeof(Customer), new MemoryStream(), content: null, formatterLogger: null),
                "The OData formatter requires an attached request in order to deserialize. Controller classes must derive from ODataController or be marked with ODataFormattingAttribute. Custom parameter bindings must call GetPerRequestFormatterInstance on each formatter and use these per-request instances.");
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsInvalidOperation_WithoutRequest()
        {
            var builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<Customer>("Customers");
            var formatter = CreateFormatter(builder.GetEdmModel());

            ExceptionAssert.Throws<InvalidOperationException>(
                () => formatter.WriteToStreamAsync(typeof(Customer), new Customer(), new MemoryStream(), content: null, transportContext: null),
                "The OData formatter does not support writing client requests. This formatter instance must have an associated request.");
        }

        [Fact]
        public void WriteToStreamAsync_Passes_MetadataLevelToSerializerContext()
        {
            // Arrange
            var model = CreateModel();
            Mock<ODataSerializer> serializer = new Mock<ODataSerializer>(ODataPayloadKind.Property);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            var request = CreateFakeODataRequest(model, null, serializerProvider.Object);

            serializerProvider.Setup(p => p.GetODataPayloadSerializer(typeof(int), request)).Returns(serializer.Object);
            serializer
                .Setup(s => s.WriteObject(42, typeof(int), It.IsAny<ODataMessageWriter>(),
                    It.Is<ODataSerializerContext>(c => c.MetadataLevel == ODataMetadataLevel.FullMetadata)))
                .Verifiable();

            var formatter = new ODataMediaTypeFormatter(Enumerable.Empty<ODataPayloadKind>());
            formatter.Request = request;
            HttpContent content = new StringContent("42");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata.metadata=full");

            // Act
            formatter.WriteToStreamAsync(typeof(int), 42, new MemoryStream(), content, transportContext: null);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void WriteToStreamAsync_PassesSelectExpandClause_ThroughSerializerContext()
        {
            // Arrange
            var model = CreateModel();
            SelectExpandClause selectExpandClause =
                new SelectExpandClause(new SelectItem[0], allSelected: true);

            Mock<ODataSerializer> serializer = new Mock<ODataSerializer>(ODataPayloadKind.Property);
            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();

            var request = CreateFakeODataRequest(model, null, serializerProvider.Object);
            request.ODataProperties().SelectExpandClause = selectExpandClause;

            serializerProvider.Setup(p => p.GetODataPayloadSerializer(typeof(int), request)).Returns(serializer.Object);
            serializer
                .Setup(s => s.WriteObject(42, typeof(int), It.IsAny<ODataMessageWriter>(),
                    It.Is<ODataSerializerContext>(c => c.SelectExpandClause == selectExpandClause)))
                .Verifiable();


            var formatter = new ODataMediaTypeFormatter(Enumerable.Empty<ODataPayloadKind>());
            formatter.Request = request;
            HttpContent content = new StringContent("42");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata.metadata=full");

            // Act
            formatter.WriteToStreamAsync(typeof(int), 42, new MemoryStream(), content, transportContext: null);

            // Assert
            serializer.Verify();
        }

        [Fact]
        public void MessageReaderSettings_Property()
        {
            var formatter = CreateFormatter();
            var messageReaderSettings = formatter.Request.GetRequestContainer()
                .GetRequiredService<ODataMessageReaderSettings>();

            Assert.NotNull(messageReaderSettings);
            Assert.False(messageReaderSettings.EnableMessageStreamDisposal);
        }

        [Fact]
        public void MessageWriterSettings_Property()
        {
            var formatter = CreateFormatter();
            var messageWriterSettings = formatter.Request.GetRequestContainer()
                .GetRequiredService<ODataMessageWriterSettings>();

            Assert.NotNull(messageWriterSettings);
            Assert.False(messageWriterSettings.EnableMessageStreamDisposal);
        }

        [Fact]
        public void MessageReaderQuotas_Property_RoundTrip()
        {
            var formatter = CreateFormatter();
            var messageReaderQuotas = formatter.Request.GetRequestContainer()
                .GetRequiredService<ODataMessageReaderSettings>().MessageQuotas;
            messageReaderQuotas.MaxNestingDepth = 42;

            Assert.Equal(42, messageReaderQuotas.MaxNestingDepth);
        }

        [Fact]
        public void MessageWriterQuotas_Property_RoundTrip()
        {
            var formatter = CreateFormatter();
            var messageWriterSettings = formatter.Request.GetRequestContainer()
                .GetRequiredService<ODataMessageWriterSettings>();
            messageWriterSettings.MessageQuotas.MaxNestingDepth = 42;

            Assert.Equal(42, messageWriterSettings.MessageQuotas.MaxNestingDepth);
        }

        [Fact]
        public void Default_ReceiveMessageSize_Is_MaxedOut()
        {
            var formatter = CreateFormatter();
            var messageReaderQuotas = formatter.Request.GetRequestContainer()
                .GetRequiredService<ODataMessageReaderSettings>().MessageQuotas;
            Assert.Equal(Int64.MaxValue, messageReaderQuotas.MaxReceivedMessageSize);
        }

        [Fact]
        public async Task MessageReaderQuotas_Is_Passed_To_ODataLib()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            var messageReaderQuotas = formatter.Request.GetRequestContainer()
                .GetRequiredService<ODataMessageReaderSettings>().MessageQuotas;
            messageReaderQuotas.MaxReceivedMessageSize = 1;

            HttpContent content = new StringContent("{ 'Number' : '42' }");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            await ExceptionAssert.ThrowsAsync<ODataException>(
                async () => await formatter.ReadFromStreamAsync(typeof(int), await content.ReadAsStreamAsync(), content, formatterLogger: null),
                "The maximum number of bytes allowed to be read from the stream has been exceeded. After the last read operation, a total of 19 bytes has been read from the stream; however a maximum of 1 bytes is allowed.");
        }

        [Fact]
        public async void Request_IsPassedThroughDeserializerContext()
        {
            // Arrange
            var model = CreateModel();
            Mock<ODataEdmTypeDeserializer> deserializer = new Mock<ODataEdmTypeDeserializer>(ODataPayloadKind.Property);
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>();
            var request = CreateFakeODataRequest(model, deserializerProvider.Object, null);

            deserializerProvider.Setup(p => p.GetEdmTypeDeserializer(It.IsAny<IEdmTypeReference>())).Returns(deserializer.Object);
            deserializer
                .Setup(d => d.Read(It.IsAny<ODataMessageReader>(), typeof(int), It.Is<ODataDeserializerContext>(c => c.Request == request)))
                .Verifiable();

            var formatter = new ODataMediaTypeFormatter(Enumerable.Empty<ODataPayloadKind>());
            formatter.Request = request;
            HttpContent content = new StringContent("42");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata.metadata=full");

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act
            await formatter.ReadFromStreamAsync(typeof(int), new MemoryStream(), content, mockFormatterLogger);

            // Assert
            deserializer.Verify();
        }

        public static TheoryDataSet<ODataPath, ODataPayloadKind> CanReadTypeTypesTestData
        {
            get
            {
                CustomersModelWithInheritance model = new CustomersModelWithInheritance();
                EntitySetSegment entitySetSegment = new EntitySetSegment(model.Customers);

                var keys = new[] { new KeyValuePair<string, object>("ID", 42) };
                KeySegment keyValueSegment = new KeySegment(keys, model.Customer, model.Customers);

                NavigationPropertySegment navSegment =
                    new NavigationPropertySegment(model.Customer.FindProperty("Orders") as IEdmNavigationProperty,
                        model.Orders);
                PropertySegment propertySegment = new PropertySegment(model.Customer.FindProperty("Address") as IEdmStructuralProperty);

                return new TheoryDataSet<ODataPath, ODataPayloadKind>
                {
                    { new ODataPath(entitySetSegment), ODataPayloadKind.Resource }, // POST ~/entityset
                    { new ODataPath(entitySetSegment, keyValueSegment), ODataPayloadKind.Resource }, // PUT ~/entityset(key)
                    { new ODataPath(entitySetSegment, keyValueSegment, navSegment), ODataPayloadKind.Resource }, // PUT ~/entityset(key)/nav
                    { new ODataPath(entitySetSegment, keyValueSegment, propertySegment), ODataPayloadKind.Resource }
                };
            }
        }

        [Theory]
        [MemberData(nameof(CanReadTypeTypesTestData))]
        public void CanReadType_ForTypeless_ReturnsExpectedResult_DependingOnODataPathAndPayloadKind(ODataPath path, ODataPayloadKind payloadKind)
        {
            // Arrange
            IEnumerable<ODataPayloadKind> allPayloadKinds = Enum.GetValues(typeof(ODataPayloadKind)).Cast<ODataPayloadKind>();
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            request.ODataProperties().Path = path;

            var formatterWithGivenPayload = new ODataMediaTypeFormatter(new[] { payloadKind }) { Request = request };
            var formatterWithoutGivenPayload = new ODataMediaTypeFormatter(allPayloadKinds.Except(new[] { payloadKind })) { Request = request };

            // Act & Assert
            Assert.True(formatterWithGivenPayload.CanReadType(typeof(IEdmObject)));
            Assert.False(formatterWithoutGivenPayload.CanReadType(typeof(IEdmObject)));
        }

        public static TheoryDataSet<ODataPayloadKind, Type> CanWriteType_ReturnsExpectedResult_ForEdmObjects_TestData
        {
            get
            {
                Type entityCollectionEdmObjectType = new Mock<IEdmObject>().As<IEnumerable<IEdmEntityObject>>().Object.GetType();
                Type complexCollectionEdmObjectType = new Mock<IEdmObject>().As<IEnumerable<IEdmComplexObject>>().Object.GetType();

                return new TheoryDataSet<ODataPayloadKind, Type>
                {
                    { ODataPayloadKind.Resource , typeof(IEdmEntityObject) },
                    { ODataPayloadKind.Resource , typeof(TypedEdmEntityObject) },
                    { ODataPayloadKind.ResourceSet , entityCollectionEdmObjectType },
                    { ODataPayloadKind.ResourceSet , typeof(IEnumerable<IEdmEntityObject>) },
                    { ODataPayloadKind.Property , typeof(IEdmComplexObject) },
                    { ODataPayloadKind.Property , typeof(TypedEdmComplexObject) },
                    { ODataPayloadKind.Collection , complexCollectionEdmObjectType },
                    { ODataPayloadKind.Collection , typeof(IEnumerable<IEdmComplexObject>) },
                    { ODataPayloadKind.Property, typeof(NullEdmComplexObject) }
                };
            }
        }

        [Theory]
        [MemberData(nameof(CanWriteType_ReturnsExpectedResult_ForEdmObjects_TestData))]
        public void CanWriteType_ReturnsTrueForEdmObjects_WithRightPayload(ODataPayloadKind payloadKind, Type type)
        {
            // Arrange
            IEnumerable<ODataPayloadKind> allPayloadKinds = Enum.GetValues(typeof(ODataPayloadKind)).Cast<ODataPayloadKind>();
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);

            var formatterWithGivenPayload = new ODataMediaTypeFormatter(new[] { payloadKind }) { Request = request };
            var formatterWithoutGivenPayload = new ODataMediaTypeFormatter(allPayloadKinds.Except(new[] { payloadKind })) { Request = request };

            // Act & Assert
            Assert.True(formatterWithGivenPayload.CanWriteType(type));
            Assert.False(formatterWithoutGivenPayload.CanWriteType(type));
        }

        public static TheoryDataSet<Type> InvalidIEdmObjectImplementationTypes
        {
            get
            {
                return new TheoryDataSet<Type>
                {
                    typeof(IEdmObject),
                    typeof(TypedEdmStructuredObject),
                    new Mock<IEdmObject>().Object.GetType(),
                    new Mock<IEdmObject>().As<IEnumerable<IEdmObject>>().Object.GetType()
                };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidIEdmObjectImplementationTypes))]
        public void CanWriteType_ReturnsFalse_ForInvalidIEdmObjectImplementations_NoMatterThePayload(Type type)
        {
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            IEnumerable<ODataPayloadKind> allPayloadKinds = Enum.GetValues(typeof(ODataPayloadKind)).Cast<ODataPayloadKind>();
            var formatter = new ODataMediaTypeFormatter(allPayloadKinds);
            formatter.Request = request;

            var result = formatter.CanWriteType(type);

            Assert.False(result);
        }

        [Fact]
        public async Task WriteToStreamAsync_ThrowsSerializationException_IfEdmTypeIsNull()
        {
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            var formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[0]);
            formatter.Request = request;

            NullEdmType edmObject = new NullEdmType();

            await ExceptionAssert.ThrowsAsync<SerializationException>(
                () => formatter
                    .WriteToStreamAsync(typeof(int), edmObject, new MemoryStream(), new Mock<HttpContent>().Object, transportContext: null),
                "The EDM type of an IEdmObject cannot be null.", partialMatch: true);
        }

        [Fact]
        public async Task WriteToStreamAsync_UsesTheRightEdmSerializer_ForEdmObjects()
        {
            // Arrange
            IEdmEntityTypeReference edmType = new EdmEntityTypeReference(new EdmEntityType("NS", "Name"), isNullable: false);
            var model = CreateModel();

            Mock<IEdmObject> instance = new Mock<IEdmObject>();
            instance.Setup(e => e.GetEdmType()).Returns(edmType);

            Mock<ODataEdmTypeSerializer> serializer = new Mock<ODataEdmTypeSerializer>(ODataPayloadKind.Resource);
            serializer
                .Setup(s => s.WriteObject(instance.Object, instance.GetType(), It.IsAny<ODataMessageWriter>(), It.IsAny<ODataSerializerContext>()))
                .Verifiable();

            Mock<ODataSerializerProvider> serializerProvider = new Mock<ODataSerializerProvider>();
            serializerProvider.Setup(s => s.GetEdmTypeSerializer(edmType)).Returns(serializer.Object);

            var request = CreateFakeODataRequest(model, null, serializerProvider.Object);
            var formatter = new ODataMediaTypeFormatter(new ODataPayloadKind[0]);
            formatter.Request = request;

            // Act
            await formatter
                .WriteToStreamAsync(instance.GetType(), instance.Object, new MemoryStream(), new StreamContent(new MemoryStream()), transportContext: null);

            // Assert
            serializer.Verify();
        }

        [Theory]
        [InlineData(typeof(SingleResult), false)]
        [InlineData(typeof(SingleResult<SampleType>), true)]
        [InlineData(typeof(SingleResult<TypeNotInModel>), false)]
        public void CanWriteType_ReturnsExpectedResult_ForSingleResult(Type type, bool expectedCanWriteTypeResult)
        {
            IEdmModel model = CreateModel();
            HttpRequestMessage request = CreateFakeODataRequest(model);
            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, ODataPayloadKind.Resource);

            Assert.Equal(expectedCanWriteTypeResult, formatter.CanWriteType(type));
        }

        [Fact]
        public async Task WriteToStreamAsync_SetsMetadataUriWithSelectClause_OnODataWriterSettings()
        {
            // Arrange
            MemoryStream stream = new MemoryStream();
            StreamContent content = new StreamContent(stream);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            IEdmModel model = CreateModel();
            IEdmSchemaType entityType = model.FindDeclaredType("Microsoft.AspNet.OData.Test.Formatter.SampleType");
            IEdmStructuralProperty property =
                ((IEdmStructuredType)entityType).FindProperty("Number") as IEdmStructuralProperty;
            HttpRequestMessage request = CreateFakeODataRequest(model);
            request.RequestUri = new Uri("http://localhost/sampleTypes?$select=Number");
            request.ODataProperties().SelectExpandClause =
                new SelectExpandClause(
                    new Collection<SelectItem>
                        {
                            new PathSelectItem(new ODataSelectPath(new PropertySegment(property))),
                        },
                    allSelected: false);

            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, ODataPayloadKind.Resource);

            // Act
            await formatter.WriteToStreamAsync(typeof(SampleType[]), new SampleType[0], stream, content, transportContext: null);

            // Assert
            stream.Seek(0, SeekOrigin.Begin);
            string result = await content.ReadAsStringAsync();
            JObject obj = JObject.Parse(result);
            Assert.Equal("http://localhost/$metadata#sampleTypes(Number)", obj["@odata.context"]);
        }

        [Fact]
        public async void ReadFromStreamAsync_UsesRightDeserializerFrom_ODataDeserializerProvider()
        {
            // Arrange
            MemoryStream stream = new MemoryStream();
            StringContent content = new StringContent("42");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            IEdmModel model = CreateModel();
            Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Property);
            deserializer.Setup(d => d.Read(It.IsAny<ODataMessageReader>(), typeof(int), It.IsAny<ODataDeserializerContext>()))
                .Verifiable();

            Mock<ODataDeserializerProvider> provider = new Mock<ODataDeserializerProvider>();
            HttpRequestMessage request = CreateFakeODataRequest(model, provider.Object, null);
            provider.Setup(p => p.GetODataDeserializer(typeof(int), request)).Returns(deserializer.Object);

            // Act
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(Enumerable.Empty<ODataPayloadKind>());
            formatter.Request = request;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            await formatter.ReadFromStreamAsync(typeof(int), stream, content, mockFormatterLogger);

            // Assert
            deserializer.Verify();
        }
#endif

#if NETCORE
        private static ODataOutputFormatter CreateOutputFormatter(IEdmModel model = null)
        {
            // Model is not used in AspNetCore.
            return new ODataOutputFormatter(new ODataPayloadKind[0]);
        }

        private static ODataInputFormatter CreateInputFormatter(IEdmModel model = null)
        {
            // Model is not used in AspNetCore.
            return new ODataInputFormatter(new ODataPayloadKind[0]);
        }

#else
        private static Encoding CreateEncoding(string name)
        {
            if (name == "utf-8")
            {
                return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            }
            else if (name == "utf-16")
            {
                return new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);
            }
            else
            {
                throw new ArgumentException("name");
            }
        }

        private static string CreateFormattedContent(string value)
        {
            return string.Format(CultureInfo.InvariantCulture,
                "{{\"@odata.context\":\"http://dummy/$metadata#Edm.String\",\"value\":\"{0}\"}}", value);
        }

        protected ODataMediaTypeFormatter CreateFormatter()
        {
            return CreateFormatterWithRequest();
        }

        protected ODataMediaTypeFormatter CreateInputFormatter()
        {
            return CreateFormatterWithRequest();
        }

        protected ODataMediaTypeFormatter CreateOutputFormatter()
        {
            return CreateFormatterWithRequest();
        }

        protected Mock<ODataMediaTypeFormatter> CreateMockFormatter()
        {
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            ODataPayloadKind[] payloadKinds = new ODataPayloadKind[] { ODataPayloadKind.Property };
            var formatter = new Mock<ODataMediaTypeFormatter>(payloadKinds) { CallBase = true };
            formatter.Object.Request = request;

            return formatter;
        }

        protected MediaTypeHeaderValue CreateSupportedMediaType()
        {
            return MediaTypeHeaderValue.Parse("application/json;odata.metadata=full");
        }

        private static ODataMediaTypeFormatter CreateFormatter(IEdmModel model)
        {
            return new ODataMediaTypeFormatter(new ODataPayloadKind[0]);
        }

        private static ODataMediaTypeFormatter CreateFormatter(IEdmModel model, HttpRequestMessage request,
            params ODataPayloadKind[] payloadKinds)
        {
            return new ODataMediaTypeFormatter(payloadKinds) { Request = request };
        }

        private static ODataMediaTypeFormatter CreateFormatterWithoutRequest()
        {
            return CreateFormatter(CreateModel());
        }

        private static ODataMediaTypeFormatter CreateFormatterWithJson(IEdmModel model, HttpRequestMessage request,
            params ODataPayloadKind[] payloadKinds)
        {
            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, payloadKinds);
            formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse(ODataMediaTypes.ApplicationJsonODataMinimalMetadata));
            return formatter;
        }

        private static ODataMediaTypeFormatter CreateFormatterWithRequest()
        {
            var model = CreateModel();
            var request = CreateFakeODataRequest(model);
            return CreateFormatter(model, request);
        }

        private static HttpRequestMessage CreateFakeODataRequest(IEdmModel model,
            ODataDeserializerProvider deserializerProvider = null,
            ODataSerializerProvider serializerProvider = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://dummy/");
            request.EnableODataDependencyInjectionSupport(Abstraction.HttpRouteCollectionExtensions.RouteName, builder =>
            {
                builder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model);

                if (deserializerProvider != null)
                {
                    builder.AddService(ServiceLifetime.Singleton, sp => deserializerProvider);
                }

                if (serializerProvider != null)
                {
                    builder.AddService(ServiceLifetime.Singleton, sp => serializerProvider);
                }
            });

        request.GetConfiguration().Routes.MapFakeODataRoute();
            request.ODataProperties().Path =
                new ODataPath(new EntitySetSegment(model.EntityContainer.EntitySets().Single()));
            return request;
        }

        private static IEdmModel CreateModel()
        {
            ODataConventionModelBuilder model = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>();
            model.ModelAliasingEnabled = false;
            model.EntityType<SampleType>();
            model.EntitySet<SampleType>("sampleTypes");
            return model.GetEdmModel();
        }

        private static Encoding CreateOrGetSupportedEncoding(MediaTypeFormatter formatter, string encoding, bool isDefaultEncoding)
        {
            Encoding enc = null;
            if (isDefaultEncoding)
            {
                enc = formatter.SupportedEncodings.First((e) => e.WebName.Equals(encoding, StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
                formatter.SupportedEncodings.Add(enc);
            }

            return enc;
        }

        private static Task ReadContentUsingCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = CreateOrGetSupportedEncoding(formatter, encoding, isDefaultEncoding);
            byte[] sourceData = enc.GetBytes(formattedContent);

            // Further Arrange, Act & Assert
            return ReadContentUsingCorrectCharacterEncodingHelper(formatter, content, sourceData, mediaType);
        }

        private static Task ReadContentUsingCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, byte[] sourceData, string mediaType)
        {
            // Arrange
            MemoryStream memStream = new MemoryStream(sourceData);

            StringContent dummyContent = new StringContent(string.Empty);
            HttpContentHeaders headers = dummyContent.Headers;
            headers.Clear();
            headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            headers.ContentLength = sourceData.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act & Assert
            return formatter.ReadFromStreamAsync(typeof(string), memStream, dummyContent, mockFormatterLogger).ContinueWith(
                (readTask) =>
                {
                    string result = readTask.Result as string;

                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, readTask.Status);
                    Assert.Equal(content, result);
                });
        }

        protected static Task WriteContentUsingCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, string formattedContent, string mediaType, string encoding, bool isDefaultEncoding)
        {
            // Arrange
            Encoding enc = CreateOrGetSupportedEncoding(formatter, encoding, isDefaultEncoding);

            byte[] preamble = enc.GetPreamble();
            byte[] data = enc.GetBytes(formattedContent);
            byte[] expectedData = new byte[preamble.Length + data.Length];
            Buffer.BlockCopy(preamble, 0, expectedData, 0, preamble.Length);
            Buffer.BlockCopy(data, 0, expectedData, preamble.Length, data.Length);

            // Further Arrange, Act & Assert
            return WriteContentUsingCorrectCharacterEncodingHelper(formatter, content, expectedData, mediaType);
        }

        private static Task WriteContentUsingCorrectCharacterEncodingHelper(MediaTypeFormatter formatter, string content, byte[] expectedData, string mediaType)
        {
            // Arrange
            MemoryStream memStream = new MemoryStream();

            StringContent dummyContent = new StringContent(string.Empty);
            HttpContentHeaders headers = dummyContent.Headers;
            headers.Clear();
            headers.ContentType = MediaTypeHeaderValue.Parse(mediaType);
            headers.ContentLength = expectedData.Length;

            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;

            // Act & Assert
            return formatter.WriteToStreamAsync(typeof(string), content, memStream, dummyContent, null).ContinueWith(
                (writeTask) =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, writeTask.Status);
                    byte[] actualData = memStream.ToArray();

                    Assert.Equal(expectedData, actualData);
                });
        }

        /// <summary>
        /// A class that is not part of the model.
        /// </summary>
        private class TypeNotInModel
        {
        }

        /// <summary>
        /// An instance of IEdmObject with no EdmType.
        /// </summary>
        private class NullEdmType : IEdmObject
        {
            public IEdmTypeReference GetEdmType()
            {
                return null;
            }
        }
#endif
    }

    /// <summary>
    /// A class with a number.
    /// </summary>
    [DataContract(Name = "DataContractSampleType")]
    public class SampleType
    {
        [DataMember]
        public int Number { get; set; }
    }
}
