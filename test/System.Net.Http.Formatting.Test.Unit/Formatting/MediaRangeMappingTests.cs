// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class MediaRangeMappingTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(MediaRangeMapping),
                TypeAssert.TypeProperties.IsPublicVisibleClass,
                typeof(MediaTypeMapping));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaRangeValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        public void Constructor(MediaTypeHeaderValue mediaRange, MediaTypeHeaderValue mediaType)
        {
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRange, mediaType);
            Assert.MediaType.AreEqual(mediaRange, mapping.MediaRange, "MediaRange failed to set.");
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "MediaType failed to set.");
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        public void ConstructorThrowsWithNullMediaRange(MediaTypeHeaderValue mediaType)
        {
            Assert.ThrowsArgumentNull(() => new MediaRangeMapping((MediaTypeHeaderValue)null, mediaType), "mediaRange");
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "LegalMediaRangeValues")]
        public void ConstructorThrowsWithNullMediaType(MediaTypeHeaderValue mediaRange)
        {
            Assert.ThrowsArgumentNull(() => new MediaRangeMapping(mediaRange, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "IllegalMediaRangeValues",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        public void ConstructorThrowsWithIllegalMediaRange(MediaTypeHeaderValue mediaRange, MediaTypeHeaderValue mediaType)
        {
            string errorMessage = Error.Format(Properties.Resources.InvalidMediaRange, mediaRange.MediaType);
            Assert.Throws<InvalidOperationException>(() => new MediaRangeMapping(mediaRange, mediaType), errorMessage);
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaRangeStrings",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void Constructor1(string mediaRange, string mediaType)
        {
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRange, mediaType);
            Assert.MediaType.AreEqual(mediaRange, mapping.MediaRange, "MediaRange failed to set.");
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "MediaType failed to set.");
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "EmptyStrings",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void Constructor1ThrowsWithEmptyMediaRange(string mediaRange, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new MediaRangeMapping(mediaRange, mediaType), "mediaRange");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaRangeStrings",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        public void Constructor1ThrowsWithEmptyMediaType(string mediaRange, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new MediaRangeMapping(mediaRange, mediaType), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "IllegalMediaRangeStrings",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void Constructor1ThrowsWithIllegalMediaRange(string mediaRange, string mediaType)
        {
            string errorMessage = Error.Format(Properties.Resources.InvalidMediaRange, mediaRange);
            Assert.Throws<InvalidOperationException>(() => new MediaRangeMapping(mediaRange, mediaType), errorMessage);
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaRangeStrings",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeThrowsWithNullHttpRequestMessage(string mediaRange, string mediaType)
        {
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRange, mediaType);
            Assert.ThrowsArgumentNull(() => mapping.TryMatchMediaType(request: null), "request");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaRangeStrings",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeReturnsOneWithMediaRangeInAcceptHeader(string mediaRange, string mediaType)
        {
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRange, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaRange));
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "MediaRangeValuesWithQuality",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        public void TryMatchMediaTypeReturnsQualityWithMediaRangeWithQualityInAcceptHeader(MediaTypeWithQualityHeaderValue mediaRangeWithQuality, MediaTypeHeaderValue mediaType)
        {
            MediaTypeWithQualityHeaderValue mediaRangeWithNoQuality = new MediaTypeWithQualityHeaderValue(mediaRangeWithQuality.MediaType);
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRangeWithNoQuality, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Accept.Add(mediaRangeWithQuality);
            double quality = mediaRangeWithQuality.Quality.Value;
            Assert.Equal(quality, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalMediaRangeStrings",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        public void TryMatchMediaTypeReturnsFalseWithMediaRangeNotInAcceptHeader(string mediaRange, string mediaType)
        {
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRange, mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }
    }
}
