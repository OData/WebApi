// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class FormattingUtilitiesTests
    {
        public static TheoryDataSet<string, string> QuotedStrings
        {
            get
            {
                return new TheoryDataSet<string, string>()
                {
                    { @"""""", @"" },
                    { @"""string""", @"string" },
                    { @"string", @"string" },
                    { @"""str""ing""", @"str""ing" },
                };
            }
        }

        public static TheoryDataSet<string> NotQuotedStrings
        {
            get
            {
                return new TheoryDataSet<string>
                {       
                    @" """,
                    @" """"",
                    @"string",
                    @"str""ing",
                    @"s""trin""g",
                };
            }
        }

        public static TheoryDataSet<string> ValidHeaderTokens
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    Convert.ToChar(0x21).ToString(),
                    Convert.ToChar(0x7E).ToString(),
                    "acb",
                    "ABC",
                    "a&b",
                };
            }
        }

        public static TheoryDataSet<string> InvalidHeaderTokens
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    Convert.ToChar(0x20).ToString(),
                    Convert.ToChar(0x7F).ToString(),
                    null,
                    "<acb>",
                    "[ABC]",
                    "{a&b}",
                };
            }
        }

        public static IEnumerable<object[]> ValidDateStringValues
        {
            get
            {
                return new TheoryDataSet<DateTimeOffset, string>
                {
                    { new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero), "Sun, 06 Nov 1994 08:49:37 GMT" },
                };
            }
        }

        public static IEnumerable<object[]> ValidDateValues
        {
            get
            {
                return new TheoryDataSet<string, DateTimeOffset>
                {
                    // RFC1123 date/time value
                    { "Sun, 06 Nov 1994 08:49:37 GMT", new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },
                    { "Sun, 06 Nov 1994 08:49:37", new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },
                    { "6 Nov 1994 8:49:37 GMT", new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },
                    { "6 Nov 1994 8:49:37",  new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },
                    { "Sun, 06 Nov 94 08:49:37",  new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },
                    { "6 Nov 94 8:49:37",  new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },

                    // RFC850 date/time value
                    { "Sunday, 06-Nov-94 08:49:37 GMT",  new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },
                    { "Sunday, 6-Nov-94 8:49:37",  new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },

                    // ANSI C's asctime() format
                    { "Sun Nov  06 08:49:37 1994",  new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },
                    { "Sun Nov  6 8:49:37 1994",  new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero) },

                    // RFC5322 date/time
                    { "Sat, 08 Nov 1997 09:55:06 -0600", new DateTimeOffset(1997, 11, 8, 9, 55, 6, new TimeSpan(-6, 0, 0)) },
                    { "8 Nov 1997 9:55:6", new DateTimeOffset(1997, 11, 8, 9, 55, 6, TimeSpan.Zero) },
                    { "Sat, 8 Nov 1997 9:55:6 +0200", new DateTimeOffset(1997, 11, 8, 9, 55, 6, new TimeSpan(2, 0, 0)) },
                };
            }
        }

        public static TheoryDataSet<string> InvalidDateValues
        {
            get
            {
                return new TheoryDataSet<string>
                {
                    "Sun, 06 Nov 1994 08:49:37 GMT invalid",
                    "Sun, 06 Nov 1994 08:49:37 GMT,",
                    ",Sun, 06 Nov 1994 08:49:37 GMT",
                };
            }
        }

        [Fact]
        [Trait("Description", "Utilities is internal static type.")]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(FormattingUtilities), TypeAssert.TypeProperties.IsClass | TypeAssert.TypeProperties.IsStatic);
        }

        [Fact]
        [Trait("Description", "IsJsonValueType returns true")]
        public void IsJsonValueTypeReturnsTrue()
        {
            Assert.True(FormattingUtilities.IsJTokenType(typeof(JToken)), "Should return true");
            Assert.True(FormattingUtilities.IsJTokenType(typeof(JValue)), "Should return true");
            Assert.True(FormattingUtilities.IsJTokenType(typeof(JObject)), "Should return true");
            Assert.True(FormattingUtilities.IsJTokenType(typeof(JArray)), "Should return true");
        }

        [Fact]
        [Trait("Description", "CreateEmptyContentHeaders returns empty headers")]
        public void CreateEmptyContentHeadersReturnsEmptyHeaders()
        {
            HttpContentHeaders headers = FormattingUtilities.CreateEmptyContentHeaders();
            Assert.NotNull(headers);
            Assert.Equal(0, headers.Count());
        }

        [Theory]
        [TestDataSet(typeof(CommonUnitTestDataSets), "EmptyStrings")]
        [Trait("Description", "UnquoteToken returns same string on null, empty strings")]
        public void UnquoteTokenReturnsSameRefOnEmpty(string empty)
        {
            string result = FormattingUtilities.UnquoteToken(empty);
            Assert.Same(empty, result);
        }

        [Theory]
        [PropertyData("NotQuotedStrings")]
        [Trait("Description", "UnquoteToken returns unquoted strings")]
        public void UnquoteTokenReturnsSameRefNonQuotedStrings(string test)
        {
            string result = FormattingUtilities.UnquoteToken(test);
            Assert.Equal(test, result);
        }

        [Theory]
        [PropertyData("QuotedStrings")]
        [Trait("Description", "UnquoteToken returns unquoted strings")]
        public void UnquoteTokenReturnsUnquotedStrings(string token, string expectedResult)
        {
            string result = FormattingUtilities.UnquoteToken(token);
            Assert.Equal(expectedResult, result);
        }

        [Theory]
        [PropertyData("ValidHeaderTokens")]
        public void ValidateHeaderToken_AcceptsValidTokens(string validToken)
        {
            bool result = FormattingUtilities.ValidateHeaderToken(validToken);
            Assert.True(result);
        }

        [Theory]
        [PropertyData("InvalidHeaderTokens")]
        public void ValidateHeaderToken_RejectsInvalidTokens(string invalidToken)
        {
            bool result = FormattingUtilities.ValidateHeaderToken(invalidToken);
            Assert.False(result);
        }

        [Theory]
        [PropertyData("ValidDateStringValues")]
        public void DateToString_GeneratesValidValue(DateTimeOffset input, string expectedValue)
        {
            string actualValue = FormattingUtilities.DateToString(input);
            Assert.Equal(expectedValue, actualValue);
        }

        [Theory]
        [PropertyData("ValidDateValues")]
        public void TryParseDate_AcceptsValidDates(string dateValue, DateTimeOffset expectedDate)
        {
            DateTimeOffset actualDate;
            Assert.True(FormattingUtilities.TryParseDate(dateValue, out actualDate));
            Assert.Equal(expectedDate, actualDate);
        }

        [Theory]
        [PropertyData("InvalidDateValues")]
        public void TryStringToDate_RejectsInvalidDates(string dateValue)
        {
            DateTimeOffset actualDate;
            Assert.False(FormattingUtilities.TryParseDate(dateValue, out actualDate));
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("1", 1)]
        [InlineData("2147483647", Int32.MaxValue)]
        public void TryParseInt32_AcceptsValidNumbers(string intValue, int expectedInt)
        {
            int actualInt;
            Assert.True(FormattingUtilities.TryParseInt32(intValue, out actualInt));
            Assert.Equal(expectedInt, actualInt);
        }

        [Theory]
        [InlineData("-2147483649")]
        [InlineData("-2147483648")]
        [InlineData("2147483648")]
        [InlineData(" 0")]
        public void TryParseInt32_RejectsInvalidNumbers(string intValue)
        {
            int actualInt;
            Assert.False(FormattingUtilities.TryParseInt32(intValue, out actualInt));
        }
    }
}
