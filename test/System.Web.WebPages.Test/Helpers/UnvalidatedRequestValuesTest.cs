// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.WebPages.Test.Helpers
{
    public class UnvalidatedRequestValuesTest
    {
        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            // Arrange
            NameValueCollection expectedForm = new NameValueCollection();
            NameValueCollection expectedQueryString = new NameValueCollection();

            Mock<System.Web.UnvalidatedRequestValuesBase> mockUnvalidatedRequestValue = new Mock<System.Web.UnvalidatedRequestValuesBase>();
            mockUnvalidatedRequestValue.SetupGet(u => u.Form).Returns(expectedForm);
            mockUnvalidatedRequestValue.SetupGet(u => u.QueryString).Returns(expectedQueryString);

            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.SetupGet(r => r.Unvalidated).Returns(mockUnvalidatedRequestValue.Object);

            // Act
            System.Web.Helpers.UnvalidatedRequestValues unvalidatedValues = new System.Web.Helpers.UnvalidatedRequestValues(mockRequest.Object);

            // Assert
            Assert.Same(expectedForm, unvalidatedValues.Form);
            Assert.Same(expectedQueryString, unvalidatedValues.QueryString);
        }

        [Fact]
        public void Indexer_LooksUpValuesInCorrectOrder()
        {
            // Order should be QueryString, Form, Cookies, ServerVariables

            // Arrange
            NameValueCollection queryString = new NameValueCollection()
            {
                { "foo", "fooQueryString" }
            };

            NameValueCollection form = new NameValueCollection()
            {
                { "foo", "fooForm" },
                { "bar", "barForm" },
            };

            HttpCookieCollection cookies = new HttpCookieCollection()
            {
                new HttpCookie("foo", "fooCookie"),
                new HttpCookie("bar", "barCookie"),
                new HttpCookie("baz", "bazCookie")
            };

            NameValueCollection serverVars = new NameValueCollection()
            {
                { "foo", "fooServerVars" },
                { "bar", "barServerVars" },
                { "baz", "bazServerVars" },
                { "quux", "quuxServerVars" },
            };

            Mock<System.Web.UnvalidatedRequestValuesBase> mockUnvalidatedRequestValue = new Mock<System.Web.UnvalidatedRequestValuesBase>();
            mockUnvalidatedRequestValue.SetupGet(u => u.Form).Returns(form);
            mockUnvalidatedRequestValue.SetupGet(u => u.QueryString).Returns(queryString);

            Mock<HttpRequestBase> mockRequest = new Mock<HttpRequestBase>();
            mockRequest.Setup(o => o.Cookies).Returns(cookies);
            mockRequest.Setup(o => o.ServerVariables).Returns(serverVars);
            mockRequest.SetupGet(r => r.Unvalidated).Returns(mockUnvalidatedRequestValue.Object);

            System.Web.Helpers.UnvalidatedRequestValues unvalidatedValues = new System.Web.Helpers.UnvalidatedRequestValues(mockRequest.Object);

            // Act
            string fooValue = unvalidatedValues["foo"];
            string barValue = unvalidatedValues["bar"];
            string bazValue = unvalidatedValues["baz"];
            string quuxValue = unvalidatedValues["quux"];
            string notFoundValue = unvalidatedValues["not-found"];

            // Assert
            Assert.Equal("fooQueryString", fooValue);
            Assert.Equal("barForm", barValue);
            Assert.Equal("bazCookie", bazValue);
            Assert.Equal("quuxServerVars", quuxValue);
            Assert.Null(notFoundValue);
        }
    }
}