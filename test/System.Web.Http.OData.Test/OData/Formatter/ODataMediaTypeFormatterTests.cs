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
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypeFormatterTests : MediaTypeFormatterTestBase<ODataMediaTypeFormatter>
    {
        [Fact]
        public void WriteToStreamAsyncReturnsODataRepresentation()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<WorkItem>("WorkItems");
            IEdmModel model = modelBuilder.GetEdmModel();

            HttpConfiguration configuration = new HttpConfiguration();
            configuration.EnableOData(model);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/WorkItems(10)");
            request.Properties[HttpPropertyKeys.HttpConfigurationKey] = configuration;
            request.Properties[HttpPropertyKeys.HttpRouteDataKey] = new HttpRouteData(configuration.Routes.First());
            request.Properties["MS_ODataPath"] = new DefaultODataPathHandler(model).Parse("WorkItems(10)");

            ODataMediaTypeFormatter formatter = CreateFormatter(model, request, ODataPayloadKind.Entry);

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

            CreateFormatterWithoutRequest()
            .GetPerRequestFormatterInstance(typeof(int), request, MediaTypeHeaderValue.Parse("application/xml"))
            .SetDefaultContentHeaders(typeof(int), contentHeaders, MediaTypeHeaderValue.Parse("application/xml"));

            IEnumerable<string> headervalues;
            Assert.True(contentHeaders.TryGetValues("DataServiceVersion", out headervalues));
            Assert.Equal(headervalues, new string[] { expectedDataServiceVersion + ";" });
        }

        [Fact]
        public void TryGetInnerTypeForDelta_ChangesRefToGenericParameter_ForDeltas()
        {
            Type type = typeof(Delta<Customer>);

            bool success = ODataMediaTypeFormatter.TryGetInnerTypeForDelta(ref type);

            Assert.Same(typeof(Customer), type);
            Assert.True(success);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(List<string>))]
        public void TryGetInnerTypeForDelta_ReturnsFalse_ForNonDeltas(Type originalType)
        {
            Type type = originalType;

            bool success = ODataMediaTypeFormatter.TryGetInnerTypeForDelta(ref type);

            Assert.Same(originalType, type);
            Assert.False(success);
        }

        [Fact]
        public override Task WriteToStreamAsync_WhenObjectIsNull_WritesDataButDoesNotCloseStream()
        {
            // Arrange
            ODataMediaTypeFormatter formatter = CreateFormatterWithRequest();
            Mock<Stream> mockStream = new Mock<Stream>();
            mockStream.Setup(s => s.CanWrite).Returns(true);
            HttpContent content = new StringContent(String.Empty);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/atom+xml");

            // Act 
            return formatter.WriteToStreamAsync(typeof(SampleType), null, mockStream.Object, content, null).ContinueWith(
                writeTask =>
                {
                    // Assert (OData formatter doesn't support writing nulls)
                    Assert.Equal(TaskStatus.Faulted, writeTask.Status);
                    Assert.Throws<SerializationException>(() => writeTask.ThrowIfFaulted(), "Cannot serialize a null 'entry'.");
                    mockStream.Verify(s => s.Close(), Times.Never());
                    mockStream.Verify(s => s.BeginWrite(It.IsAny<byte[]>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AsyncCallback>(), It.IsAny<object>()), Times.Never());
                });
        }

        // TODO: Uncomment the attribute below once the test exists.
        //[Fact]
        public override Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // TODO: Tracked by Issue #339, Needs an implementation
            throw new NotImplementedException();
        }

        // TODO: Uncomment the attribute below once the test exists.
        //[Fact]
        public override Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            // TODO: Tracked by Issue #339, Needs an implementation
            throw new NotImplementedException();
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsNotSupported_WithoutRequest()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var formatter = CreateFormatter(builder.GetEdmModel());

            Assert.Throws<NotSupportedException>(
                () => formatter.WriteToStreamAsync(typeof(Customer), new Customer(), new MemoryStream(), content: null, transportContext: null),
                "The OData formatter does not support writing client requests. This formatter instance must have an associated request.");
        }

        protected override ODataMediaTypeFormatter CreateFormatter()
        {
            return CreateFormatterWithRequest();
        }

        protected override MediaTypeHeaderValue CreateSupportedMediaType()
        {
            return new MediaTypeHeaderValue("application/atom+xml");
        }

        private ODataMediaTypeFormatter CreateFormatter(IEdmModel model)
        {
            return new ODataMediaTypeFormatter(model, new ODataPayloadKind[0]);
        }

        public ODataMediaTypeFormatter CreateFormatter(IEdmModel model, HttpRequestMessage request,
            params ODataPayloadKind[] payloadKinds)
        {
            return new ODataMediaTypeFormatter(model, payloadKinds, request);
        }

        public ODataMediaTypeFormatter CreateFormatterWithoutRequest()
        {
            return CreateFormatter(CreateModel());
        }

        public ODataMediaTypeFormatter CreateFormatterWithRequest()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://dummy/");
            HttpConfiguration configuration = new HttpConfiguration();
            configuration.AddFakeODataRoute();
            request.Properties["MS_HttpConfiguration"] = configuration;
            IEdmModel model = CreateModel();
            request.Properties["MS_ODataPath"] = new ODataPath(new EntitySetPathSegment(
                model.EntityContainers().Single().EntitySets().Single()));
            return CreateFormatter(model, request);
        }

        private static IEdmModel CreateModel()
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.Entity<SampleType>();
            model.EntitySet<SampleType>("sampleTypes");
            return model.GetEdmModel();
        }

        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get
            {
                return new MediaTypeHeaderValue[0];
            }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get
            {
                return new Encoding[0];
            }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get
            {
                return Encoding.UTF8.GetBytes(
                  @"<entry xml:base=""http://localhost/"" xmlns=""http://www.w3.org/2005/Atom"" xmlns:d=""http://schemas.microsoft.com/ado/2007/08/dataservices"" xmlns:m=""http://schemas.microsoft.com/ado/2007/08/dataservices/metadata"" xmlns:georss=""http://www.georss.org/georss"" xmlns:gml=""http://www.opengis.net/gml"">
                      <category term=""System.Net.Http.Formatting.SampleType"" scheme=""http://schemas.microsoft.com/ado/2007/08/dataservices/scheme"" />
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
}
