// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Formatter.Serialization;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypeFormattersTests
    {
        [Fact]
        public void TestCreate_CombinedFormatters_SupportedEncodings()
        {
            // Arrange
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest();
            Assert.NotNull(formatters); // Guard assertion

            // Act
            IEnumerable<Encoding> supportedEncodings = formatters.SelectMany(f => f.SupportedEncodings).Distinct();

            // Assert
            IEnumerable<Encoding> expectedEncodings = new Encoding[]
            {
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
                new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true)
            };

            Assert.True(expectedEncodings.SequenceEqual(supportedEncodings));
        }

        [Fact]
        public void TestCreate_CombinedFormatters_SupportedMediaTypes()
        {
            // Arrange
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest();
            Assert.NotNull(formatters); // Guard assertion

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = formatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/atomsvc+xml"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/atom+xml;type=feed"),
                MediaTypeHeaderValue.Parse("application/atom+xml"),
                MediaTypeHeaderValue.Parse("application/atom+xml;type=entry"),
                MediaTypeHeaderValue.Parse("application/xml"),
                MediaTypeHeaderValue.Parse("text/xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForFeed_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> feedFormatters = formatters.Where(
                f => f.CanWriteType(typeof(IEnumerable<SampleType>)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = feedFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/atom+xml;type=feed"),
                MediaTypeHeaderValue.Parse("application/atom+xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForEntry_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> entryFormatters = formatters.Where(
                f => f.CanWriteType(typeof(SampleType)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = entryFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/atom+xml;type=entry"),
                MediaTypeHeaderValue.Parse("application/atom+xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForProperty_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> propertyFormatters = formatters.Where(
                f => f.CanWriteType(typeof(int)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = propertyFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/xml"),
                MediaTypeHeaderValue.Parse("text/xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForEntityReferenceLink_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> entityReferenceLinkFormatters = formatters.Where(
                f => f.CanWriteType(typeof(Uri)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = entityReferenceLinkFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/xml"),
                MediaTypeHeaderValue.Parse("text/xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForCollection_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> collectionFormatters = formatters.Where(
                f => f.CanWriteType(typeof(IEnumerable<int>)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = collectionFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/xml"),
                MediaTypeHeaderValue.Parse("text/xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForServiceDocument_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> serviceDocumentFormatters = formatters.Where(
                f => f.CanWriteType(typeof(ODataWorkspace)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = serviceDocumentFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/atomsvc+xml"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForMetadataDocument_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> metadataDocumentFormatters = formatters.Where(
                f => f.CanWriteType(typeof(IEdmModel)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = metadataDocumentFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForError_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> errorFormatters = formatters.Where(
                f => f.CanWriteType(typeof(ODataError)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = errorFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForParameter_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            Assert.NotNull(formatters); // Guard assertion
            IEnumerable<ODataMediaTypeFormatter> parameterFormatters = formatters.Where(
                f => f.CanReadType(typeof(ODataActionParameters)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = parameterFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=minimalmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=fullmetadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata=nometadata"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/json;streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;streaming=false"),
                MediaTypeHeaderValue.Parse("application/json")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_Feed_DefaultContentType()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type feedType = typeof(IEnumerable<SampleType>);

            // Act
            MediaTypeHeaderValue mediaType = GetDefaultContentType(model, feedType);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json; odata=minimalmetadata; streaming=true"), mediaType);
        }

        [Fact]
        public void TestCreate_Entry_DefaultContentType()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type entryType = typeof(SampleType);

            // Act
            MediaTypeHeaderValue mediaType = GetDefaultContentType(model, entryType);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json; odata=minimalmetadata; streaming=true"), mediaType);
        }

        [Fact]
        public void TestCreate_Property_DefaultContentType()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type propertyType = typeof(int);

            // Act
            MediaTypeHeaderValue mediaType = GetDefaultContentType(model, propertyType);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json; odata=minimalmetadata; streaming=true"), mediaType);
        }

        [Fact]
        public void TestCreate_ServiceDocument_DefaultContentType()
        {
            // Arrange
            IEdmModel model = CreateModel();
            Type serviceDocumentType = typeof(ODataWorkspace);

            // Act
            MediaTypeHeaderValue mediaType = GetDefaultContentType(model, serviceDocumentType);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse("application/atomsvc+xml"), mediaType);
        }

        [Fact]
        public void TestCreate_MetadataDocument_DefaultContentType()
        {
            // Arrange
            IEdmModel model = CreateModel();
            Type serviceDocumentType = typeof(IEdmModel);

            // Act
            MediaTypeHeaderValue mediaType = GetDefaultContentType(model, serviceDocumentType);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse("application/xml"), mediaType);
        }

        [Fact]
        public void Create_UsesDefaultSerializerProviderInstance()
        {
            var formatters = ODataMediaTypeFormatters.Create();

            Assert.Same(formatters.First().SerializerProvider, DefaultODataSerializerProvider.Instance);
        }

        [Fact]
        public void Create_UsesDefaultDeserializerProviderInstance()
        {
            var formatters = ODataMediaTypeFormatters.Create();

            Assert.Same(formatters.First().DeserializerProvider, DefaultODataDeserializerProvider.Instance);
        }

        private static IEdmModel CreateModel()
        {
            return new Mock<IEdmModel>().Object;
        }

        private static IEdmModel CreateModelWithEntity<T>() where T : class
        {
            ODataConventionModelBuilder model = new ODataConventionModelBuilder();
            model.Entity<T>();
            return model.GetEdmModel();
        }

        private static MediaTypeHeaderValue GetDefaultContentType(IEdmModel model, Type type)
        {
            // Arrange
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            IEnumerable<ODataMediaTypeFormatter> feedFormatters = formatters.Where(f => f.CanWriteType(type));
            IContentNegotiator negotiator = new DefaultContentNegotiator(false);
            MediaTypeHeaderValue mediaType;

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Act
                ContentNegotiationResult result = negotiator.Negotiate(type, request, formatters);

                // Collect results for Assert
                mediaType = result.MediaType;
            }

            // We don't care what the charset is for these tests.
            mediaType.Parameters.Remove(mediaType.Parameters.Single(p => p.Name == "charset"));

            return mediaType;
        }

        private static IEnumerable<ODataMediaTypeFormatter> CreateProductUnderTest()
        {
            IEdmModel model = CreateModel();
            return CreateProductUnderTest(model);
        }

        private static IEnumerable<ODataMediaTypeFormatter> CreateProductUnderTest(IEdmModel model)
        {
            HttpRequestMessage request = new HttpRequestMessage();
            request.ODataProperties().Model = model;
            return ODataMediaTypeFormatters.Create().Select(f => f.GetPerRequestFormatterInstance(typeof(void), request, null) as ODataMediaTypeFormatter);
        }
    }
}
