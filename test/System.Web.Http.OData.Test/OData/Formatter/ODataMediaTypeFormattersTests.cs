// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.Http.OData.Formatter
{
    public class ODataMediaTypeFormattersTests
    {
        [Fact]
        public void TestSupportedEncodings()
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
        public void TestSupportedMediaTypes()
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
                MediaTypeHeaderValue.Parse("application/atom+xml"),
                MediaTypeHeaderValue.Parse("application/json;odata=verbose"),
                MediaTypeHeaderValue.Parse("application/xml")
            };

            Assert.True(expectedMediaTypes.SequenceEqual(supportedMediaTypes));
        }

        private static IEdmModel CreateModel()
        {
            return new Mock<IEdmModel>().Object;
        }

        private static IEnumerable<ODataMediaTypeFormatter> CreateProductUnderTest()
        {
            IEdmModel model = CreateModel();
            return ODataMediaTypeFormatters.Create(model);
        }
    }
}
