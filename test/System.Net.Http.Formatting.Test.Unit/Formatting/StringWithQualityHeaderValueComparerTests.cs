// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http.Formatting
{
    public class StringWithQualityHeaderValueComparerTests
    {
        public static IEnumerable<object[]> EqualValues
        {
            get
            {
                yield return new[] { "value", "value" };
                yield return new[] { "value", "VALUE" };
                yield return new[] { "value", "value;q=1" };
                yield return new[] { "value", "value; q=1" };
                yield return new[] { "value", "value;q=1.0" };
                yield return new[] { "value", "value; q=1.0" };
                yield return new[] { "value", "value; q=1.00000" };
                yield return new[] { "value; q=0.5", "value; q=0.5" };
                yield return new[] { "value; q=1.0", "value; q=1.0" };
                yield return new[] { "*", "*" };
                yield return new[] { "*", "*;q=1" };
                yield return new[] { "*", "*; q=1" };
                yield return new[] { "*", "*;q=1.0" };
                yield return new[] { "*", "*; q=1.0" };
                yield return new[] { "*; q=0.5", "*; q=0.5" };
                yield return new[] { "*; q=1.0", "*; q=1.0" };
                yield return new[] { "value1", "value2" };
                yield return new[] { "value1", "value2;q=1" };
                yield return new[] { "value1", "value2; q=1" };
                yield return new[] { "value1", "value2;q=1.0" };
                yield return new[] { "value1", "value2; q=1.0" };
            }
        }

        public static IEnumerable<object[]> NonEqualValues
        {
            get
            {
                yield return new[] { "value; q=0.5", "value; q=1.0" };
                yield return new[] { "value1; q=0.5", "value2; q=1.0" };
                yield return new[] { "*", "value1" };
                yield return new[] { "*;q=1", "value1" };
                yield return new[] { "*; q=1", "value1" };
                yield return new[] { "*;q=1.0", "value1" };
                yield return new[] { "*; q=1.0", "value1" };
                yield return new[] { "*; q=0.5", "value1; q=0.5" };
                yield return new[] { "*; q=1.0", "value1; q=1.0" };
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
                        "text",
                        "text;q=1.0",
                        "text",
                        "text;q=0",
                        "*;q=0.8",
                        "*;q=1",
                        "text;q=0.8",
                        "*;q=0.6",
                        "text;q=1.0",
                        "*;q=0.4",
                        "text;q=0.6",
                    }, 
                    new List<string>
                    {
                        "text",
                        "text;q=1.0",
                        "text",
                        "text;q=1.0",
                        "*;q=1",
                        "text;q=0.8",
                        "*;q=0.8",
                        "text;q=0.6",
                        "*;q=0.6",
                        "*;q=0.4",
                        "text;q=0",
                    }, 
                };
            }
        }

        [Fact]
        public void StaticComparerReturnsSameInstance()
        {
            StringWithQualityHeaderValueComparer comparer1 = StringWithQualityHeaderValueComparer.QualityComparer;
            StringWithQualityHeaderValueComparer comparer2 = StringWithQualityHeaderValueComparer.QualityComparer;

            Assert.NotNull(comparer1);
            Assert.Same(comparer1, comparer2);
        }

        [Theory]
        [PropertyData("EqualValues")]
        public void ComparerReturnsZeroForEqualValues(string stringWithQuality1, string stringWithQuality2)
        {
            // Arrange
            StringWithQualityHeaderValueComparer comparer = StringWithQualityHeaderValueComparer.QualityComparer;

            // Act
            StringWithQualityHeaderValue stringWithQualityHeaderValue1 = StringWithQualityHeaderValue.Parse(stringWithQuality1);
            StringWithQualityHeaderValue stringWithQualityHeaderValue2 = StringWithQualityHeaderValue.Parse(stringWithQuality2);

            // Assert
            Assert.Equal(0, comparer.Compare(stringWithQualityHeaderValue1, stringWithQualityHeaderValue2));
            Assert.Equal(0, comparer.Compare(stringWithQualityHeaderValue2, stringWithQualityHeaderValue1));
        }

        [Theory]
        [PropertyData("NonEqualValues")]
        public void ComparerReturnsNonZeroForNonEqualValues(string stringWithQuality1, string stringWithQuality2)
        {
            // Arrange
            StringWithQualityHeaderValueComparer comparer = StringWithQualityHeaderValueComparer.QualityComparer;

            // Act
            StringWithQualityHeaderValue stringWithQualityHeaderValue1 = StringWithQualityHeaderValue.Parse(stringWithQuality1);
            StringWithQualityHeaderValue stringWithQualityHeaderValue2 = StringWithQualityHeaderValue.Parse(stringWithQuality2);

            // Assert
            Assert.Equal(-1, comparer.Compare(stringWithQualityHeaderValue1, stringWithQualityHeaderValue2));
            Assert.Equal(1, comparer.Compare(stringWithQualityHeaderValue2, stringWithQualityHeaderValue1));
        }

        [Theory]
        [PropertyData("BeforeAfterSortedValues")]
        public void ComparerSortsListCorrectly(List<string> unsorted, List<string> expectedSorted)
        {
            // Arrange
            IEnumerable<StringWithQualityHeaderValue> unsortedValues =
                unsorted.Select(u => StringWithQualityHeaderValue.Parse(u));

            IEnumerable<StringWithQualityHeaderValue> expectedSortedValues =
                expectedSorted.Select(u => StringWithQualityHeaderValue.Parse(u));

            // Act
            IEnumerable<StringWithQualityHeaderValue> actualSorted = unsortedValues.OrderByDescending(m => m, StringWithQualityHeaderValueComparer.QualityComparer);

            // Assert
            Assert.True(expectedSortedValues.SequenceEqual(actualSorted));
        }
    }
}
