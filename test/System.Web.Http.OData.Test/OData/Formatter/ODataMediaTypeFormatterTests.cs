// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypeFormatterTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties<ODataMediaTypeFormatter, MediaTypeFormatter>(TypeAssert.TypeProperties.IsPublicVisibleClass);
        }

        [Theory]
        [InlineData("application/atom+xml")]
        [InlineData("application/json;odata=verbose")]
        public void Constructor(string mediaType)
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();
            Assert.True(formatter.SupportedMediaTypes.Contains(MediaTypeHeaderValue.Parse(mediaType)), string.Format("SupportedMediaTypes should have included {0}.", mediaType.ToString()));
        }

        [Fact]
        public void DefaultMediaTypeReturnsApplicationAtomXml()
        {
            MediaTypeHeaderValue mediaType = ODataMediaTypeFormatter.DefaultMediaType;
            Assert.NotNull(mediaType);
            Assert.Equal("application/atom+xml", mediaType.MediaType);
        }

        [Fact]
        public void WriteToStreamAsyncReturnsODataRepresentation()
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.EntitySet<WorkItem>("WorkItems");

            HttpConfiguration configuration = new HttpConfiguration();
            var route = configuration.Routes.MapHttpRoute(ODataRouteNames.GetById, "{controller}({id})");
            configuration.Routes.MapHttpRoute(ODataRouteNames.PropertyNavigation, "{controller}({id})/{property}");

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(route);

            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(model.GetEdmModel()) { Request = request };

            ObjectContent<WorkItem> content = new ObjectContent<WorkItem>((WorkItem)TypeInitializer.GetInstance(SupportedTypes.WorkItem), formatter);

            RegexReplacement replaceUpdateTime = new RegexReplacement("<updated>*.*</updated>", "<updated>UpdatedTime</updated>");
            Assert.Xml.Equal(BaselineResource.TestEntityWorkItem, content.ReadAsStringAsync().Result, regexReplacements: replaceUpdateTime);
        }

        [Theory]
        [InlineData(null, null, "3.0")]
        [InlineData("1.0", null, "1.0")]
        [InlineData("2.0", null, "2.0")]
        [InlineData("3.0", null, "3.0")]
        [InlineData(null, "1.0", "1.0")]
        [InlineData(null, "2.0", "2.0")]
        [InlineData(null, "3.0", "3.0")]
        [InlineData("1.0", "1.0", "1.0")]
        [InlineData("1.0", "2.0", "2.0")]
        [InlineData("1.0", "3.0", "3.0")]
        public void SetDefaultContentHeaders_SetsRightODataServiceVersion(string requestDataServiceVersion, string requestMaxDataServiceVersion, string expectedDataServiceVersion)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            if (requestDataServiceVersion != null)
            {
                request.Headers.TryAddWithoutValidation("DataServiceVersion", requestDataServiceVersion);
            }
            if (requestMaxDataServiceVersion != null)
            {
                request.Headers.TryAddWithoutValidation("MaxDataServiceVersion", requestMaxDataServiceVersion);
            }

            HttpContentHeaders contentHeaders = new StringContent("").Headers;

            new ODataMediaTypeFormatter()
            .GetPerRequestFormatterInstance(typeof(int), request, MediaTypeHeaderValue.Parse("application/xml"))
            .SetDefaultContentHeaders(typeof(int), contentHeaders, MediaTypeHeaderValue.Parse("application/xml"));

            IEnumerable<string> headervalues;
            Assert.True(contentHeaders.TryGetValues("DataServiceVersion", out headervalues));
            Assert.Equal(headervalues, new string[] { expectedDataServiceVersion + ";" });
        }

        [Fact]
        public void SupportedMediaTypes_HeaderValuesAreNotSharedBetweenInstances()
        {
            var formatter1 = CreateFormatter();
            var formatter2 = CreateFormatter();

            foreach (MediaTypeHeaderValue mediaType1 in formatter1.SupportedMediaTypes)
            {
                MediaTypeHeaderValue mediaType2 = formatter2.SupportedMediaTypes.Single(m => m.Equals(mediaType1));
                Assert.NotSame(mediaType1, mediaType2);
            }
        }

        [Fact]
        public void SupportEncodings_ValuesAreNotSharedBetweenInstances()
        {
            var formatter1 = CreateFormatter();
            var formatter2 = CreateFormatter();

            foreach (Encoding mediaType1 in formatter1.SupportedEncodings)
            {
                Encoding mediaType2 = formatter2.SupportedEncodings.Single(m => m.Equals(mediaType1));
                Assert.NotSame(mediaType1, mediaType2);
            }
        }

        [Fact]
        public void SupportEncoding_DefaultSupportedMediaTypes()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Assert.True(ExpectedSupportedMediaTypes.SequenceEqual(formatter.SupportedMediaTypes));
        }

        [Fact]
        public void SupportEncoding_DefaultSupportedEncodings()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Assert.True(ExpectedSupportedEncodings.SequenceEqual(formatter.SupportedEncodings));
        }

        [Fact]
        public void ReadFromStreamAsync_ThrowsOnNull()
        {
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.ReadFromStreamAsync(null, Stream.Null, null, null); }, "type");
            Assert.ThrowsArgumentNull(() => { formatter.ReadFromStreamAsync(typeof(object), null, null, null); }, "readStream");
        }

        [Fact]
        public Task ReadFromStreamAsync_WhenContentLengthIsZero_DoesNotReadStream()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = 0;
            contentHeaders.ContentType = ExpectedSupportedMediaTypes.First();

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
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Mock<Stream> mockStream = new Mock<Stream>();
            IFormatterLogger mockFormatterLogger = new Mock<IFormatterLogger>().Object;
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = 0;
            contentHeaders.ContentType = ExpectedSupportedMediaTypes.First();

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
        public void ReadFromStreamAsync_WhenContentLengthIsZero_ReturnsDefaultTypeValue<T>(T value)
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatter();
            HttpContent content = new StringContent("");

            // Act
            var result = formatter.ReadFromStreamAsync(typeof(T), content.ReadAsStreamAsync().Result,
                content, null);
            result.WaitUntilCompleted();

            // Assert
            Assert.Equal(default(T), (T)result.Result);
        }

        [Fact]
        public Task ReadFromStreamAsync_ReadsDataButDoesNotCloseStream()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatter();
            MemoryStream memStream = new MemoryStream(ExpectedSampleTypeByteRepresentation);
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = memStream.Length;
            contentHeaders.ContentType = ExpectedSupportedMediaTypes.First();

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
            ODataMediaTypeFormatter formatter = CreateFormatter();
            MemoryStream memStream = new MemoryStream(ExpectedSampleTypeByteRepresentation);
            HttpContent content = new StringContent(String.Empty);
            HttpContentHeaders contentHeaders = content.Headers;
            contentHeaders.ContentLength = null;
            contentHeaders.ContentType = ExpectedSupportedMediaTypes.First();

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
            ODataMediaTypeFormatter formatter = CreateFormatter();
            Assert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(null, new object(), Stream.Null, null, null); }, "type");
            Assert.ThrowsArgumentNull(() => { formatter.WriteToStreamAsync(typeof(object), new object(), null, null, null); }, "writeStream");
        }

        [Fact]
        public Task WriteToStreamAsync_DoesNotCloseStream()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatter();
            SampleType sampleType = new SampleType { Number = 42 };
            Mock<Stream> mockStream = new Mock<Stream>();
            HttpContent content = new StringContent(String.Empty);

            // Act
            return formatter.WriteToStreamAsync(typeof(SampleType), sampleType, mockStream.Object, content, null).ContinueWith(
                writeTask =>
                {
                    // Assert
                    Assert.Equal(TaskStatus.RanToCompletion, writeTask.Status);
                    mockStream.Verify(s => s.Close(), Times.Never());
                    mockStream.Verify(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
                });
        }

        private ODataMediaTypeFormatter CreateFormatter()
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.Entity<SampleType>();
            return new ODataMediaTypeFormatter(model.GetEdmModel()) { IsClient = true };
        }

        private IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get
            {
                yield return MediaTypeHeaderValue.Parse("application/atom+xml");
                yield return MediaTypeHeaderValue.Parse("application/json;odata=verbose");
                yield return MediaTypeHeaderValue.Parse("application/xml");
            }
        }

        private IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get
            {
                yield return new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);
                yield return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            }
        }

        private byte[] ExpectedSampleTypeByteRepresentation
        {
            get
            {
                return ExpectedSupportedEncodings.ElementAt(0).GetBytes(
                  @"<entry xml:base=""http://localhost/"" xmlns=""http://www.w3.org/2005/Atom"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:georss=""http://www.georss.org/georss"" xmlns:gml=""http://www.opengis.net/gml"">
                      <category term=""System.Web.Http.OData.Formatter.SampleType"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"" />
                      <id />
                      <title />
                      <updated>2012-08-17T00:16:14Z</updated>
                      <author>
                        <name />
                      </author>
                      <content type=""application/xml"">
                        <m:properties>
                          <d:Number m:type=""Edm.Int32"">42</d:Number>
                        </m:properties>
                      </content>
                    </entry>"
                );
            }
        }
    }

    [DataContract(Name = "DataContractSampleType")]
    public class SampleType
    {
        [DataMember]
        public int Number { get; set; }
    }
}
