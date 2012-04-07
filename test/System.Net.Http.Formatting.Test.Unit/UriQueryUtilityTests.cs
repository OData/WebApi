// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http.Formatting;
using System.Net.Http.Internal;
using System.Text;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Net.Http
{
    public class UriQueryUtilityTests
    {
        [Fact]
        public void TypeIsCorrect()
        {
            Assert.Type.HasProperties(typeof(UriQueryUtility), TypeAssert.TypeProperties.IsClass | TypeAssert.TypeProperties.IsStatic);
        }

        #region UrlEncode

        [Fact]
        public void UrlEncodeReturnsNull()
        {
            Assert.Null(UriQueryUtility.UrlEncode(null));
        }

        public void UrlEncodeToBytesThrowsOnInvalidArgs()
        {
            Assert.Null(UriQueryUtility.UrlEncodeToBytes(null, 0, 0));
            Assert.ThrowsArgumentNull(() => UriQueryUtility.UrlEncodeToBytes(null, 0, 2), "bytes");

            Assert.ThrowsArgumentOutOfRange(() => UriQueryUtility.UrlEncodeToBytes(new byte[0], -1, 0), "offset", null);
            Assert.ThrowsArgumentOutOfRange(() => UriQueryUtility.UrlEncodeToBytes(new byte[0], 2, 0), "offset", null);

            Assert.ThrowsArgumentOutOfRange(() => UriQueryUtility.UrlEncodeToBytes(new byte[0], 0, -1), "count", null);
            Assert.ThrowsArgumentOutOfRange(() => UriQueryUtility.UrlEncodeToBytes(new byte[0], 0, 2), "count", null);
        }

        #endregion

        #region UrlDecode

        [Fact]
        public void UrlDecodeReturnsNull()
        {
            Assert.Null(UriQueryUtility.UrlDecode(null));
        }

        [Fact]
        public void UrlDecodeParsesEmptySegmentsCorrectly()
        {
            int iterations = 16;
            List<string> segments = new List<string>();

            for (int index = 1; index < iterations; index++)
            {
                segments.Add("&");
                string query = string.Join("", segments);
                NameValueCollection result = ParseQueryString(query);
                Assert.NotNull(result);

                // Because this is a NameValueCollection, the same name appears only once
                Assert.Equal(1, result.Count);

                // Values should be a comma separated list of empty strings
                string[] values = result[""].Split(new char[] { ',' });

                // We expect length+1 segment as the final '&' counts as a segment 
                Assert.Equal(index + 1, values.Length);
                foreach (var value in values)
                {
                    Assert.Equal("", value);
                }
            }
        }

        public static TheoryDataSet<string, string, string> UriQueryData
        {
            get
            {
                return UriQueryTestData.UriQueryData;
            }
        }

        private static string CreateQuery(params string[] segments)
        {
            StringBuilder buffer = new StringBuilder();
            bool first = true;
            foreach (string segment in segments)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    buffer.Append('&');
                }

                buffer.Append(segment);
            }

            return buffer.ToString();
        }

        [Theory]
        [InlineData("N", "N", "")]
        [InlineData("%26", "&", "")]
        [PropertyData("UriQueryData")]
        public void UrlDecodeParsesCorrectly(string segment, string resultName, string resultValue)
        {
            int iterations = 16;
            List<string> segments = new List<string>();

            for (int index = 1; index < iterations; index++)
            {
                segments.Add(segment);
                string query = CreateQuery(segments.ToArray());
                NameValueCollection result = ParseQueryString(query);
                Assert.NotNull(result);

                // Because this is a NameValueCollection, the same name appears only once
                Assert.Equal(1, result.Count);

                // Values should be a comma separated list of resultValue
                string[] values = result[resultName].Split(new char[] { ',' });
                Assert.Equal(index, values.Length);
                foreach (var value in values)
                {
                    Assert.Equal(resultValue, value);
                }
            }
        }

        public void UrlDecodeToBytesThrowsOnInvalidArgs()
        {
            Assert.Null(UriQueryUtility.UrlDecodeToBytes(null, 0, 0));
            Assert.ThrowsArgumentNull(() => UriQueryUtility.UrlDecodeToBytes(null, 0, 2), "bytes");

            Assert.ThrowsArgumentOutOfRange(() => UriQueryUtility.UrlDecodeToBytes(new byte[0], -1, 0), "offset", null);
            Assert.ThrowsArgumentOutOfRange(() => UriQueryUtility.UrlDecodeToBytes(new byte[0], 2, 0), "offset", null);

            Assert.ThrowsArgumentOutOfRange(() => UriQueryUtility.UrlDecodeToBytes(new byte[0], 0, -1), "count", null);
            Assert.ThrowsArgumentOutOfRange(() => UriQueryUtility.UrlDecodeToBytes(new byte[0], 0, 2), "count", null);
        }

        #endregion


        private static NameValueCollection ParseQueryString(string query)
        {
            return new FormDataCollection(query).ReadAsNameValueCollection();
        }
    }
}