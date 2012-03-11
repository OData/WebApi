using System.Json;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.TestCommon;
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
            Assert.True(FormattingUtilities.IsJsonValueType(typeof(JsonValue)), "Should return true");
            Assert.True(FormattingUtilities.IsJsonValueType(typeof(JsonPrimitive)), "Should return true");
            Assert.True(FormattingUtilities.IsJsonValueType(typeof(JsonObject)), "Should return true");
            Assert.True(FormattingUtilities.IsJsonValueType(typeof(JsonArray)), "Should return true");
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
    }
}
