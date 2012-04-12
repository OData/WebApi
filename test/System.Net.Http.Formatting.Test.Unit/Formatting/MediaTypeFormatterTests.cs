// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;
using Moq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class MediaTypeFormatterTests
    {
        private const string TestMediaType = "text/test";

        [Fact]
        [Trait("Description", "MediaTypeFormatter is public, abstract, and unsealed.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(MediaTypeFormatter), TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsAbstract);
        }

        [Fact]
        [Trait("Description", "MediaTypeFormatter() constructor (via derived class) sets SupportedMediaTypes and MediaTypeMappings.")]
        public void Constructor()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Collection<MediaTypeHeaderValue> supportedMediaTypes = formatter.SupportedMediaTypes;
            Assert.NotNull(supportedMediaTypes);
            Assert.Equal(0, supportedMediaTypes.Count);

            Collection<MediaTypeMapping> mappings = formatter.MediaTypeMappings;
            Assert.NotNull(mappings);
            Assert.Equal(0, mappings.Count);
        }

        [Fact]
        public void MaxCollectionKeySize_RoundTrips()
        {
            Assert.Reflection.IntegerProperty<MediaTypeFormatter, int>(
                null, 
                c => MediaTypeFormatter.MaxHttpCollectionKeys,
                expectedDefaultValue: 1000,
                minLegalValue: 1,
                illegalLowerValue: 0,
                maxLegalValue: null,
                illegalUpperValue: null,
                roundTripTestValue: 125);
        }

        [Fact]
        [Trait("Description", "SupportedMediaTypes is a mutable collection.")]
        public void SupportedMediaTypesIsMutable()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Collection<MediaTypeHeaderValue> supportedMediaTypes = formatter.SupportedMediaTypes;
            MediaTypeHeaderValue[] mediaTypes = HttpUnitTestDataSets.LegalMediaTypeHeaderValues.ToArray();
            foreach (MediaTypeHeaderValue mediaType in mediaTypes)
            {
                supportedMediaTypes.Add(mediaType);
            }

            Assert.True(mediaTypes.SequenceEqual(formatter.SupportedMediaTypes));
        }

        [Fact]
        [Trait("Description", "SupportedMediaTypes Add throws with a null media type.")]
        public void SupportedMediaTypesAddThrowsWithNullMediaType()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Collection<MediaTypeHeaderValue> supportedMediaTypes = formatter.SupportedMediaTypes;

            Assert.ThrowsArgumentNull(() => supportedMediaTypes.Add(null), "item");
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "LegalMediaRangeValues")]
        [Trait("Description", "SupportedMediaTypes Add throws with a media range.")]
        public void SupportedMediaTypesAddThrowsWithMediaRange(MediaTypeHeaderValue mediaType)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Collection<MediaTypeHeaderValue> supportedMediaTypes = formatter.SupportedMediaTypes;
            Assert.ThrowsArgument(() => supportedMediaTypes.Add(mediaType), "item", RS.Format(Properties.Resources.CannotUseMediaRangeForSupportedMediaType, typeof(MediaTypeHeaderValue).Name, mediaType.MediaType));
        }

        [Fact]
        [Trait("Description", "SupportedMediaTypes Insert throws with a null media type.")]
        public void SupportedMediaTypesInsertThrowsWithNullMediaType()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Collection<MediaTypeHeaderValue> supportedMediaTypes = formatter.SupportedMediaTypes;

            Assert.ThrowsArgumentNull(() => supportedMediaTypes.Insert(0, null), "item");
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "LegalMediaRangeValues")]
        [Trait("Description", "SupportedMediaTypes Insert throws with a media range.")]
        public void SupportedMediaTypesInsertThrowsWithMediaRange(MediaTypeHeaderValue mediaType)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Collection<MediaTypeHeaderValue> supportedMediaTypes = formatter.SupportedMediaTypes;

            Assert.ThrowsArgument(() => supportedMediaTypes.Insert(0, mediaType), "item", RS.Format(Properties.Resources.CannotUseMediaRangeForSupportedMediaType, typeof(MediaTypeHeaderValue).Name, mediaType.MediaType));
        }

        [Fact]
        [Trait("Description", "MediaTypeMappings is a mutable collection.")]
        public void MediaTypeMappingsIsMutable()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Collection<MediaTypeMapping> mappings = formatter.MediaTypeMappings;
            MediaTypeMapping[] standardMappings = HttpUnitTestDataSets.StandardMediaTypeMappings.ToArray();
            foreach (MediaTypeMapping mapping in standardMappings)
            {
                mappings.Add(mapping);
            }

            Assert.True(standardMappings.SequenceEqual(formatter.MediaTypeMappings));
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "StandardMediaTypesWithQuality")]
        [Trait("Description", "TryMatchSupportedMediaType(MediaTypeHeaderValue, out MediaTypeMatch) returns media type and quality.")]
        public void TryMatchSupportedMediaTypeWithQuality(MediaTypeWithQualityHeaderValue mediaTypeWithQuality)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            MediaTypeHeaderValue mediaTypeWithoutQuality = new MediaTypeHeaderValue(mediaTypeWithQuality.MediaType);
            formatter.SupportedMediaTypes.Add(mediaTypeWithoutQuality);
            MediaTypeMatch match;
            bool result = formatter.TryMatchSupportedMediaType(mediaTypeWithQuality, out match);
            Assert.True(result, String.Format("TryMatchSupportedMediaType should have succeeded for '{0}'.", mediaTypeWithQuality));
            Assert.NotNull(match);
            double quality = mediaTypeWithQuality.Quality.Value;
            Assert.Equal(quality, match.Quality);
            Assert.NotNull(match.MediaType);
            Assert.Equal(mediaTypeWithoutQuality.MediaType, match.MediaType.MediaType);
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "StandardMediaTypesWithQuality")]
        [Trait("Description", "TryMatchSupportedMediaType(MediaTypeHeaderValue, out MediaTypeMatch) returns cloned media type, not original.")]
        public void TryMatchSupportedMediaTypeReturnsClone(MediaTypeWithQualityHeaderValue mediaTypeWithQuality)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            MediaTypeHeaderValue mediaTypeWithoutQuality = new MediaTypeHeaderValue(mediaTypeWithQuality.MediaType);
            formatter.SupportedMediaTypes.Add(mediaTypeWithoutQuality);
            MediaTypeMatch match;
            bool result = formatter.TryMatchSupportedMediaType(mediaTypeWithQuality, out match);

            Assert.True(result);
            Assert.NotNull(match);
            Assert.NotNull(match.MediaType);
            Assert.NotSame(mediaTypeWithoutQuality, match.MediaType);
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "MediaRangeValuesWithQuality")]
        [Trait("Description", "TryMatchMediaTypeMapping(HttpRequestMessage, out MediaTypeMatch) returns media type and quality from media range with quality.")]
        public void TryMatchMediaTypeMappingWithQuality(MediaTypeWithQualityHeaderValue mediaRangeWithQuality)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            MediaTypeHeaderValue mediaRangeWithoutQuality = new MediaTypeHeaderValue(mediaRangeWithQuality.MediaType);
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/xml");
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRangeWithoutQuality, mediaType);
            formatter.MediaTypeMappings.Add(mapping);

            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Accept.Add(mediaRangeWithQuality);
            MediaTypeMatch match;
            bool result = formatter.TryMatchMediaTypeMapping(request, out match);
            Assert.True(result, String.Format("TryMatchMediaTypeMapping should have succeeded for '{0}'.", mediaRangeWithQuality));
            Assert.NotNull(match);
            double quality = mediaRangeWithQuality.Quality.Value;
            Assert.Equal(quality, match.Quality);
            Assert.NotNull(match.MediaType);
            Assert.Equal(mediaType.MediaType, match.MediaType.MediaType);
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "MediaRangeValuesWithQuality")]
        [Trait("Description", "TryMatchMediaTypeMapping(HttpRequestMessage, out MediaTypeMatch) returns a clone of the original media type.")]
        public void TryMatchMediaTypeMappingClonesMediaType(MediaTypeWithQualityHeaderValue mediaRangeWithQuality)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            MediaTypeHeaderValue mediaRangeWithoutQuality = new MediaTypeHeaderValue(mediaRangeWithQuality.MediaType);
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/xml");
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRangeWithoutQuality, mediaType);
            formatter.MediaTypeMappings.Add(mapping);

            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Accept.Add(mediaRangeWithQuality);
            MediaTypeMatch match;
            formatter.TryMatchMediaTypeMapping(request, out match);
            Assert.NotNull(match);
            Assert.NotNull(match.MediaType);
            Assert.NotSame(mediaType, match.MediaType);
        }

        [Fact]
        [Trait("Description", "SelectResponseMediaType(Type, HttpRequestMessage) matches based only on type.")]
        public void SelectResponseMediaTypeMatchesType()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            HttpRequestMessage request = new HttpRequestMessage();
            ResponseMediaTypeMatch match = formatter.SelectResponseMediaType(typeof(string), request);

            Assert.NotNull(match);
            Assert.Equal(ResponseFormatterSelectionResult.MatchOnCanWriteType, match.ResponseFormatterSelectionResult);
            Assert.Null(match.MediaTypeMatch.MediaType);
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        [Trait("Description", "SelectResponseMediaType(Type, HttpRequestMessage) matches media type from request content type.")]
        public void SelectResponseMediaTypeMatchesRequestContentType(MediaTypeHeaderValue mediaType)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            formatter.SupportedMediaTypes.Add(mediaType);
            HttpRequestMessage request = new HttpRequestMessage() { Content = new StringContent("fred") };
            request.Content.Headers.ContentType = mediaType;
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request };
            ResponseMediaTypeMatch match = formatter.SelectResponseMediaType(typeof(string), request);

            Assert.NotNull(match);
            Assert.Equal(ResponseFormatterSelectionResult.MatchOnRequestContentType, match.ResponseFormatterSelectionResult);
            Assert.NotNull(match.MediaTypeMatch.MediaType);
            Assert.Equal(mediaType.MediaType, match.MediaTypeMatch.MediaType.MediaType);
        }

        [TestDataSet(typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        [Trait("Description", "SelectResponseMediaType(Type, HttpRequestMessage) matches media type from response content type.")]
        public void SelectResponseMediaTypeMatchesResponseContentType(MediaTypeHeaderValue mediaType)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            formatter.SupportedMediaTypes.Add(mediaType);
            HttpRequestMessage request = new HttpRequestMessage();
            HttpResponseMessage response = new HttpResponseMessage() { RequestMessage = request, Content = new StringContent("fred") };
            response.Content.Headers.ContentType = mediaType;
            ResponseMediaTypeMatch match = formatter.SelectResponseMediaType(typeof(string), request);

            Assert.NotNull(match);
            Assert.Equal(ResponseFormatterSelectionResult.MatchOnResponseContentType, match.ResponseFormatterSelectionResult);
            Assert.NotNull(match.MediaTypeMatch.MediaType);
            Assert.Equal(mediaType.MediaType, match.MediaTypeMatch.MediaType.MediaType);
        }

        [Theory]
        [TestDataSet(typeof(HttpUnitTestDataSets), "StandardMediaTypesWithQuality")]
        [Trait("Description", "SelectResponseMediaType(Type, HttpRequestMessage) matches supported media type from accept headers.")]
        public void SelectResponseMediaTypeMatchesAcceptHeaderToSupportedMediaTypes(MediaTypeWithQualityHeaderValue mediaTypeWithQuality)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            MediaTypeHeaderValue mediaTypeWithoutQuality = new MediaTypeHeaderValue(mediaTypeWithQuality.MediaType);
            formatter.SupportedMediaTypes.Add(mediaTypeWithoutQuality);

            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Accept.Add(mediaTypeWithQuality);
            ResponseMediaTypeMatch match = formatter.SelectResponseMediaType(typeof(string), request);

            Assert.NotNull(match);
            Assert.Equal(ResponseFormatterSelectionResult.MatchOnRequestAcceptHeader, match.ResponseFormatterSelectionResult);
            double quality = mediaTypeWithQuality.Quality.Value;
            Assert.Equal(quality, match.MediaTypeMatch.Quality);
            Assert.NotNull(match.MediaTypeMatch.MediaType);
            Assert.Equal(mediaTypeWithoutQuality.MediaType, match.MediaTypeMatch.MediaType.MediaType);
        }

        [TestDataSet(typeof(HttpUnitTestDataSets), "MediaRangeValuesWithQuality")]
        [Trait("Description", "SelectResponseMediaType(Type, HttpRequestMessage) matches media type with quality from media type mapping.")]
        public void SelectResponseMediaTypeMatchesWithMediaTypeMapping(MediaTypeWithQualityHeaderValue mediaRangeWithQuality)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            MediaTypeHeaderValue mediaRangeWithoutQuality = new MediaTypeHeaderValue(mediaRangeWithQuality.MediaType);
            MediaTypeHeaderValue mediaType = new MediaTypeHeaderValue("application/xml");
            MediaRangeMapping mapping = new MediaRangeMapping(mediaRangeWithoutQuality, mediaType);
            formatter.MediaTypeMappings.Add(mapping);

            HttpRequestMessage request = new HttpRequestMessage();
            request.Headers.Accept.Add(mediaRangeWithQuality);
            ResponseMediaTypeMatch match = formatter.SelectResponseMediaType(typeof(string), request);

            Assert.NotNull(match);
            Assert.Equal(ResponseFormatterSelectionResult.MatchOnRequestWithMediaTypeMapping, match.ResponseFormatterSelectionResult);
            double quality = mediaRangeWithQuality.Quality.Value;
            Assert.Equal(quality, match.MediaTypeMatch.Quality);
            Assert.NotNull(match.MediaTypeMatch.MediaType);
            Assert.Equal(mediaType.MediaType, match.MediaTypeMatch.MediaType.MediaType);
        }

        public static IEnumerable<object[]> SelectCharacterEncodingTestData
        {
            get
            {
                yield return new object[]
                {
                    null,
                    new[] { "utf-8", "utf-16"},
                    "utf-8"
                };
                yield return new object[]
                {
                    null,
                    new[] { "utf-16", "utf-8"},
                    "utf-16"
                };
                yield return new object[]
                {
                    "utf-32",
                    new[] { "utf-8", "utf-16"},
                    "utf-8"
                };
                yield return new object[]
                {
                    "utf-32",
                    new[] { "utf-8", "utf-16", "utf-32"},
                    "utf-32"
                };
            }
        }

        public static IEnumerable<object[]> SelectResponseCharacterEncodingTestData
        {
            get
            {
                yield return new object[]
                {
                    new[] { "*;q=0.5", "utf-8;q=0.8", "utf-16;q=0.7" },
                    null,
                    new[] { "utf-16", "utf-8"},
                    "utf-8"
                };
                yield return new object[]
                {
                    new[] { "*;q=0.5", "utf-8;q=0.8", "utf-16;q=0.9" },
                    null,
                    new[] { "utf-8", "utf-16"},
                    "utf-16"
                };
                yield return new object[]
                {
                    new[] { "*;q=0.9", "utf-8;q=0.5", "utf-16;q=0.5" },
                    null,
                    new[] { "utf-8", "utf-16"},
                    "utf-8"
                };
                yield return new object[]
                {
                    new string[] { },
                    "utf-16",
                    new[] { "utf-8", "utf-16"},
                    "utf-16"
                };
                yield return new object[]
                {
                    new[] { "*;q=0.5" },
                    "utf-16",
                    new[] { "utf-8", "utf-16"},
                    "utf-8"
                };
                yield return new object[]
                {
                    new[] { "*;q=0.5", "utf-16;q=0.7", "utf-8;q=0.8" },
                    "utf-16",
                    new[] { "utf-8", "utf-16"},
                    "utf-8"
                };
            }
        }

        [Fact]
        public void SelectCharacterEncoding_ThrowsIfNoSupportedEncodings()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter { CallBase = true };
            HttpContent content = new StringContent("Hello World", Encoding.UTF8, "text/plain");

            // Act
            Assert.Throws<InvalidOperationException>(() => formatter.SelectCharacterEncoding(content.Headers));
        }

        [Theory]
        [PropertyData("SelectCharacterEncodingTestData")]
        public void SelectCharacterEncoding_ReturnsBestEncoding(string bodyEncoding, string[] supportedEncodings, string expectedEncoding)
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter { CallBase = true };

            foreach (string supportedEncoding in supportedEncodings)
            {
                formatter.SupportedEncodings.Add(Encoding.GetEncoding(supportedEncoding));
            }

            HttpContentHeaders contentHeaders = null;
            if (bodyEncoding != null)
            {
                Encoding bodyEnc = Encoding.GetEncoding(bodyEncoding);
                HttpContent content = new StringContent("Hello World", bodyEnc, "text/plain");
                contentHeaders = content.Headers;
            }

            // Act
            Encoding actualEncoding = formatter.SelectCharacterEncoding(contentHeaders);

            // Assert
            Encoding expectedEnc = expectedEncoding != null ? Encoding.GetEncoding(expectedEncoding) : null;
            Assert.Equal(expectedEnc, actualEncoding);
        }

        [Theory]
        [PropertyData("SelectResponseCharacterEncodingTestData")]
        public void SelectResponseCharacterEncoding_ReturnsBestEncodingBasedOnAcceptCharsetMatch(string[] acceptCharsetValues, string bodyEncoding, string[] supportedEncodings, string expectedEncoding)
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            HttpRequestMessage request = new HttpRequestMessage();
            foreach (var acceptCharsetValue in acceptCharsetValues)
            {
                request.Headers.AcceptCharset.Add(StringWithQualityHeaderValue.Parse(acceptCharsetValue));
            }

            foreach (var supportedEncoding in supportedEncodings)
            {
                Encoding supportedEnc = Encoding.GetEncoding(supportedEncoding);
                formatter.SupportedEncodings.Add(supportedEnc);
            }

            if (bodyEncoding != null)
            {
                Encoding bodyEnc = Encoding.GetEncoding(bodyEncoding);
                request.Method = HttpMethod.Post;
                request.Content = new StringContent("Hello World", bodyEnc, "text/plain");
            }

            // Act
            Encoding actualEncoding = formatter.SelectResponseCharacterEncoding(request);

            // Assert
            Encoding expectedEnc = expectedEncoding != null ? Encoding.GetEncoding(expectedEncoding) : null;
            Assert.Equal(expectedEnc, actualEncoding);
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "CanReadAs(Type, MediaTypeHeaderValue) returns true for all standard media types.")]
        public void CanReadAsReturnsTrue(Type variationType, object testData, string mediaType)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            string[] legalMediaTypeStrings = HttpUnitTestDataSets.LegalMediaTypeStrings.ToArray();
            foreach (string legalMediaType in legalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(legalMediaType));
            }

            MediaTypeHeaderValue contentType = new MediaTypeHeaderValue(mediaType);
            Assert.True(formatter.CanReadAs(variationType, contentType));
        }

        [Fact]
        [Trait("Description", "CanReadAs(Type, MediaTypeHeaderValue) throws with null type.")]
        public void CanReadAsThrowsWithNullType()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => formatter.CanReadAs(type: null, mediaType: null), "type");
        }

        [Fact]
        [Trait("Description", "CanReadAs(Type, MediaTypeHeaderValue) throws with null formatter context.")]
        public void CanReadAsThrowsWithNullMediaType()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Assert.ThrowsArgumentNull(() => formatter.CanReadAs(typeof(int), mediaType: null), "mediaType");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "CanWriteAs(Type, MediaTypeHeaderValue, out MediaTypeHeaderValue) returns true always for supported media types.")]
        public void CanWriteAsReturnsTrue(Type variationType, object testData)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            foreach (string mediaType in HttpUnitTestDataSets.LegalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            }

            MediaTypeHeaderValue matchedMediaType = null;
            Assert.True(formatter.CanWriteAs(variationType, formatter.SupportedMediaTypes[0], out matchedMediaType));
        }

        [Fact]
        [Trait("Description", "CanWriteAs(Type, MediaTypeHeaderValue, out MediaTypeHeaderValue) throws with null content.")]
        public void CanWriteAsThrowsWithNullContent()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            MediaTypeHeaderValue mediaType = null;
            Assert.ThrowsArgumentNull(() => formatter.CanWriteAs(typeof(int), null, out mediaType), "mediaType");
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "CanWriteAs(Type, MediaTypeHeaderValue, out MediaTypeHeaderValue) returns true always for supported media types.")]
        public void CanWriteAsUsingRequestReturnsTrue(Type variationType, object testData)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            foreach (string mediaType in HttpUnitTestDataSets.LegalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            }

            MediaTypeHeaderValue matchedMediaType = null;
            Assert.True(formatter.CanWriteAs(variationType, formatter.SupportedMediaTypes[0], out matchedMediaType));
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "CanReadType(Type) base implementation returns true for all types.")]
        public void CanReadTypeReturnsTrue(Type variationType, object testData, string mediaType)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            string[] legalMediaTypeStrings = HttpUnitTestDataSets.LegalMediaTypeStrings.ToArray();
            foreach (string mediaTypeTmp in legalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaTypeTmp));
            }

            // Invoke CanReadAs because it invokes CanReadType
            Assert.True(formatter.CanReadAs(variationType, new MediaTypeHeaderValue(mediaType)));
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "RepresentativeValueAndRefTypeTestDataCollection")]
        [Trait("Description", "CanWriteType() base implementation returns true always.")]
        public void CanWriteTypeReturnsTrue(Type variationType, object testData)
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter() { CallBase = true };
            foreach (string mediaType in HttpUnitTestDataSets.LegalMediaTypeStrings)
            {
                formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(mediaType));
            }

            MediaTypeHeaderValue matchedMediaType = null;
            Assert.True(formatter.CanWriteAs(variationType, formatter.SupportedMediaTypes[0], out matchedMediaType));
        }

        [Fact]
        public void ReadFromStreamAsync_ThrowsNotSupportedException()
        {
            var formatter = new Mock<MediaTypeFormatter> { CallBase = true }.Object;

            Assert.Throws<NotSupportedException>(() => formatter.ReadFromStreamAsync(null, null, null, null),
                "The media type formatter of type 'MediaTypeFormatterProxy' does not support reading because it does not implement the ReadFromStreamAsync method.");
        }

        [Fact]
        public void WriteToStreamAsync_ThrowsNotSupportedException()
        {
            var formatter = new Mock<MediaTypeFormatter> { CallBase = true }.Object;

            Assert.Throws<NotSupportedException>(() => formatter.WriteToStreamAsync(null, null, null, null, null),
                "The media type formatter of type 'MediaTypeFormatterProxy' does not support writing because it does not implement the WriteToStreamAsync method.");
        }

        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(string))]
        [InlineData(typeof(Nullable<int>))]
        public void GetDefaultValueForType_ReturnsNullForReferenceTypes(Type referenceType)
        {
            Assert.Null(MediaTypeFormatter.GetDefaultValueForType(referenceType));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(0)]
        [InlineData('a')]
        public void GetDefaultValueForType_ReturnsValueForValueTypes<T>(T value)
        {
            Type valueType = value.GetType();
            T defaultValue = default(T);
            Assert.Equal(defaultValue, MediaTypeFormatter.GetDefaultValueForType(valueType));
        }

        [Fact]
        public void GetDefaultValueForType_ReturnsValueForStruct()
        {
            TestStruct s = new TestStruct();

            TestStruct result = (TestStruct)MediaTypeFormatter.GetDefaultValueForType(typeof(TestStruct));

            Assert.Equal(s, result);
        }

        [Fact]
        public void SetDefaultContentHeaders_ThrowsOnNullType()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();
            Assert.ThrowsArgumentNull(() => formatter.SetDefaultContentHeaders(null, contentHeaders, TestMediaType), "type");
        }

        [Fact]
        public void SetDefaultContentHeaders_ThrowsOnNullHeaders()
        {
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Type type = typeof(object);
            Assert.ThrowsArgumentNull(() => formatter.SetDefaultContentHeaders(type, null, TestMediaType), "headers");
        }

        [Fact]
        public void SetDefaultContentHeaders_UsesNonNullMediaType()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Type type = typeof(object);
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();

            // Act
            formatter.SetDefaultContentHeaders(type, contentHeaders, TestMediaType);

            // Assert
            Assert.Equal(TestMediaType, contentHeaders.ContentType.MediaType);
        }

        [Fact]
        public void SetDefaultContentHeaders_UsesDefaultSupportedMediaType()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(TestMediaType));
            Type type = typeof(object);
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();

            // Act
            formatter.SetDefaultContentHeaders(type, contentHeaders, null);

            // Assert
            Assert.Equal(TestMediaType, contentHeaders.ContentType.MediaType);
        }

        [Fact]
        public void SetDefaultContentHeaders_UsesDefaultSupportedEncoding()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Encoding encoding = new UnicodeEncoding();
            formatter.SupportedEncodings.Add(encoding);
            Type type = typeof(object);
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();

            // Act
            formatter.SetDefaultContentHeaders(type, contentHeaders, TestMediaType);

            // Assert
            Assert.Equal(TestMediaType, contentHeaders.ContentType.MediaType);
            Assert.Equal(encoding.WebName, contentHeaders.ContentType.CharSet);
        }

        [Fact]
        public void SetDefaultContentHeaders_UsesDefaultSupportedMediaTypeAndEncoding()
        {
            // Arrange
            MockMediaTypeFormatter formatter = new MockMediaTypeFormatter();
            formatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue(TestMediaType));
            Encoding encoding = new UnicodeEncoding();
            formatter.SupportedEncodings.Add(encoding);
            Type type = typeof(object);
            HttpContentHeaders contentHeaders = FormattingUtilities.CreateEmptyContentHeaders();

            // Act
            formatter.SetDefaultContentHeaders(type, contentHeaders, null);

            // Assert
            Assert.Equal(TestMediaType, contentHeaders.ContentType.MediaType);
            Assert.Equal(encoding.WebName, contentHeaders.ContentType.CharSet);
        }

        public struct TestStruct
        {
            private int I;
            public TestStruct(int i)
            {
                I = i + 1;
            }
        }
    }
}
