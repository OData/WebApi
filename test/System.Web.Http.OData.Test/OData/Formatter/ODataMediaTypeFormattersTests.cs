// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
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
                new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
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
                MediaTypeHeaderValue.Parse("application/atom+xml;type=feed"),
                MediaTypeHeaderValue.Parse("application/atom+xml"),
                MediaTypeHeaderValue.Parse("application/atom+xml;type=entry"),
                MediaTypeHeaderValue.Parse("application/xml"),
                MediaTypeHeaderValue.Parse("application/atomsvc+xml"),
                MediaTypeHeaderValue.Parse("text/xml"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose")
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
                MediaTypeHeaderValue.Parse("application/atom+xml;type=entry"),
                MediaTypeHeaderValue.Parse("application/atom+xml"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
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

        private static IEnumerable<ODataMediaTypeFormatter> CreateProductUnderTest()
        {
            IEdmModel model = CreateModel();
            return CreateProductUnderTest(model);
        }

        private static IEnumerable<ODataMediaTypeFormatter> CreateProductUnderTest(IEdmModel model)
        {
            return ODataMediaTypeFormatters.Create(model);
        }
    }
}
