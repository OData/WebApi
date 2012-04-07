// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Microsoft.TestCommon;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;
using Xunit.Extensions;

namespace System.Net.Http.Headers
{
    class CookieHeaderValueTest
    {
        public static TheoryDataSet<CookieHeaderValue, string> CookieHeaderDataSet
        {
            get
            {
                var dataset = new TheoryDataSet<CookieHeaderValue, string>();
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("n1", "v1");
                nvc.Add("n2", "v2");
                nvc.Add("n3", "v3");
                CookieHeaderValue header1 = new CookieHeaderValue("name1", nvc)
                {
                    Domain = "domain1",
                    Expires = new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero),
                    HttpOnly = true,
                    MaxAge = TimeSpan.FromDays(1),
                    Path = "path1",
                    Secure = true
                };
                dataset.Add(header1, "name1=n1=v1&n2=v2&n3=v3; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1; path=path1; secure; httponly");

                CookieHeaderValue header2 = new CookieHeaderValue("name2", "");
                dataset.Add(header2, "name2=");

                CookieHeaderValue header3 = new CookieHeaderValue("name2", "value2");
                dataset.Add(header3, "name2=value2");

                CookieHeaderValue header4 = new CookieHeaderValue("name4", "value4")
                {
                    MaxAge = TimeSpan.FromDays(1),
                };
                dataset.Add(header4, "name4=value4; max-age=86400");

                CookieHeaderValue header5 = new CookieHeaderValue("name5", "value5")
                {
                    Domain = "domain1",
                    Expires = new DateTimeOffset(1994, 11, 6, 8, 49, 37, TimeSpan.Zero),
                };
                dataset.Add(header5, "name5=value5; expires=Sun, 06 Nov 1994 08:49:37 GMT; domain=domain1");

                return dataset;
            }
        }

        public static TheoryDataSet<string> InvalidCookieHeaderDataSet
        {
            get 
            {
                return new TheoryDataSet<string>
                {
                    "expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=86400; domain=domain1",
                    "name=value; expires=Sun, 06 Nov 1994 08:49:37 ZZZ; max-age=86400; domain=domain1",
                    "name=value; expires=Sun, 06 Nov 1994 08:49:37 GMT; max-age=-86400; domain=domain1",
                };
            }
        }

        [Fact]
        public void CookieHeaderValueCtor1_InitializesCorrectly()
        {
            CookieHeaderValue header = new CookieHeaderValue("cookie", "value");
            Assert.Equal(1, header.Cookies.Count);
            Assert.Equal("cookie", header.Cookies[0].Name);
            Assert.Equal("value", header.Cookies[0].Values.AllKeys[0]);
        }

        [Fact]
        public void CookieHeaderValueCtor2_InitializesCorrectly()
        {
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("name", "value");
            CookieHeaderValue header = new CookieHeaderValue("cookie", nvc);
            Assert.Equal(1, header.Cookies.Count);
            Assert.Equal("cookie", header.Cookies[0].Name);
            Assert.Equal("name", header.Cookies[0].Values.AllKeys[0]);
            Assert.Equal("value", header.Cookies[0].Values["name"]);
        }

        [Fact]
        public void CookieHeaderValueClone()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("name", "value");
            CookieHeaderValue expectedValue = new CookieHeaderValue("cookie", nvc);
            expectedValue.Domain = "domain";
            expectedValue.Expires = DateTimeOffset.Now;
            expectedValue.MaxAge = TimeSpan.FromDays(10);
            expectedValue.Path = "path";
            expectedValue.HttpOnly = true;
            expectedValue.Secure = true;

            // Act
            CookieHeaderValue actualValue = expectedValue.Clone() as CookieHeaderValue;

            // Assert
            Assert.Equal(expectedValue.ToString(), actualValue.ToString());
        }

        [Theory]
        [PropertyData("CookieHeaderDataSet")]
        public void CookieHeaderToString(CookieHeaderValue input, string expectedValue)
        {
            Assert.Equal(expectedValue, input.ToString());
        }

        [Theory]
        [PropertyData("CookieHeaderDataSet")]
        public void CookieHeaderTryParse_AcceptsValidValues(CookieHeaderValue cookie, string expectedValue)
        {
            CookieHeaderValue header;
            bool result = CookieHeaderValue.TryParse(expectedValue, out header);

            Assert.True(result);
            Assert.Equal(expectedValue, header.ToString());
        }

        [Theory]
        [PropertyData("InvalidCookieHeaderDataSet")]
        public void CookieHeaderTryParse_RejectsInvalidValues(string value)
        {
            CookieHeaderValue header;
            bool result = CookieHeaderValue.TryParse(value, out header);

            Assert.False(result);
        }
    }
}
