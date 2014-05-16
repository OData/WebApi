// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.TestCommon;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.OData.Formatter
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
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json"),
                MediaTypeHeaderValue.Parse("application/xml")
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
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json")
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
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json")
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
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json")
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
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json")
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
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json")
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
                f => f.CanWriteType(typeof(ODataServiceDocument)));

            // Act
            IEnumerable<MediaTypeHeaderValue> supportedMediaTypes = serviceDocumentFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            IEnumerable<MediaTypeHeaderValue> expectedMediaTypes = new MediaTypeHeaderValue[]
            {
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json")
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
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json")
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
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=full"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none;odata.streaming=false"),
                MediaTypeHeaderValue.Parse("application/json;odata.metadata=none"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=true"),
                MediaTypeHeaderValue.Parse("application/json;odata.streaming=false"),
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
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json; odata.metadata=minimal; odata.streaming=true"), mediaType);
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
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json; odata.metadata=minimal; odata.streaming=true"), mediaType);
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
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json; odata.metadata=minimal; odata.streaming=true"), mediaType);
        }

        [Fact]
        public void TestCreate_ServiceDocument_DefaultContentType()
        {
            // Arrange
            IEdmModel model = CreateModel();
            Type serviceDocumentType = typeof(ODataServiceDocument);

            // Act
            MediaTypeHeaderValue mediaType = GetDefaultContentType(model, serviceDocumentType);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"), mediaType);
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

        [Theory]
        [InlineData("json", "application/json")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dtrue", "application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dfalse", "application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal", "application/json;odata.metadata=minimal")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dtrue", "application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dfalse", "application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull", "application/json;odata.metadata=full")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dtrue", "application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dfalse", "application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone", "application/json;odata.metadata=none")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue", "application/json;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.streaming%3dfalse", "application/json;odata.streaming=false")]
        [InlineData("application%2fjson", "application/json")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue%3bodata.metadata%3dminimal", "application/json;odata.streaming=true;odata.metadata=minimal")]
        public void TestCreate_DollarFormat_Feed(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type feedType = typeof(IEnumerable<SampleType>);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, feedType, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
        }

        [Theory]
        [InlineData("json", "application/json")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dtrue", "application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dfalse", "application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal", "application/json;odata.metadata=minimal")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dtrue", "application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dfalse", "application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull", "application/json;odata.metadata=full")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dtrue", "application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dfalse", "application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone", "application/json;odata.metadata=none")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue", "application/json;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.streaming%3dfalse", "application/json;odata.streaming=false")]
        [InlineData("application%2fjson", "application/json")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue%3bodata.metadata%3dminimal", "application/json;odata.streaming=true;odata.metadata=minimal")]
        public void TestCreate_DollarFormat_Entry(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type entryType = typeof(SampleType);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, entryType, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
        }

        [Theory]
        [InlineData("json", "application/json")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dtrue", "application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dfalse", "application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal", "application/json;odata.metadata=minimal")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dtrue", "application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dfalse", "application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull", "application/json;odata.metadata=full")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dtrue", "application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dfalse", "application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone", "application/json;odata.metadata=none")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue", "application/json;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.streaming%3dfalse", "application/json;odata.streaming=false")]
        [InlineData("application%2fjson", "application/json")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue%3bodata.metadata%3dminimal", "application/json;odata.streaming=true;odata.metadata=minimal")]
        public void TestCreate_DollarFormat_Property(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type propertyType = typeof(int);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, propertyType, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
        }

        [Theory]
        [InlineData("json", "application/json")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dtrue", "application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dfalse", "application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal", "application/json;odata.metadata=minimal")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dtrue", "application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dfalse", "application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull", "application/json;odata.metadata=full")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dtrue", "application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dfalse", "application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone", "application/json;odata.metadata=none")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue", "application/json;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.streaming%3dfalse", "application/json;odata.streaming=false")]
        [InlineData("application%2fjson", "application/json")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue%3bodata.metadata%3dminimal", "application/json;odata.streaming=true;odata.metadata=minimal")]
        public void TestCreate_DollarFormat_EntityReferenceLink(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type linkType = typeof(Uri);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, linkType, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
        }

        [Theory]
        [InlineData("json", "application/json")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dtrue", "application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dfalse", "application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal", "application/json;odata.metadata=minimal")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dtrue", "application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dfalse", "application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull", "application/json;odata.metadata=full")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dtrue", "application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dfalse", "application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone", "application/json;odata.metadata=none")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue", "application/json;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.streaming%3dfalse", "application/json;odata.streaming=false")]
        [InlineData("application%2fjson", "application/json")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue%3bodata.metadata%3dminimal", "application/json;odata.streaming=true;odata.metadata=minimal")]
        public void TestCreate_DollarFormat_Collection(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type collectionType = typeof(IEnumerable<int>);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, collectionType, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
        }

        [Theory]
        [InlineData("json", "application/json")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dtrue", "application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dfalse", "application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal", "application/json;odata.metadata=minimal")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dtrue", "application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dfalse", "application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull", "application/json;odata.metadata=full")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dtrue", "application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dfalse", "application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone", "application/json;odata.metadata=none")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue", "application/json;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.streaming%3dfalse", "application/json;odata.streaming=false")]
        [InlineData("application%2fjson", "application/json")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue%3bodata.metadata%3dminimal", "application/json;odata.streaming=true;odata.metadata=minimal")]
        public void TestCreate_DollarFormat_ServiceDocument(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type serviceDocumentType = typeof(ODataServiceDocument);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, serviceDocumentType, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
        }

        [Theory]
        [InlineData("xml", "application/xml")]
        [InlineData("application%2fxml", "application/xml")]
        public void TestCreate_DollarFormat_MetadataDocument(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type modelType = typeof(IEdmModel);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, modelType, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
        }

        [Theory]
        [InlineData("json", "application/json")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dtrue", "application/json;odata.metadata=minimal;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal%3bodata.streaming%3dfalse", "application/json;odata.metadata=minimal;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dminimal", "application/json;odata.metadata=minimal")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dtrue", "application/json;odata.metadata=full;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull%3bodata.streaming%3dfalse", "application/json;odata.metadata=full;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dfull", "application/json;odata.metadata=full")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dtrue", "application/json;odata.metadata=none;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone%3bodata.streaming%3dfalse", "application/json;odata.metadata=none;odata.streaming=false")]
        [InlineData("application%2fjson%3bodata.metadata%3dnone", "application/json;odata.metadata=none")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue", "application/json;odata.streaming=true")]
        [InlineData("application%2fjson%3bodata.streaming%3dfalse", "application/json;odata.streaming=false")]
        [InlineData("application%2fjson", "application/json")]
        [InlineData("application%2fjson%3bodata.streaming%3dtrue%3bodata.metadata%3dminimal", "application/json;odata.streaming=true;odata.metadata=minimal")]
        public void TestCreate_DollarFormat_Error(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            Type errorType = typeof(ODataError);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, errorType, dollarFormatValue);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse(expectedMediaType), mediaType);
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
            ODataConventionModelBuilder model = ODataModelBuilderMocks.GetModelBuilderMock<ODataConventionModelBuilder>();
            model.EntityType<T>();
            return model.GetEdmModel();
        }

        private static MediaTypeHeaderValue GetDefaultContentType(IEdmModel model, Type type)
        {
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            IEnumerable<ODataMediaTypeFormatter> feedFormatters = formatters.Where(f => f.CanWriteType(type));
            IContentNegotiator negotiator = new DefaultContentNegotiator(false);
            MediaTypeHeaderValue mediaType;

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.RequestUri = new Uri("http://any");
                ContentNegotiationResult result = negotiator.Negotiate(type, request, formatters);
                mediaType = result.MediaType;
            }

            // We don't care what the charset is for these tests.
            mediaType.Parameters.Remove(mediaType.Parameters.Single(p => p.Name == "charset"));

            return mediaType;
        }

        private static MediaTypeHeaderValue GetContentTypeFromQueryString(IEdmModel model, Type type, string dollarFormat)
        {
            IEnumerable<ODataMediaTypeFormatter> formatters = CreateProductUnderTest(model);
            IEnumerable<ODataMediaTypeFormatter> feedFormatters = formatters.Where(f => f.CanWriteType(type));
            IContentNegotiator negotiator = new DefaultContentNegotiator(false);
            MediaTypeHeaderValue mediaType;

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.RequestUri = new Uri("http://any/?$format=" + dollarFormat);
                ContentNegotiationResult result = negotiator.Negotiate(type, request, formatters);
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
            request.RequestUri = new Uri("http://any");
            request.ODataProperties().Model = model;
            return ODataMediaTypeFormatters.Create().Select(f => f.GetPerRequestFormatterInstance(typeof(void), request, null) as ODataMediaTypeFormatter);
        }
    }
}
