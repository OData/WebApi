// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;
using Microsoft.TestCommon;

namespace System.Net.Http.Headers
{
    class CookieStateTest
    {
        [Fact]
        public void CookieState_CtorThrowsOnNullName()
        {
            Assert.ThrowsArgumentNull(() => new CookieState(null, "value"), "name");
        }

        [Theory]
        [InlineData("<acb>")]
        [InlineData("{acb}")]
        [InlineData("[acb]")]
        public void CookieState_CtorThrowsOnInvalidName(string name)
        {
            Assert.ThrowsArgument(() => new CookieState(name, "value"), "name");
        }

        [Theory]
        [InlineData("\"acb\"")]
        [InlineData("a,b")]
        [InlineData("a;b")]
        [InlineData("a\\b")]
        public void CookieState_CtorThrowsOnInvalidValue(string value)
        {
            Assert.ThrowsArgument(() => new CookieState("name", value), "value");
        }

        [Fact]
        public void CookieState_CtorThrowsOnNullNameValueCollection()
        {
            Assert.ThrowsArgumentNull(() => new CookieState("name", (NameValueCollection)null), "values");
        }

        [Theory]
        [InlineData("name", "")]
        [InlineData("name", "value")]
        public void CookieState_Ctor1InitializesCorrectly(string name, string value)
        {
            CookieState cookie = new CookieState(name, value);
            Assert.Equal(name, cookie.Name);
            Assert.Equal(value, cookie.Values.AllKeys[0]);
            Assert.Equal(value, cookie.Value);
        }

        [Fact]
        public void CookieState_Ctor2InitializesCorrectly()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("n1", "v1");

            // Act
            CookieState cookie = new CookieState("name", nvc);

            // Assert
            Assert.Equal("name", cookie.Name);
            Assert.Equal(1, cookie.Values.Count);
            Assert.Equal("n1", cookie.Values.AllKeys[0]);
            Assert.Equal("v1", cookie.Values["n1"]);
            Assert.Equal("n1=v1", cookie.Value);
        }

        [Fact]
        public void CookieState_Value()
        {
            CookieState cookie = new CookieState("name");
            Assert.Equal(String.Empty, cookie.Value);

            cookie.Value = "value1";
            Assert.Equal("value1", cookie.Value);

            cookie.Values.AllKeys[0] = "value2";
            Assert.Equal("value2", cookie.Value);
        }

        [Fact]
        public void CookieState_Clone()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("n1", "v1");
            CookieState expectedValue = new CookieState("name", nvc);

            // Act
            CookieState actualValue = expectedValue.Clone() as CookieState;

            // Assert
            Assert.Equal("name", actualValue.Name);
            Assert.Equal(1, actualValue.Values.Count);
            Assert.Equal("n1", actualValue.Values.AllKeys[0]);
            Assert.Equal("v1", actualValue.Values["n1"]);
        }

        [Fact]
        public void CookieState_ToStringWithSingleValue()
        {
            // Arrange
            CookieState cookie = new CookieState("name", "value");

            // Act
            string actualValue = cookie.ToString();

            // Assert
            Assert.Equal("name=value", actualValue);
        }

        [Fact]
        public void CookieState_ToStringWithNameValueCollection()
        {
            // Arrange
            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("n1", "v1");
            nvc.Add("n2", "v2");
            nvc.Add("n3", "v3");
            CookieState cookie = new CookieState("name", nvc);

            // Act
            string actualValue = cookie.ToString();

            // Assert
            Assert.Equal("name=n1=v1&n2=v2&n3=v3", actualValue);
        }
    }
}
