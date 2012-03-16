using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class UriPathExtensionMappingTests
    {
        [Fact]
        [Trait("Description", "UriPathExtensionMapping is public, and concrete.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(
                typeof(UriPathExtensionMapping),
                TypeAssert.TypeProperties.IsPublicVisibleClass,
                typeof(MediaTypeMapping));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "UriPathExtensionMapping(string, string) sets UriPathExtension and MediaType.")]
        public void Constructor(string uriPathExtension, string mediaType)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Assert.Equal(uriPathExtension, mapping.UriPathExtension);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "Failed to set MediaType.");
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "EmptyStrings",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "UriPathExtensionMapping(string, string) throws if the UriPathExtensions parameter is null.")]
        public void ConstructorThrowsWithEmptyUriPathExtension(string uriPathExtension, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new UriPathExtensionMapping(uriPathExtension, mediaType), "uriPathExtension");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(CommonUnitTestDataSets), "EmptyStrings")]
        [Trait("Description", "UriPathExtensionMapping(string, string) throws if the MediaType (string) parameter is empty.")]
        public void ConstructorThrowsWithEmptyMediaType(string uriPathExtension, string mediaType)
        {
            Assert.ThrowsArgumentNull(() => new UriPathExtensionMapping(uriPathExtension, mediaType), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        [Trait("Description", "UriPathExtensionMapping(string, MediaTypeHeaderValue) sets UriPathExtension and MediaType.")]
        public void Constructor1(string uriPathExtension, MediaTypeHeaderValue mediaType)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Assert.Equal(uriPathExtension, mapping.UriPathExtension);
            Assert.MediaType.AreEqual(mediaType, mapping.MediaType, "Failed to set MediaType.");
        }

        [Theory]
        [TestDataSet(
            typeof(CommonUnitTestDataSets), "EmptyStrings",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeHeaderValues")]
        [Trait("Description", "UriPathExtensionMapping(string, MediaTypeHeaderValue) throws if the UriPathExtensions parameter is null.")]
        public void Constructor1ThrowsWithEmptyUriPathExtension(string uriPathExtension, MediaTypeHeaderValue mediaType)
        {
            Assert.ThrowsArgumentNull(() => new UriPathExtensionMapping(uriPathExtension, mediaType), "uriPathExtension");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions")]
        [Trait("Description", "UriPathExtensionMapping(string, MediaTypeHeaderValue) constructor throws if the mediaType parameter is null.")]
        public void Constructor1ThrowsWithNullMediaType(string uriPathExtension)
        {
            Assert.ThrowsArgumentNull(() => new UriPathExtensionMapping(uriPathExtension, (MediaTypeHeaderValue)null), "mediaType");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(HttpUnitTestDataSets), "UriStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) returns 1.0 when the extension is in the Uri.")]
        public void TryMatchMediaTypeReturnsMatchWithExtensionInUri(string uriPathExtension, string mediaType, string baseUriString)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Uri baseUri = new Uri(baseUriString);
            Uri uri = new Uri(baseUri, "x." + uriPathExtension);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(HttpUnitTestDataSets), "UriStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) returns 1.0 when the extension is in the Uri but differs in case")]
        public void TryMatchMediaTypeReturnsMatchWithExtensionInUriDifferCase(string uriPathExtension, string mediaType, string baseUriString)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension.ToUpperInvariant(), mediaType);
            Uri baseUri = new Uri(baseUriString);
            Uri uri = new Uri(baseUri, "x." + uriPathExtension.ToLowerInvariant());
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            Assert.Equal(1.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(HttpUnitTestDataSets), "UriStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) returns 0.0 when the extension is not in the Uri.")]
        public void TryMatchMediaTypeReturnsZeroWithExtensionNotInUri(string uriPathExtension, string mediaType, string baseUriString)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Uri baseUri = new Uri(baseUriString);
            Uri uri = new Uri(baseUri, "x.");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings",
            typeof(HttpUnitTestDataSets), "UriStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) returns 0.0 when the uri contains the extension but does not end with it.")]
        public void TryMatchMediaTypeReturnsZeroWithExtensionNotLastInUri(string uriPathExtension, string mediaType, string baseUriString)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Uri baseUri = new Uri(baseUriString);
            Uri uri = new Uri(baseUri, "x." + uriPathExtension + "z");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            Assert.Equal(0.0, mapping.TryMatchMediaType(request));
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) throws if the request is null.")]
        public void TryMatchMediaTypeThrowsWithNullHttpRequestMessage(string uriPathExtension, string mediaType)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            Assert.ThrowsArgumentNull(() => mapping.TryMatchMediaType(request: null), "request");
        }

        [Theory]
        [TestDataSet(
            typeof(HttpUnitTestDataSets), "LegalUriPathExtensions",
            typeof(HttpUnitTestDataSets), "LegalMediaTypeStrings")]
        [Trait("Description", "TryMatchMediaType(HttpRequestMessage) throws if the Uri in the request is null.")]
        public void TryMatchMediaTypeThrowsWithNullUriInHttpRequestMessage(string uriPathExtension, string mediaType)
        {
            UriPathExtensionMapping mapping = new UriPathExtensionMapping(uriPathExtension, mediaType);
            string errorMessage = RS.Format(Properties.Resources.NonNullUriRequiredForMediaTypeMapping, typeof(UriPathExtensionMapping).Name);
            Assert.Throws<InvalidOperationException>(() => mapping.TryMatchMediaType(new HttpRequestMessage()), errorMessage);
        }
    }
}
