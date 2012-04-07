// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Headers;
using Xunit;
using Xunit.Extensions;

namespace System.Net.Http.Formatting
{
    public class MediaTypeHeaderValueExtensionsTests
    {
        public static IEnumerable<object[]> EqualValues
        {
            get
            {
                // These values are all equal
                yield return new[] { "text/xml", "text/xml" };
                yield return new[] { "text/xml", "TEXT/XML" };
                yield return new[] { "text/xml; charset=utf-8", "text/xml; charset=utf-8" };
                yield return new[] { "text/xml; parameter=value", "text/xml; parameter=value" };
                yield return new[] { "text/xml; charset=utf-8; parameter=value", "text/xml; parameter=value; charset=utf-8" };

                yield return new[] { "text/*", "text/*" };
                yield return new[] { "text/*", "TEXT/*" };
                yield return new[] { "text/*; charset=utf-8", "text/*; charset=utf-8" };
                yield return new[] { "text/*; parameter=value", "text/*; parameter=value" };
                yield return new[] { "text/*; charset=utf-8; parameter=value", "text/*; parameter=value; charset=utf-8" };

                yield return new[] { "*/*", "*/*" };
                yield return new[] { "*/*; charset=utf-8", "*/*; charset=utf-8" };
                yield return new[] { "*/*; parameter=value", "*/*; parameter=value" };
                yield return new[] { "*/*; charset=utf-8; parameter=value", "*/*; parameter=value; charset=utf-8" };
            }
        }

        public static IEnumerable<object[]> NonEqualValues
        {
            get
            {
                // These values are all subsets. If compared in reverse they are all non-subsets.
                yield return new[] { "text/xml", "text/xml; charset=utf-8" };
                yield return new[] { "text/xml", "TEXT/XML; charset=utf-8" };
                yield return new[] { "text/xml", "text/xml; parameter=value" };
                yield return new[] { "text/xml; charset=utf-8", "text/xml; parameter=value; charset=utf-8" };

                yield return new[] { "text/*", "text/*; charset=utf-8" };
                yield return new[] { "text/*", "TEXT/*; charset=utf-8" };
                yield return new[] { "text/*", "text/*; parameter=value" };
                yield return new[] { "text/*; charset=utf-8", "text/*; parameter=value; charset=utf-8" };

                yield return new[] { "text/xml", "text/*" };
                yield return new[] { "text/xml", "text/*; charset=utf-8" };
                yield return new[] { "text/xml", "TEXT/*; charset=utf-8" };
                yield return new[] { "text/xml", "text/*; parameter=value" };
                yield return new[] { "text/xml; charset=utf-8", "text/*; parameter=value; charset=utf-8" };

                yield return new[] { "*/*", "*/*; charset=utf-8" };
                yield return new[] { "*/*", "*/*; parameter=value" };
                yield return new[] { "*/*; charset=utf-8", "*/*; parameter=value; charset=utf-8" };

                yield return new[] { "text/*", "*/*" };
                yield return new[] { "text/*", "*/*; charset=utf-8" };
                yield return new[] { "text/*", "*/*; charset=utf-8" };
                yield return new[] { "text/*", "*/*; parameter=value" };
                yield return new[] { "text/*; charset=utf-8", "*/*; parameter=value; charset=utf-8" };

                yield return new[] { "text/xml", "*/*" };
                yield return new[] { "text/xml", "*/*; charset=utf-8" };
                yield return new[] { "text/xml", "*/*; parameter=value" };
                yield return new[] { "text/xml; charset=utf-8", "*/*; parameter=value; charset=utf-8" };
            }
        }

