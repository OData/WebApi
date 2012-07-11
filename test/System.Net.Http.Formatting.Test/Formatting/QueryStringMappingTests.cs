// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class QueryStringMappingTests
    {
        public static IEnumerable<string> UriStringsWithoutQuery
        {
            get
            {
                return HttpUnitTestDataSets.UriStrings.Where((s) => !s.Contains('?'));
            }
        }

        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(QueryStringMapping),
                TypeAssert.TypeProperties.IsPublicVisibleClass,
                typeof(MediaTypeMapping));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        public void Constructor(string queryStringParameterName, string queryStringParameterValue, MediaTypeHeaderValue mediaType)
        {
            QueryStringMapping mapping = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
            Assert.Equal(queryStringParameterName, mapping.QueryStringParameterName);
            Assert.Equal(queryStringParameterValue, mapping.QueryStringParameterValue);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "MediaType failed to set.");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void ConstructorThrowsWithEmptyQueryParameterName(MediaTypeHeaderValue mediaType, string queryStringParameterName)
        {
            Assert.ThrowsArgumentNull(() => new QueryStringMapping(queryStringParameterName, "json", mediaType), "queryStringParameterName");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void ConstructorThrowsWithEmptyQueryParameterValue(MediaTypeHeaderValue mediaType, string queryStringParameterValue)
        {
            Assert.ThrowsArgumentNull(() => new QueryStringMapping("query", queryStringParameterValue, mediaType), "queryStringParameterValue");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues")]
        public void ConstructorThrowsWithNullMediaTypeHeaderValue(string queryStringParameterName, string queryStringParameterValue)
        {
            Assert.ThrowsArgumentNull(() => new QueryStringMapping(queryStringParameterName, queryStringParameterValue, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void Constructor1(string queryStringParameterName, string queryStringParameterValue, string mediaType)
        {
            QueryStringMapping mapping = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
            Assert.Equal(queryStringParameterName, mapping.QueryStringParameterName);
            Assert.Equal(queryStringParameterValue, mapping.QueryStringParameterValue);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "MediaType failed to set.");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void Constructor1ThrowsWithEmptyQueryParameterName(string mediaType, string queryStringParameterName)
        {
            Assert.ThrowsArgumentNull(() => new QueryStringMapping(queryStringParameterName, "json", mediaType), "queryStringParameterName");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void Constructor1ThrowsWithEmptyQueryParameterValue(string mediaType, string queryStringParameterValue)
        {
            Assert.ThrowsArgumentNull(() => new QueryStringMapping("query", queryStringParameterValue, mediaType), "queryStringParameterValue");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void Constructor1ThrowsWithEmptyMediaType(string queryStringParameterName, string queryStringParameterValue, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new QueryStringMapping(queryStringParameterName, queryStringParameterValue, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(QueryStringMappingTests), "UriStringsWithoutQuery")]
        public void TryMatchMediaTypeReturnsMatchWithQueryStringParameterNameAndValueInUri(string queryStringParameterName, string queryStringParameterValue, string mediaType, string uriBase)
        {

            QueryStringMapping mapping = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
            string uri = uriBase + "?" + queryStringParameterName + "=" + queryStringParameterValue;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(QueryStringMappingTests), "UriStringsWithoutQuery")]
        public void TryMatchMediaTypeReturnsZeroWithQueryStringParameterNameNotInUri(string queryStringParameterName, string queryStringParameterValue, string mediaType, string uriBase)
        {
            QueryStringMapping mapping = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
            string uri = uriBase + "?" + "not" + queryStringParameterName + "=" + queryStringParameterValue;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(QueryStringMappingTests), "UriStringsWithoutQuery")]
        public void TryMatchMediaTypeReturnsZeroWithQueryStringParameterValueNotInUri(string queryStringParameterName, string queryStringParameterValue, string mediaType, string uriBase)
        {
            QueryStringMapping mapping = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
            string uri = uriBase + "?" + queryStringParameterName + "=" + "not" + queryStringParameterValue;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeThrowsWithNullHttpRequestMessage(string queryStringParameterName, string queryStringParameterValue, string mediaType)
        {
            QueryStringMapping mapping = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
            Assert.ThrowsArgumentNull(() => mapping.TryMatchMediaType(request: null), "request");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterNames",
            typeof(HttpUnitTestDataSets), "LegalQueryStringParameterValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeThrowsWithNullUriInHttpRequestMessage(string queryStringParameterName, string queryStringParameterValue, string mediaType)
        {
            QueryStringMapping mapping = new QueryStringMapping(queryStringParameterName, queryStringParameterValue, mediaType);
            string errorMessage = Error.Format(Properties.Resources.NonNullUriRequiredForMediaTypeMapping, typeof(QueryStringMapping).Name);
            Assert.Throws<InvalidOperationException>(() => mapping.TryMatchMediaType(new HttpRequestMessage()), errorMessage);
        }

        [Theory]
        [InlineData("nAmE", "VaLuE", "name=value")]
        [InlineData("Format", "Xml", "format=xml")]
        public void TryMatchMediaTypeIsCaseInsensitive(string name, string value, string query)
        {
            QueryStringMapping mapping = new QueryStringMapping(name, value, "application/json");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?" + query);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }
    }
}
