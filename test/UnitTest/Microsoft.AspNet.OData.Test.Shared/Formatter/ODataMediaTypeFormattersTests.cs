// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
#if NETCOREAPP3_1
#else
    using Microsoft.AspNetCore.Mvc.Internal;
#endif
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
using MediaTypeHeaderValue = System.Net.Http.Headers.MediaTypeHeaderValue;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Moq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test.Formatter
{
    public class ODataMediaTypeFormattersTests
    {
        [Fact]
        public void TestCreate_CombinedFormatters_SupportedEncodings()
        {
            // Arrange
            var formatters = CreateOutputFormatters();
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
            var formatters = CreateOutputFormatters();
            Assert.NotNull(formatters); // Guard assertion

            // Act
            var supportedMediaTypes = formatters.SelectMany(f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
                "application/xml"
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForFeed_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateOutputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var feedFormatters = formatters.Where(f => CanWriteType(f, typeof(IEnumerable<SampleType>), request));

            // Act
            var supportedMediaTypes = feedFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForEntry_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateOutputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var entryFormatters = formatters.Where(f => CanWriteType(f, typeof(SampleType), request));

            // Act
            var supportedMediaTypes = entryFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForProperty_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateOutputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var propertyFormatters = formatters.Where(f => CanWriteType(f, typeof(int), request));

            // Act
            var supportedMediaTypes = propertyFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForEntityReferenceLink_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateOutputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var entityReferenceLinkFormatters = formatters.Where(f => CanWriteType(f, typeof(Uri), request));

            // Act
            var supportedMediaTypes = entityReferenceLinkFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForCollection_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateOutputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var collectionFormatters = formatters.Where(f => CanWriteType(f, typeof(IEnumerable<int>), request));

            // Act
            var supportedMediaTypes = collectionFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForServiceDocument_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateOutputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var serviceDocumentFormatters = formatters.Where(f => CanWriteType(f, typeof(ODataServiceDocument), request));

            // Act
            var supportedMediaTypes = serviceDocumentFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForMetadataDocument_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateOutputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var metadataDocumentFormatters = formatters.Where(f => CanWriteType(f, typeof(IEdmModel), request));

            // Act
            var supportedMediaTypes = metadataDocumentFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/xml",
                "application/json",
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false"
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForError_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateOutputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var errorFormatters = formatters.Where(f => CanWriteType(f, typeof(ODataError), request));

            // Act
            var supportedMediaTypes = errorFormatters.SelectMany(
                f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
            });

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        [Fact]
        public void TestCreate_FormattersForParameter_SupportedMediaTypes()
        {
            // Arrange
            IEdmModel model = CreateModelWithEntity<SampleType>();
            var request = RequestFactory.CreateFromModel(model, "http://any", "odata", new ODataPath());
            var formatters = CreateInputFormatters(model);
            Assert.NotNull(formatters); // Guard assertion
            var parameterFormatters = formatters.Where(f => CanReadType(f, typeof(ODataActionParameters), request));

            // Act
            var supportedMediaTypes = parameterFormatters.SelectMany(f => f.SupportedMediaTypes).Distinct();

            // Assert
            var expectedMediaTypes = GetMediaTypes(new string[]
            {
                "application/json;odata.metadata=minimal;odata.streaming=true",
                "application/json;odata.metadata=minimal;odata.streaming=false",
                "application/json;odata.metadata=minimal",
                "application/json;odata.metadata=full;odata.streaming=true",
                "application/json;odata.metadata=full;odata.streaming=false",
                "application/json;odata.metadata=full",
                "application/json;odata.metadata=none;odata.streaming=true",
                "application/json;odata.metadata=none;odata.streaming=false",
                "application/json;odata.metadata=none",
                "application/json;odata.streaming=true",
                "application/json;odata.streaming=false",
                "application/json",
            });

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
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"), mediaType);
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
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"), mediaType);
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
            Assert.Equal(MediaTypeHeaderValue.Parse("application/json;odata.metadata=minimal;odata.streaming=true"), mediaType);
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
            Type metadataDocumentType = typeof(IEdmModel);

            // Act
            MediaTypeHeaderValue mediaType = GetDefaultContentType(model, metadataDocumentType);

            // Assert
            Assert.Equal(MediaTypeHeaderValue.Parse("application/xml"), mediaType);
        }

        [Theory]
        [InlineData("xml", "application/xml")]
        [InlineData("application%2fxml", "application/xml")]
        [InlineData("json", "application/json")]
        [InlineData("application%2fjson", "application/json")]
        public void TestCreate_MetadataDocument_DollarFormat(string dollarFormatValue, string expectedMediaType)
        {
            // Arrange
            IEdmModel model = CreateModel();
            Type metadataDocumentType = typeof(IEdmModel);

            // Act
            MediaTypeHeaderValue mediaType = GetContentTypeFromQueryString(model, metadataDocumentType, dollarFormatValue);

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

#if NETCORE
        [Fact]
        public void TestGet_InputFormatterMetadata_DontError()
        {
            // Arrange
            var formatters = CreateInputFormatters().ToList();
            Assert.NotNull(formatters); // Guard assertion

            // Act
            foreach (var formatter in formatters)
            {
                formatter.SupportedMediaTypes.Clear();
            }

            // Assert
            foreach (var formatter in formatters)
            {
                var exception = Record.Exception(() => formatter.GetSupportedContentTypes("application/json", typeof(string)));
                Assert.Null(exception);
            }
        }

        [Fact]
        public void TestGet_OutputFormatterMetadata_DontError()
        {
            // Arrange
            var formatters = CreateOutputFormatters().ToList();
            Assert.NotNull(formatters); // Guard assertion

            // Act
            foreach (var formatter in formatters)
            {
                formatter.SupportedMediaTypes.Clear();
            }

            // Assert
            foreach (var formatter in formatters)
            {
                var exception = Record.Exception(() => formatter.GetSupportedContentTypes("application/json", typeof(string)));
                Assert.Null(exception);
            }
        }

#endif
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

#if NETCORE
        private static IEnumerable<ODataOutputFormatter> CreateOutputFormatters(IEdmModel model = null)
        {
            // Model is not used in AspNetCore.
            return ODataOutputFormatterFactory.Create();
        }

        private static IEnumerable<ODataInputFormatter> CreateInputFormatters(IEdmModel model = null)
        {
            // Model is not used in AspNetCore.
            return ODataInputFormatterFactory.Create();
        }

        private static IEnumerable<string> GetMediaTypes(string[] mediaTypes)
        {
            return mediaTypes;
        }

        private static bool CanWriteType(ODataOutputFormatter formatter, Type type, HttpRequest request)
        {
            var context = new OutputFormatterWriteContext(
                request.HttpContext,
                new TestHttpResponseStreamWriterFactory().CreateWriter,
                objectType: type,
                @object: null);

            return formatter.CanWriteResult(context);
        }

        private static bool CanReadType(ODataInputFormatter formatter, Type type, HttpRequest request)
        {
            var context = new InputFormatterContext(
                request.HttpContext,
                "modelName",
                new ModelStateDictionary(),
                new EmptyModelMetadataProvider().GetMetadataForType(type),
                (stream, encoding) => new StreamReader(stream, encoding));

            return formatter.CanRead(context);
        }

        private static MediaTypeHeaderValue GetDefaultContentType(IEdmModel model, Type type)
        {
            return GetContentTypeFromQueryString(model, type, null);
        }

        private static MediaTypeHeaderValue GetContentTypeFromQueryString(IEdmModel model, Type type, string dollarFormat)
        {
            var formatters = CreateOutputFormatters(model);
            var path = new ODataPath();
            var request = string.IsNullOrEmpty(dollarFormat)
                ? RequestFactory.CreateFromModel(model, "http://any", "OData", path)
                : RequestFactory.CreateFromModel(model, "http://any/?$format=" + dollarFormat, "OData", path);

            var context = new OutputFormatterWriteContext(
                request.HttpContext,
                new TestHttpResponseStreamWriterFactory().CreateWriter,
                type,
                new MemoryStream());

            foreach (var formatter in formatters)
            {
                context.ContentType = new StringSegment();
                context.ContentTypeIsServerDefined = false;

                if (formatter.CanWriteResult(context))
                {
                    MediaTypeHeaderValue mediaType = MediaTypeHeaderValue.Parse(context.ContentType.ToString());

                    // We don't care what the charset is for these tests.
                    if (mediaType.Parameters.Where(p => p.Name == "charset").Any())
                    {
                        mediaType.Parameters.Remove(mediaType.Parameters.Single(p => p.Name == "charset"));
                    }

                    return mediaType;
                }
            }

            return null;
        }

#if NETCOREAPP3_1
        public class TestHttpResponseStreamWriterFactory
#else
        public class TestHttpResponseStreamWriterFactory : IHttpResponseStreamWriterFactory
#endif
        {
            public const int DefaultBufferSize = 16 * 1024;

            public TextWriter CreateWriter(Stream stream, Encoding encoding)
            {
                return new HttpResponseStreamWriter(stream, encoding, DefaultBufferSize);
            }
        }
#else
        private static IEnumerable<ODataMediaTypeFormatter> CreateOutputFormatters()
        {
            IEdmModel model = CreateModel();
            return CreateOutputFormatters(model);
        }

        private static IEnumerable<ODataMediaTypeFormatter> CreateOutputFormatters(IEdmModel model)
        {
            var request = RequestFactory.CreateFromModel(model, "http://any");
            return ODataMediaTypeFormatters.Create().Select(f => f.GetPerRequestFormatterInstance(typeof(void), request, null) as ODataMediaTypeFormatter);
        }

        private static IEnumerable<ODataMediaTypeFormatter> CreateInputFormatters()
        {
            return CreateOutputFormatters();
        }

        private static IEnumerable<ODataMediaTypeFormatter> CreateInputFormatters(IEdmModel model)
        {
            return CreateOutputFormatters(model);
        }

        private static IEnumerable<MediaTypeHeaderValue> GetMediaTypes(string[] mediaTypes)
        {
            return mediaTypes.Select(m => MediaTypeHeaderValue.Parse(m));
        }

        private static bool CanWriteType(ODataMediaTypeFormatter formatter, Type type, HttpRequestMessage request)
        {
            // request is not used in AspNet.
            return formatter.CanWriteType(type);
        }

        private static bool CanReadType(ODataMediaTypeFormatter formatter, Type type, HttpRequestMessage request)
        {
            // request is not used in AspNet.
            return formatter.CanReadType(type);
        }

        private static MediaTypeHeaderValue GetDefaultContentType(IEdmModel model, Type type)
        {
            return GetContentTypeFromQueryString(model, type, null);
        }

        private static MediaTypeHeaderValue GetContentTypeFromQueryString(IEdmModel model, Type type, string dollarFormat)
        {
            var formatters = CreateOutputFormatters(model);
            var feedFormatters = formatters.Where(f => f.CanWriteType(type));
            IContentNegotiator negotiator = new DefaultContentNegotiator(false);
            MediaTypeHeaderValue mediaType;

            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.RequestUri = string.IsNullOrEmpty(dollarFormat)
                    ? request.RequestUri = new Uri("http://any")
                    : new Uri("http://any/?$format=" + dollarFormat);

                request.EnableODataDependencyInjectionSupport(model);
                ContentNegotiationResult result = negotiator.Negotiate(type, request, formatters);
                mediaType = result.MediaType;
            }

            // We don't care what the charset is for these tests.
            mediaType.Parameters.Remove(mediaType.Parameters.Single(p => p.Name == "charset"));

            return mediaType;
        }
#endif
    }
}
