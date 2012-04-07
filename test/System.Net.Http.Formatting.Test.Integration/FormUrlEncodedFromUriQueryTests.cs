// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Json;
using System.Net.Http.Internal;
using System.Text;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Extensions;

namespace System.Net.Http.Formatting
{
    public class FormUrlEncodedJsonFromUriQueryTests
    {
        #region Tests

        [Theory,
            InlineData("abc", "{\"abc\":\"\"}"),
            InlineData("%2eabc%2e", "{\".abc.\":\"\"}"),
            InlineData("", "{}"),
            InlineData("a=1", "{\"a\":\"1\"}")]
        public void SimpleStringsTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("a=2", "{\"a\":\"2\"}"),
            InlineData("b=true", "{\"b\":\"true\"}"),
            InlineData("c=hello", "{\"c\":\"hello\"}"),
            InlineData("d=", "{\"d\":\"\"}"),
            InlineData("e=null", "{\"e\":\"null\"}")]
        public void SimpleObjectsTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Fact]
        public void LegacyArraysTest()
        {
            ValidateFormUrlEncoded("a=1&a=hello&a=333", "{\"a\":[\"1\",\"hello\",\"333\"]}");

            // Only valid in shallow serialization
            ParseInvalidFormUrlEncoded("a[z]=2&a[z]=3");
        }

        [Theory,
            InlineData("a[]=1&a[]=hello&a[]=333", "{\"a\":[\"1\",\"hello\",\"333\"]}"),
            InlineData("a[b][]=1&a[b][]=hello&a[b][]=333", "{\"a\":{\"b\":[\"1\",\"hello\",\"333\"]}}"),
            InlineData("a[]=", "{\"a\":[\"\"]}"),
            InlineData("a%5B%5D=2", @"{""a"":[""2""]}"),
            InlineData("a[x][0]=1&a[x][]=2", @"{""a"":{""x"":[""1"",""2""]}}")]
        public void ArraysTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("a[0][]=1&a[0][]=hello&a[1][]=333", "{\"a\":[[\"1\",\"hello\"],[\"333\"]]}"),
            InlineData("a[b][0][]=1&a[b][1][]=hello&a[b][1][]=333", "{\"a\":{\"b\":[[\"1\"],[\"hello\",\"333\"]]}}"),
            InlineData("a[0][0][0][]=1", "{\"a\":[[[[\"1\"]]]]}")]
        public void MultidimensionalArraysTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("a[0][]=hello&a[2][]=333", "{\"a\":{\"0\":[\"hello\"],\"2\":[\"333\"]}}"),
            InlineData("a[0]=hello", "{\"a\":[\"hello\"]}"),
            InlineData("a[1][]=hello", "{\"a\":{\"1\":[\"hello\"]}}"),
            InlineData("a[1][0]=hello", "{\"a\":{\"1\":[\"hello\"]}}")]
        public void SparseArraysTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("b[]=2&b[1][c]=d", "{\"b\":[\"2\",{\"c\":\"d\"}]}")]
        public void ArraysWithMixedMembers(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("=3", "{\"\":\"3\"}"),
            InlineData("a=1&=3", "{\"a\":\"1\",\"\":\"3\"}"),
            InlineData("=3&b=2", "{\"\":\"3\",\"b\":\"2\"}")]
        public void EmptyKeyTest(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        [Theory,
            InlineData("a[b]=1&a=2"),
            InlineData("a[b]=1&a[b][]=2"),
            InlineData("a[x][]=1&a[x][0]=2"),
            InlineData("a=2&a[b]=1"),
            InlineData("[]=1"),
            InlineData("a[][]=0"),
            InlineData("a[][x]=0"),
            InlineData("a&a[b]=1")]
        public void InvalidObjectGraphsTest(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        [Theory,
            InlineData("a[b=2"),
            InlineData("a[[b]=2"),
            InlineData("a[b]]=2")]
        public void InvalidFormUrlEncodingTest(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data originated from JS primitives.
        /// </summary>
        [Theory,
            InlineData("abc", @"{""abc"":""""}"),
            InlineData("123", @"{""123"":""""}"),
            InlineData("true", @"{""true"":""""}"),
            InlineData("", "{}"),
            InlineData("%2fabc%2f", @"{""/abc/"":""""}")]
        public void TestJValue(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Negative tests for parsing form-urlencoded data originated from JS primitives.
        /// </summary>
        [Theory,
            InlineData("a[b]=1&a=2"),
            InlineData("a=2&a[b]=1"),
            InlineData("[]=1")]
        public void TestJValueNegative(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data originated from JS objects.
        /// </summary>
        [Theory,
            InlineData("a=NaN", @"{""a"":""NaN""}"),
            InlineData("a=false", @"{""a"":""false""}"),
            InlineData("a=foo", @"{""a"":""foo""}"),
            InlineData("1=1", "{\"1\":\"1\"}")]
        public void TestObjects(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data originated from JS arrays.
        /// </summary>
        [Theory,
            InlineData("a[]=2", @"{""a"":[""2""]}"),
            InlineData("a[]=", @"{""a"":[""""]}"),
            InlineData("a[0][0][]=1", @"{""a"":[[[""1""]]]}"),
            InlineData("z[]=9&z[]=true&z[]=undefined&z[]=", @"{""z"":[""9"",""true"",""undefined"",""""]}"),
            InlineData("z[]=9&z[]=true&z[]=undefined&z[]=null", @"{""z"":[""9"",""true"",""undefined"",""null""]}"),
            InlineData("z[0][]=9&z[0][]=true&z[1][]=undefined&z[1][]=null", @"{""z"":[[""9"",""true""],[""undefined"",""null""]]}"),
            InlineData("a[0][x]=2", @"{""a"":[{""x"":""2""}]}"),
            InlineData("a%5B%5D=2", @"{""a"":[""2""]}"),
            InlineData("a%5B%5D=", @"{""a"":[""""]}"),
            InlineData("z%5B%5D=9&z%5B%5D=true&z%5B%5D=undefined&z%5B%5D=", @"{""z"":[""9"",""true"",""undefined"",""""]}"),
            InlineData("z%5B%5D=9&z%5B%5D=true&z%5B%5D=undefined&z%5B%5D=null", @"{""z"":[""9"",""true"",""undefined"",""null""]}"),
            InlineData("z%5B0%5D%5B%5D=9&z%5B0%5D%5B%5D=true&z%5B1%5D%5B%5D=undefined&z%5B1%5D%5B%5D=null", @"{""z"":[[""9"",""true""],[""undefined"",""null""]]}")]
        public void TestArray(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data originated from JS arrays, using the jQuery 1.3 format (no []'s).
        /// </summary>
        [Theory,
            InlineData("z=9&z=true&z=undefined&z=", @"{""z"":[""9"",""true"",""undefined"",""""]}"),
            InlineData("z=9&z=true&z=undefined&z=null", @"{""z"":[""9"",""true"",""undefined"",""null""]}"),
            InlineData("z=9&z=true&z=undefined&z=null&a=hello", @"{""z"":[""9"",""true"",""undefined"",""null""],""a"":""hello""}")]
        public void TestArrayCompat(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Negative tests for parsing form-urlencoded data originated from JS arrays.
        /// </summary>
        [Theory,
            InlineData("a[z]=2&a[z]=3")]
        public void TestArrayCompatNegative(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        /// <summary>
        /// Tests for form-urlencoded data originated from sparse JS arrays.
        /// </summary>
        [Theory,
            InlineData("a[2]=hello", @"{""a"":{""2"":""hello""}}"),
            InlineData("a[x][0]=2", @"{""a"":{""x"":[""2""]}}"),
            InlineData("a[x][1]=2", @"{""a"":{""x"":{""1"":""2""}}}"),
            InlineData("a[x][0]=0&a[x][1]=1", @"{""a"":{""x"":[""0"",""1""]}}"),
            InlineData("a[0][0][0]=hello&a[1][0][0][0][]=hello", @"{""a"":[[[""hello""]],[[[[""hello""]]]]]}"),
            InlineData("a[0][0][0]=hello&a[1][0][0][0]=hello", @"{""a"":[[[""hello""]],[[[""hello""]]]]}"),
            InlineData("a[1][0][]=1", @"{""a"":{""1"":[[""1""]]}}"),
            InlineData("a[1][1][]=1", @"{""a"":{""1"":{""1"":[""1""]}}}"),
            InlineData("a[1][1][0]=1", @"{""a"":{""1"":{""1"":[""1""]}}}"),
            InlineData("a[0][]=2&a[0][]=3&a[2][]=1", "{\"a\":{\"0\":[\"2\",\"3\"],\"2\":[\"1\"]}}"),
            InlineData("a[x][]=1&a[x][1]=2", @"{""a"":{""x"":[""1"",""2""]}}"),
            InlineData("a[x][0]=1&a[x][]=2", @"{""a"":{""x"":[""1"",""2""]}}")]
        public void TestArraySparse(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Negative tests for parsing form-urlencoded arrays.
        /// </summary>
        [Theory,
            InlineData("a[x]=2&a[x][]=3"),
            InlineData("a[]=1&a[0][]=2"),
            InlineData("a[]=1&a[0][0][]=2"),
            InlineData("a[x][]=1&a[x][0]=2"),
            InlineData("a[][]=0"),
            InlineData("a[][x]=0")]
        public void TestArrayIndexNegative(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }


        public static IEnumerable<object[]> TestObjectPropertyData
        {
            get
            {
                string encoded = "a[]=4&a[]=5&b[x][]=7&b[y]=8&b[z][]=9&b[z][]=true&b[z][]=undefined&b[z][]=&c=1&f=";
                string resultStr = @"{""a"":[""4"",""5""],""b"":{""x"":[""7""],""y"":""8"",""z"":[""9"",""true"",""undefined"",""""]},""c"":""1"",""f"":""""}";
                yield return new[] { encoded, resultStr };

                encoded = "customer[Name]=Pete&customer[Address]=Redmond&customer[Age][0][]=23&customer[Age][0][]=24&customer[Age][1][]=25&" +
                    "customer[Age][1][]=26&customer[Phones][]=425+888+1111&customer[Phones][]=425+345+7777&customer[Phones][]=425+888+4564&" +
                    "customer[EnrolmentDate]=%22%5C%2FDate(1276562539537)%5C%2F%22&role=NewRole&changeDate=3&count=15";
                resultStr = @"{""customer"":{""Name"":""Pete"",""Address"":""Redmond"",""Age"":[[""23"",""24""],[""25"",""26""]]," +
                    @"""Phones"":[""425 888 1111"",""425 345 7777"",""425 888 4564""],""EnrolmentDate"":""\""\\/Date(1276562539537)\\/\""""},""role"":""NewRole"",""changeDate"":""3"",""count"":""15""}";
                yield return new[] { encoded, resultStr };

                encoded = "customers[0][Name]=Pete2&customers[0][Address]=Redmond2&customers[0][Age][0][]=23&customers[0][Age][0][]=24&" +
                    "customers[0][Age][1][]=25&customers[0][Age][1][]=26&customers[0][Phones][]=425+888+1111&customers[0][Phones][]=425+345+7777&" +
                    "customers[0][Phones][]=425+888+4564&customers[0][EnrolmentDate]=%22%5C%2FDate(1276634840700)%5C%2F%22&customers[1][Name]=Pete3&" +
                    "customers[1][Address]=Redmond3&customers[1][Age][0][]=23&customers[1][Age][0][]=24&customers[1][Age][1][]=25&customers[1][Age][1][]=26&" +
                    "customers[1][Phones][]=425+888+1111&customers[1][Phones][]=425+345+7777&customers[1][Phones][]=425+888+4564&customers[1][EnrolmentDate]=%22%5C%2FDate(1276634840700)%5C%2F%22";
                resultStr = @"{""customers"":[{""Name"":""Pete2"",""Address"":""Redmond2"",""Age"":[[""23"",""24""],[""25"",""26""]]," +
                    @"""Phones"":[""425 888 1111"",""425 345 7777"",""425 888 4564""],""EnrolmentDate"":""\""\\/Date(1276634840700)\\/\""""}," +
                    @"{""Name"":""Pete3"",""Address"":""Redmond3"",""Age"":[[""23"",""24""],[""25"",""26""]],""Phones"":[""425 888 1111"",""425 345 7777"",""425 888 4564""],""EnrolmentDate"":""\""\\/Date(1276634840700)\\/\""""}]}";
                yield return new[] { encoded, resultStr };

                encoded = "ab%5B%5D=hello";
                resultStr = @"{""ab"":[""hello""]}";
                yield return new[] { encoded, resultStr };

                encoded = "123=hello";
                resultStr = @"{""123"":""hello""}";
                yield return new[] { encoded, resultStr };

                encoded = "a%5B%5D=1&a";
                resultStr = @"{""a"":[""1"",""""]}";
                yield return new[] { encoded, resultStr };

                encoded = "a=1&a";
                resultStr = @"{""a"":[""1"",""""]}";
                yield return new[] { encoded, resultStr };
            }
        }

        /// <summary>
        /// Tests for parsing complex object graphs form-urlencoded.
        /// </summary>
        [Theory]
        [PropertyData("TestObjectPropertyData")]
        public void TestObject(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        public static IEnumerable<object[]> TestEncodedNameTestData
        {
            get
            {
                string encoded = "some+thing=10";
                string resultStr = @"{""some thing"":""10""}";
                yield return new[] { encoded, resultStr };

                encoded = "%E5%B8%A6%E4%B8%89%E4%B8%AA%E8%A1%A8=bar";
                resultStr = @"{""带三个表"":""bar""}";
                yield return new[] { encoded, resultStr };

                encoded = "some+thing=10&%E5%B8%A6%E4%B8%89%E4%B8%AA%E8%A1%A8=bar";
                resultStr = @"{""some thing"":""10"",""带三个表"":""bar""}";
                yield return new[] { encoded, resultStr };

                encoded = "a[0\r\n][b]=1";
                resultStr = "{\"a\":{\"0\\r\\n\":{\"b\":\"1\"}}}";
                yield return new[] { encoded, resultStr };
                yield return new[] { encoded.Replace("\r", "%0D").Replace("\n", "%0A"), resultStr };

                yield return new[] { "a[0\0]=1", "{\"a\":{\"0\\u0000\":\"1\"}}" };
                yield return new[] { "a[0%00]=1", "{\"a\":{\"0\\u0000\":\"1\"}}" };
                yield return new[] { "a[\00]=1", "{\"a\":{\"\\u00000\":\"1\"}}" };
                yield return new[] { "a[%000]=1", "{\"a\":{\"\\u00000\":\"1\"}}" };
            }
        }

        /// <summary>
        /// Tests for parsing form-urlencoded data with encoded names.
        /// </summary>
        [Theory]
        [PropertyData("TestEncodedNameTestData")]
        public void TestEncodedName(string encoded, string expectedResult)
        {
            ValidateFormUrlEncoded(encoded, expectedResult);
        }

        /// <summary>
        /// Tests for malformed form-urlencoded data.
        /// </summary>
        [Theory,
            InlineData("a[b=2"),
            InlineData("a[[b]=2"),
            InlineData("a[b]]=2")]
        public void TestNegative(string encoded)
        {
            ParseInvalidFormUrlEncoded(encoded);
        }

        /// <summary>
        /// Tests for parsing generated form-urlencoded data.
        /// </summary>
        [Fact]
        public void GeneratedJTokenTest()
        {
            Random rndGen = new Random(1);
            int oldMaxArray = CreatorSettings.MaxArrayLength;
            int oldMaxList = CreatorSettings.MaxListLength;
            int oldMaxStr = CreatorSettings.MaxStringLength;
            double oldNullProbability = CreatorSettings.NullValueProbability;
            bool oldCreateAscii = CreatorSettings.CreateOnlyAsciiChars;
            CreatorSettings.MaxArrayLength = 5;
            CreatorSettings.MaxListLength = 3;
            CreatorSettings.MaxStringLength = 3;
            CreatorSettings.NullValueProbability = 0;
            CreatorSettings.CreateOnlyAsciiChars = true;
            JTokenCreatorSurrogate jsonValueCreator = new JTokenCreatorSurrogate();
            try
            {
                for (int i = 0; i < 1000; i++)
                {
                    JToken jv = (JToken)jsonValueCreator.CreateInstanceOf(typeof(JToken), rndGen);
                    if (jv.Type == JTokenType.Array || jv.Type == JTokenType.Object)
                    {
                        string jaStr = FormUrlEncoding(jv);
                        JToken deserJv = ParseFormUrlEncoded(jaStr);
                        bool compare = true;
                        if (deserJv is JObject && ((IDictionary<string, JToken>)deserJv).ContainsKey("JV"))
                        {
                            compare = JTokenRoundTripComparer.Compare(jv, deserJv["JV"]);
                        }
                        else
                        {
                            compare = JTokenRoundTripComparer.Compare(jv, deserJv);
                        }

                        Assert.True(compare, "Comparison failed for test instance " + i);
                    }
                }
            }
            finally
            {
                CreatorSettings.MaxArrayLength = oldMaxArray;
                CreatorSettings.MaxListLength = oldMaxList;
                CreatorSettings.MaxStringLength = oldMaxStr;
                CreatorSettings.NullValueProbability = oldNullProbability;
                CreatorSettings.CreateOnlyAsciiChars = oldCreateAscii;
            }
        }

        #endregion

        #region Helpers
        private static string FormUrlEncoding(JToken jsonValue)
        {
            List<string> results = new List<string>();
            if (jsonValue is JValue)
            {
                return UriQueryUtility.UrlEncode(((JValue)jsonValue).Value.ToString());
            }

            BuildParams("JV", jsonValue, results);
            StringBuilder strResult = new StringBuilder();
            foreach (var result in results)
            {
                strResult.Append("&" + result);
            }

            if (strResult.Length > 0)
            {
                return strResult.Remove(0, 1).ToString();
            }

            return strResult.ToString();
        }

        private static void BuildParams(string prefix, JToken jsonValue, List<string> results)
        {
            if (jsonValue is JValue)
            {
                JValue jsonPrimitive = jsonValue as JValue;
                if (jsonPrimitive != null)
                {
                    if (jsonPrimitive.Type == JTokenType.String && String.IsNullOrEmpty(jsonPrimitive.Value.ToString()))
                    {
                        results.Add(prefix + "=" + String.Empty);
                    }
                    else
                    {
                        if (jsonPrimitive.Value is DateTime || jsonPrimitive.Value is DateTimeOffset)
                        {
                            string dateStr = jsonPrimitive.ToString();
                            if (!String.IsNullOrEmpty(dateStr) && dateStr.StartsWith("\""))
                            {
                                dateStr = dateStr.Substring(1, dateStr.Length - 2);
                            }
                            results.Add(prefix + "=" + UriQueryUtility.UrlEncode(dateStr));
                        }
                        else
                        {
                            results.Add(prefix + "=" + UriQueryUtility.UrlEncode(jsonPrimitive.Value.ToString()));
                        }
                    }
                }
                else
                {
                    results.Add(prefix + "=" + String.Empty);
                }
            }
            else if (jsonValue is JArray)
            {
                for (int i = 0; i < ((JArray)jsonValue).Count; i++)
                {
                    if (jsonValue[i] is JArray || jsonValue[i] is JObject)
                    {
                        BuildParams(prefix + "[" + i + "]", jsonValue[i], results);
                    }
                    else
                    {
                        BuildParams(prefix + "[]", jsonValue[i], results);
                    }
                }
            }
            else //jsonValue is JObject
            {
                foreach (KeyValuePair<string, JToken> item in (JObject)jsonValue)
                {
                    BuildParams(prefix + "[" + item.Key + "]", item.Value, results);
                }
            }
        }

        private static Uri GetQueryUri(string query)
        {
            UriBuilder uriBuilder = new UriBuilder("http://some.host");
            uriBuilder.Query = query;
            return uriBuilder.Uri;
        }

        private static JToken ParseFormUrlEncoded(string encoded)
        {
            Uri address = GetQueryUri(encoded);
            JObject result;
            Assert.True(address.TryReadQueryAsJson(out result), "Expected parsing to return true");
            return result;
        }

        private static void ParseInvalidFormUrlEncoded(string encoded)
        {
            Uri address = GetQueryUri(encoded);
            JObject result;
            Assert.False(address.TryReadQueryAsJson(out result), "Expected parsing to return false");
            Assert.Null(result);
        }

        private static void ValidateFormUrlEncoded(string encoded, string expectedResult)
        {
            Uri address = GetQueryUri(encoded);
            JObject result;
            Assert.True(address.TryReadQueryAsJson(out result), "Expected parsing to return true");
            Assert.NotNull(result);
            Assert.Equal(expectedResult, result.ToString(Newtonsoft.Json.Formatting.None));
        }

        #endregion
    }
}