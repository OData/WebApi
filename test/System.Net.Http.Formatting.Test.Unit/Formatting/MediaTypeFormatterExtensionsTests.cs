// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting.Mocks;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class MediaTypeFormatterExtensionsTests
    {
        [Fact]
        [Trait("Description", "MediaTypeFormatterExtensionMethods is public and static.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(MediaTypeFormatterExtensions), TypeAssert.TypeProperties.IsPublicVisibleClass | TypeAssert.TypeProperties.IsStatic);
        }

        [Fact]
        [Trait("Description", "AddQueryStringMapping(MediaTypeFormatter, string, string, MediaTypeHeaderValue) throws for null 'this'.")]
        public void AddQueryStringMappingThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddQueryStringMapping("name", "value", new MediaTypeHeaderValue("application/xml")), "formatter");
        }

        [Fact]
        [Trait("Description", "AddQueryStringMapping(MediaTypeFormatter, string, string, string) throws for null 'this'.")]
        public void AddQueryStringMapping1ThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddQueryStringMapping("name", "value", "application/xml"), "formatter");
        }

        [Fact]
        [Trait("Description", "AddMediaRangeMapping(MediaTypeFormatter, MediaTypeHeaderValue, MediaTypeHeaderValue) throws for null 'this'.")]
        public void AddMediaRangeMappingThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddMediaRangeMapping(new MediaTypeHeaderValue("application/*"), new MediaTypeHeaderValue("application/xml")), "formatter");
        }

        [Fact]
        [Trait("Description", "AddMediaRangeMapping(MediaTypeFormatter, string, string) throws for null 'this'.")]
        public void AddMediaRangeMapping1ThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddMediaRangeMapping("application/*", "application/xml"), "formatter");
        }



        [Fact]
        [Trait("Description", "AddRequestHeaderMapping(MediaTypeFormatter, string, string, StringComparison, bool, MediaTypeHeaderValue) throws for null 'this'.")]
        public void AddRequestHeaderMappingThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddRequestHeaderMapping("name", "value", StringComparison.CurrentCulture, true, new MediaTypeHeaderValue("application/xml")), "formatter");
        }

        [Fact]
        [Trait("Description", "AddRequestHeaderMapping(MediaTypeFormatter, string, string, StringComparison, bool, MediaTypeHeaderValue) adds formatter on 'this'.")]
        public void AddRequestHeaderMappingAddsSuccessfully()
        {
            MediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Assert.Equal(0, formatter.MediaTypeMappings.Count);
            formatter.AddRequestHeaderMapping("name", "value", StringComparison.CurrentCulture, true, new MediaTypeHeaderValue("application/xml"));
            IEnumerable<RequestHeaderMapping> mappings = formatter.MediaTypeMappings.OfType<RequestHeaderMapping>();
            Assert.Equal(1, mappings.Count());
            RequestHeaderMapping mapping = mappings.ElementAt(0);
            Assert.Equal("name", mapping.HeaderName);
            Assert.Equal("value", mapping.HeaderValue);
            Assert.Equal(StringComparison.CurrentCulture, mapping.HeaderValueComparison);
            Assert.Equal(true, mapping.IsValueSubstring);
            Assert.Equal(new MediaTypeHeaderValue("application/xml"), mapping.MediaType);
        }

        [Fact]
        [Trait("Description", "AddRequestHeaderMapping(MediaTypeFormatter, string, string, StringComparison, bool, string) throws for null 'this'.")]
        public void AddRequestHeaderMapping1ThrowsWithNullThis()
        {
            MediaTypeFormatter formatter = null;
            Assert.ThrowsArgumentNull(() => formatter.AddRequestHeaderMapping("name", "value", StringComparison.CurrentCulture, true, "application/xml"), "formatter");
        }

        [Fact]
        [Trait("Description", "AddRequestHeaderMapping(MediaTypeFormatter, string, string, StringComparison, bool, string) adds formatter on 'this'.")]
        public void AddRequestHeaderMapping1AddsSuccessfully()
        {
            MediaTypeFormatter formatter = new MockMediaTypeFormatter();
            Assert.Equal(0, formatter.MediaTypeMappings.Count);
            formatter.AddRequestHeaderMapping("name", "value", StringComparison.CurrentCulture, true, "application/xml");
            IEnumerable<RequestHeaderMapping> mappings = formatter.MediaTypeMappings.OfType<RequestHeaderMapping>();
            Assert.Equal(1, mappings.Count());
            RequestHeaderMapping mapping = mappings.ElementAt(0);
            Assert.Equal("name", mapping.HeaderName);
            Assert.Equal("value", mapping.HeaderValue);
            Assert.Equal(StringComparison.CurrentCulture, mapping.HeaderValueComparison);
            Assert.Equal(true, mapping.IsValueSubstring);
            Assert.Equal(new MediaTypeHeaderValue("application/xml"), mapping.MediaType);
        }
    }
}