        public static IEnumerable<object[]> NonOverlappingValues
        {
            get
            {
                // These values are all value1 < value2 regardless of which value is first and second
                // We do this asymmetric sorting algorithm to ensure that subsets are always <=0.
                yield return new[] { "text/xml", "application/xml" };
                yield return new[] { "text/xml", "APPLICATION/XML" };
                yield return new[] { "text/xml", "application/xml; charset=utf-8" };
                yield return new[] { "text/xml; charset=utf-8", "application/xml; charset=utf-8" };
                yield return new[] { "text/xml; charset=utf-8", "application/xml; parameter=value" };

                yield return new[] { "text/xml", "text/plain" };
                yield return new[] { "text/xml", "TEXT/PLAIN" };
                yield return new[] { "text/xml", "text/plain; charset=utf-8" };
                yield return new[] { "text/xml; charset=utf-8", "text/plain; charset=utf-8" };
                yield return new[] { "text/xml; charset=utf-8", "text/plain; parameter=value" };
            }
        }

        public static IEnumerable<object[]> MediaRanges
        {
            get
            {
                yield return new[] { "text/*" };
                yield return new[] { "TEXT/*" };
                yield return new[] { "application/*; charset=utf-8" };
                yield return new[] { "APPLICATION/*; charset=utf-8" };
                yield return new[] { "*/*" };
                yield return new[] { "*/*; charset=utf-8" };
                yield return new[] { "*/*; charset=utf-8" };
            }
        }

        public static IEnumerable<object[]> NonMediaRanges
        {
            get
            {
                yield return new[] { "text/plain" };
                yield return new[] { "TEXT/XML" };
                yield return new[] { "application/xml; charset=utf-8" };
                yield return new[] { "APPLICATION/xml; charset=utf-8" };
            }
        }

        [Theory]
        [PropertyData("EqualValues")]
        public void IsSubsetOf_ReturnsTrueForEqualValues(string mediaType1, string mediaType2)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue1 = MediaTypeHeaderValue.Parse(mediaType1);
            MediaTypeHeaderValue mediaTypeHeaderValue2 = MediaTypeHeaderValue.Parse(mediaType2);

            Assert.True(mediaTypeHeaderValue1.IsSubsetOf(mediaTypeHeaderValue2));
            Assert.True(mediaTypeHeaderValue2.IsSubsetOf(mediaTypeHeaderValue1));
        }

        [Theory]
        [PropertyData("NonEqualValues")]
        public void IsSubsetOf_ReturnsTrueForNonEqualValues(string mediaType1, string mediaType2)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue1 = MediaTypeHeaderValue.Parse(mediaType1);
            MediaTypeHeaderValue mediaTypeHeaderValue2 = MediaTypeHeaderValue.Parse(mediaType2);

            Assert.True(mediaTypeHeaderValue1.IsSubsetOf(mediaTypeHeaderValue2));
            Assert.False(mediaTypeHeaderValue2.IsSubsetOf(mediaTypeHeaderValue1));
        }

        [Theory]
        [PropertyData("NonOverlappingValues")]
        public void IsSubsetOf_ReturnsFalseForNonOverlappingValues(string mediaType1, string mediaType2)
        {
            MediaTypeHeaderValue mediaTypeHeaderValue1 = MediaTypeHeaderValue.Parse(mediaType1);
            MediaTypeHeaderValue mediaTypeHeaderValue2 = MediaTypeHeaderValue.Parse(mediaType2);

            Assert.False(mediaTypeHeaderValue1.IsSubsetOf(mediaTypeHeaderValue2));
        }

        [Theory]
        [PropertyData("MediaRanges")]
        public void IsMediaRange_ReturnsTrueForMediaRanges(string mediaRange)
        {
            MediaTypeHeaderValue mediaTypeHeader = MediaTypeHeaderValue.Parse(mediaRange);
            Assert.True(mediaTypeHeader.IsMediaRange());
        }

        [Theory]
        [PropertyData("NonMediaRanges")]
        public void IsMediaRange_ReturnsFalseForNonMediaRanges(string nonMediaRange)
        {
            MediaTypeHeaderValue mediaTypeHeader = MediaTypeHeaderValue.Parse(nonMediaRange);
            Assert.False(mediaTypeHeader.IsMediaRange());
        }
    }
}
