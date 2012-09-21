// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Hosting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.TestCommon.Models;
using System.Web.Http.Routing;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypeFormatterTests : MediaTypeFormatterTestBase<ODataMediaTypeFormatter>
    {
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

        [Fact(Skip = "OData formatter doesn't support writing nulls")]
        public override Task WriteToStreamAsync_WhenObjectIsNull_WritesDataButDoesNotCloseStream()
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "Tracked by Issue #339, Needs an implementation")]
        public override Task ReadFromStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            throw new NotImplementedException();
        }

        [Fact(Skip = "Tracked by Issue #339, Needs an implementation")]
        public override Task WriteToStreamAsync_UsesCorrectCharacterEncoding(string content, string encoding, bool isDefaultEncoding)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsNotSupported_WithoutRequest()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            var formatter = new ODataMediaTypeFormatter(builder.GetEdmModel());

            Assert.Throws<NotSupportedException>(
                () => formatter.WriteToStreamAsync(typeof(Customer), new Customer(), new MemoryStream(), content: null, transportContext: null),
                "The OData formatter does not support writing client requests. This formatter instance must have an associated request.");
        }

        [Fact]
        public void ODataFormatter_DefaultPatchKeyMode_Is_Ignore()
        {
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter();
            Assert.Equal(PatchKeyMode.Ignore, formatter.PatchKeyMode);
        }

        [Fact]
        public void ReadFromStreamAsync_PassesPatchKeyModeToTheDeserializers()
        {
            ODataDeserializerContext context = null;
            Mock<ODataDeserializer> deserializer = new Mock<ODataDeserializer>(ODataPayloadKind.Entry);
            deserializer
                .Setup(d => d.Read(It.IsAny<ODataMessageReader>(), It.IsAny<ODataDeserializerContext>()))
                .Callback((ODataMessageReader reader, ODataDeserializerContext deserializerContext) =>
                {
                    context = deserializerContext;
                });
            Mock<Type> type = new Mock<Type>();
            Mock<ODataDeserializerProvider> deserializerProvider = new Mock<ODataDeserializerProvider>(EdmCoreModel.Instance);
            deserializerProvider.Setup(d => d.GetODataDeserializer(type.Object)).Returns(deserializer.Object);
            ODataMediaTypeFormatter formatter = new ODataMediaTypeFormatter(deserializerProvider.Object, new DefaultODataSerializerProvider(EdmCoreModel.Instance));
            formatter.PatchKeyMode = PatchKeyMode.Patch;

            Mock<Stream> stream = new Mock<Stream>();
            Mock<HttpContent> content = new Mock<HttpContent>();
            Mock<IFormatterLogger> formatterLogger = new Mock<IFormatterLogger>();

            formatter.ReadFromStreamAsync(type.Object, stream.Object, content.Object, formatterLogger.Object);

            Assert.Equal(context.PatchKeyMode, PatchKeyMode.Patch);
        }

        public override ODataMediaTypeFormatter CreateFormatter()
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.Entity<SampleType>();
            var formatter = new ODataMediaTypeFormatter(model.GetEdmModel());
            var request = new HttpRequestMessage(HttpMethod.Get, "http://dummy/");
            request.Properties["MS_HttpConfiguration"] = new HttpConfiguration();
            formatter.Request = request;
            return formatter;
        }

        public override IEnumerable<MediaTypeHeaderValue> ExpectedSupportedMediaTypes
        {
            get
            {
                yield return MediaTypeHeaderValue.Parse("application/atom+xml");
                yield return MediaTypeHeaderValue.Parse("application/json;odata=verbose");
                yield return MediaTypeHeaderValue.Parse("application/xml");
            }
        }

        public override IEnumerable<Encoding> ExpectedSupportedEncodings
        {
            get
            {
                yield return new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);
                yield return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
            }
        }

        public override byte[] ExpectedSampleTypeByteRepresentation
        {
            get
            {
                return ExpectedSupportedEncodings.ElementAt(0).GetBytes(
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
