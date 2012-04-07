// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class RequestHeaderMappingTests
    {

        [Fact]
        [Trait("Description", "RequestHeaderMapping is public, and concrete.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(RequestHeaderMapping),
                TypeAssert.TypeProperties.IsPublicVisibleClass,
                typeof(MediaTypeMapping));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, MediaTypeHeaderValue) sets properties.")]
        public void Constructor(string headerName, string headerValue, MediaTypeHeaderValue mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, true, mediaType);
            Assert.Equal(headerName, mapping.HeaderName);
            Assert.Equal(headerValue, mapping.HeaderValue);
            Assert.Equal(StringComparison.CurrentCulture, mapping.HeaderValueComparison);
            Assert.Equal(true, mapping.IsValueSubstring);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "MediaType failed to set.");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, MediaTypeHeaderValue) throws with empty headerName.")]
        public void ConstructorThrowsWithEmptyHeaderName(MediaTypeHeaderValue mediaType, string headerName)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping(headerName, "value", StringComparison.CurrentCulture, false, mediaType), "headerName");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, MediaTypeHeaderValue) throws with empty headerValue.")]
        public void ConstructorThrowsWithEmptyHeaderValue(MediaTypeHeaderValue mediaType, string headerValue)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping("name", headerValue, StringComparison.CurrentCulture, false, mediaType), "headerValue");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, MediaTypeHeaderValue) throws with null MediaTypeHeaderValue.")]
        public void ConstructorThrowsWithNullMediaTypeHeaderValue(string headerName, string headerValue)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, false, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, MediaTypeHeaderValue) throws with invalid StringComparison.")]
        public void ConstructorThrowsWithInvalidStringComparison(string headerName, string headerValue, MediaTypeHeaderValue mediaType)
        {
            int invalidValue = 999;
            Assert.ThrowsInvalidEnumArgument(() => new RequestHeaderMapping(headerName, headerValue, (StringComparison)invalidValue, false, mediaType),
                "valueComparison", invalidValue, typeof(StringComparison));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, string) sets properties.")]
        public void Constructor1(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, true, mediaType);
            Assert.Equal(headerName, mapping.HeaderName);
            Assert.Equal(headerValue, mapping.HeaderValue);
            Assert.Equal(StringComparison.CurrentCulture, mapping.HeaderValueComparison);
            Assert.Equal(true, mapping.IsValueSubstring);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "MediaType failed to set.");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, string) throws with empty headerName.")]
        public void Constructor1ThrowsWithEmptyHeaderName(string mediaType, string headerName)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping(headerName, "value", StringComparison.CurrentCulture, false, mediaType), "headerName");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, string) throws with empty headerValue.")]
        public void Constructor1ThrowsWithEmptyHeaderValue(string mediaType, string headerValue)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping("name", headerValue, StringComparison.CurrentCulture, false, mediaType), "headerValue");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, string) throws with empty MediaTypeHeaderValue.")]
        public void Constructor1ThrowsWithEmptyMediaType(string headerName, string headerValue, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, false, mediaType), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "RequestHeaderMapping(string, string, StringComparison, bool, string) throws with invalid StringComparison.")]
        public void Constructor1ThrowsWithInvalidStringComparison(string headerName, string headerValue, string mediaType)
        {
            int invalidValue = 999;
            Assert.ThrowsInvalidEnumArgument(
                () => new RequestHeaderMapping(headerName, headerValue, (StringComparison)invalidValue, false, mediaType),
                "valueComparison", invalidValue, typeof(StringComparison));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(CommonUnitTestDataSets), "Bools")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) returns true when the HeaderName and HeaderValue are in the request.")]
        public void TryMatchMediaTypeReturnsTrueWithNameAndValueInRequest(string headerName, string headerValue, string mediaType, bool subset)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.Ordinal, subset, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(headerName, headerValue);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
            typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) returns true when the HeaderName and a HeaderValue subset are in the request.")]
        public void TryMatchMediaTypeReturnsTrueWithNameAndValueSubsetInRequest(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.Ordinal, true, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(headerName, "prefix" + headerValue);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName, headerValue + "postfix");
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName, "prefix" + headerValue + "postfix");
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
           typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
           typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
           typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) returns false when HeaderName is not in the request.")]
        public void TryMatchMediaTypeReturnsFalseWithNameNotInRequest(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.Ordinal, false, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add("prefix" + headerName, headerValue);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName + "postfix", headerValue);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add("prefix" + headerName + "postfix", headerValue);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
           typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
           typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
           typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) returns false when HeaderValue is not in the request.")]
        public void TryMatchMediaTypeReturnsFalseWithValueNotInRequest(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.Ordinal, false, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Add(headerName, "prefix" + headerValue);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName, headerValue + "postfix");
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));

            request = new HttpRequestMessage();
            request.Headers.Add(headerName, "prefix" + headerValue + "postfix");
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
           typeof(HttpUnitTestDataSets), "LegalHttpHeaderNames",
           typeof(HttpUnitTestDataSets), "LegalHttpHeaderValues",
           typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "TryMatchMediaType(HttpResponseMessage) throws with a null HttpRequestMessage.")]
        public void TryMatchMediaTypeThrowsWithNullHttpRequestMessage(string headerName, string headerValue, string mediaType)
        {
            RequestHeaderMapping mapping = new RequestHeaderMapping(headerName, headerValue, StringComparison.CurrentCulture, true, mediaType);
            Assert.ThrowsArgumentNull(() => mapping.TryMatchMediaType(request: null), "request");
        }
    }
}
