// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class MediaTypeWithQualityHeaderValueComparerTests
    {
        public static IEnumerable<object[]> EqualValues
        {
            get
            {
                yield return new[] { "text/xml", "text/xml" };
                yield return new[] { "text/xml", "TEXT/XML" };
                yield return new[] { "text/plain", "text/xml" };
                yield return new[] { "text/*", "text/*" };
                yield return new[] { "text/*", "TEXT/*" };
                yield return new[] { "*/*", "*/*" };
                yield return new[] { "text/xml", "text/xml; charset=utf8" };
                yield return new[] { "text/xml", "text/xml; parameter=value" };
                yield return new[] { "text/xml; parameter=value", "text/xml; parameter=value" };
                yield return new[] { "text/xml; parameter1=value1", "text/xml; parameter2=value2" };
                yield return new[] { "text/xml; q=0.5", "text/xml; q=0.50" };
                yield return new[] { "application/xml; q=0.5", "text/xml; q=0.5" };
                yield return new[] { "application/xml; q=0.1", "text/xml; q=0.1" };
                yield return new[] { "application/xml; parameter=value1; q=0.5", "text/xml; parameter=value2; q=0.5" };
                yield return new[] { "text/xml", "text/xml;q=1" };
                yield return new[] { "text/xml", "text/xml; q=1" };
                yield return new[] { "text/xml", "text/xml;q=1.0" };
                yield return new[] { "text/xml", "text/xml; q=1.0" };
                yield return new[] { "text/xml", "text/xml; q=1.00000" };
                yield return new[] { "text/xml; q=0.5", "text/xml; q=0.5" };
                yield return new[] { "text/xml; q=1.0", "text/xml; q=1.0" };
                yield return new[] { "*/*", "*/*;q=1" };
                yield return new[] { "*/*", "*/*; q=1" };
                yield return new[] { "*/*", "*/*;q=1.0" };
                yield return new[] { "*/*", "*/*; q=1.0" };
                yield return new[] { "*/*; q=0.5", "*/*; q=0.5" };
                yield return new[] { "*/*; q=1.0", "*/*; q=1.0" };
                yield return new[] { "text/xml", "text/xml;q=1" };
                yield return new[] { "text/xml", "text/xml; q=1" };
                yield return new[] { "text/xml", "text/xml;q=1.0" };
                yield return new[] { "text/xml", "text/xml; q=1.0" };
            }
        }

        public static IEnumerable<object[]> NonEqualValues
        {
            get
            {
                yield return new[] { "text/plain; q=0.5", "text/plain; q=1.0" };
                yield return new[] { "text/plain; q=0.5", "text/xml; q=1.0" };
                yield return new[] { "text/*", "text/plain" };
                yield return new[] { "*/*", "text/xml" };
                yield return new[] { "*/*", "text/*" };
                yield return new[] { "*/*;q=0.5", "*/*;q=0.6" };
                yield return new[] { "*/*;q=0.5", "text/*;q=0.5" };
                yield return new[] { "*/*;q=1", "text/plain" };
                yield return new[] { "*/*; q=1", "text/plain" };
                yield return new[] { "*/*;q=1.0", "text/plain" };
                yield return new[] { "*/*; q=1.0", "text/plain" };
                yield return new[] { "*/*; q=0.5", "text/plain; q=0.5" };
                yield return new[] { "*/*; q=1.0", "text/plain; q=1.0" };
                yield return new[] { "*/*; q=0.5", "text/*; q=0.6" };
            }
        }

        public static IEnumerable<object[]> BeforeAfterSortedValues
        {
            get
            {
                yield return new[]
                {
                    new List<string>
                    {
                        "text/plain",
                        "text/plain;q=1.0",
                        "text/plain",
                        "text/plain;q=0",
                        "*/*;q=0.8",
                        "*/*;q=1",
                        "text/*;q=1",
                        "text/plain;q=0.8",
                        "text/*;q=0.8",
                        "text/*;q=0.6",
                        "text/*;q=1.0",
                        "*/*;q=0.4",
                        "text/plain;q=0.6",
                        "text/xml",
                    }, 
                    new List<string>
                    {
                        "text/plain",
                        "text/plain;q=1.0",
                        "text/plain",
                        "text/xml",
                        "text/*;q=1",
                        "text/*;q=1.0",
                        "*/*;q=1",
                        "text/plain;q=0.8",
                        "text/*;q=0.8",
                        "*/*;q=0.8",
                        "text/plain;q=0.6",
                        "text/*;q=0.6",
                        "*/*;q=0.4",
                        "text/plain;q=0",
                    }, 
                };
            }
        }

        [Fact]
        public void StaticComparer_Returns_SameInstance()
        {
            MediaTypeWithQualityHeaderValueComparer comparer1 = MediaTypeWithQualityHeaderValueComparer.QualityComparer;
            MediaTypeWithQualityHeaderValueComparer comparer2 = MediaTypeWithQualityHeaderValueComparer.QualityComparer;

            Assert.NotNull(comparer1);
            Assert.Same(comparer1, comparer2);
        }

        [Theory]
        [PropertyData("EqualValues")]
        public void ComparerReturnsZeroForEqualValues(string mediaType1, string mediaType2)
        {
            // Arrange
            MediaTypeWithQualityHeaderValueComparer comparer = MediaTypeWithQualityHeaderValueComparer.QualityComparer;

            // Act
            MediaTypeWithQualityHeaderValue mediaTypeHeaderValue1 = MediaTypeWithQualityHeaderValue.Parse(mediaType1);
            MediaTypeWithQualityHeaderValue mediaTypeHeaderValue2 = MediaTypeWithQualityHeaderValue.Parse(mediaType2);

            // Assert
            Assert.Equal(0, comparer.Compare(mediaTypeHeaderValue1, mediaTypeHeaderValue2));
            Assert.Equal(0, comparer.Compare(mediaTypeHeaderValue2, mediaTypeHeaderValue1));
        }

        [Theory]
        [PropertyData("NonEqualValues")]
        public void ComparerReturnsNonZeroForNonEqualValues(string mediaType1, string mediaType2)
        {
            // Arrange
            MediaTypeWithQualityHeaderValueComparer comparer = MediaTypeWithQualityHeaderValueComparer.QualityComparer;

            // Act
            MediaTypeWithQualityHeaderValue mediaTypeHeaderValue1 = MediaTypeWithQualityHeaderValue.Parse(mediaType1);
            MediaTypeWithQualityHeaderValue mediaTypeHeaderValue2 = MediaTypeWithQualityHeaderValue.Parse(mediaType2);

            // Assert
            Assert.Equal(-1, comparer.Compare(mediaTypeHeaderValue1, mediaTypeHeaderValue2));
            Assert.Equal(1, comparer.Compare(mediaTypeHeaderValue2, mediaTypeHeaderValue1));
        }

        [Theory]
        [PropertyData("BeforeAfterSortedValues")]
        public void ComparerSortsListCorrectly(List<string> unsorted, List<string> expectedSorted)
        {
            // Arrange
            IEnumerable<MediaTypeWithQualityHeaderValue> unsortedValues =
                unsorted.Select(u => MediaTypeWithQualityHeaderValue.Parse(u));

            IEnumerable<MediaTypeWithQualityHeaderValue> expectedSortedValues =
                expectedSorted.Select(u => MediaTypeWithQualityHeaderValue.Parse(u));

            // Act
            IEnumerable<MediaTypeWithQualityHeaderValue> actualSorted = unsortedValues.OrderByDescending(m => m, MediaTypeWithQualityHeaderValueComparer.QualityComparer);

            // Assert
            Assert.True(expectedSortedValues.SequenceEqual(actualSorted));
        }
    }
}
