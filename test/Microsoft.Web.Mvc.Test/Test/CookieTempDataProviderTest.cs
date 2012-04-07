// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace Microsoft.Web.Mvc.Test
{
    public class CookieTempDataProviderTest
    {
        [Fact]
        public void ConstructProviderThrowsOnNullHttpContext()
        {
            Assert.ThrowsArgumentNull(
                delegate { new CookieTempDataProvider(null); },
                "httpContext");
        }

        [Fact]
        public void CtorSetsHttpContextProperty()
        {
            var httpContext = new Mock<HttpContextBase>().Object;
            var provider = new CookieTempDataProvider(httpContext);

            Assert.Equal(httpContext, provider.HttpContext);
        }

        [Fact]
        public void LoadTempDataWithEmptyCookieReturnsEmptyDictionary()
        {
            HttpCookie cookie = new HttpCookie("__ControllerTempData");
            cookie.Value = String.Empty;
            var cookies = new HttpCookieCollection();
            cookies.Add(cookie);

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);

            IDictionary<string, object> tempData = provider.LoadTempData(null /* controllerContext */);
            Assert.NotNull(tempData);
            Assert.Equal(0, tempData.Count);
        }

        [Fact]
        public void LoadTempDataWithNullCookieReturnsEmptyTempDataDictionary()
        {
            var cookies = new HttpCookieCollection();

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);

            IDictionary<string, object> tempData = provider.LoadTempData(null /* controllerContext */);
            Assert.NotNull(tempData);
            Assert.Equal(0, tempData.Count);
        }

        [Fact]
        public void LoadTempDataIgnoresNullResponseCookieDoesNotThrowException()
        {
            HttpCookie cookie = new HttpCookie("__ControllerTempData");
            var initialTempData = new Dictionary<string, object>();
            initialTempData.Add("WhatIsInHere?", "Stuff");
            cookie.Value = CookieTempDataProvider.DictionaryToBase64String(initialTempData);
            var cookies = new HttpCookieCollection();
            cookies.Add(cookie);

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(r => r.Cookies).Returns((HttpCookieCollection)null);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);

            IDictionary<string, object> tempData = provider.LoadTempData(null /* controllerContext */);
            Assert.Equal("Stuff", tempData["WhatIsInHere?"]);
        }

        [Fact]
        public void LoadTempDataWithNullResponseDoesNotThrowException()
        {
            HttpCookie cookie = new HttpCookie("__ControllerTempData");
            var initialTempData = new Dictionary<string, object>();
            initialTempData.Add("WhatIsInHere?", "Stuff");
            cookie.Value = CookieTempDataProvider.DictionaryToBase64String(initialTempData);
            var cookies = new HttpCookieCollection();
            cookies.Add(cookie);

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns((HttpResponseBase)null);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);

            IDictionary<string, object> tempData = provider.LoadTempData(null /* controllerContext */);
            Assert.Equal("Stuff", tempData["WhatIsInHere?"]);
        }

        [Fact]
        public void SaveTempDataStoresSerializedFormInCookie()
        {
            var cookies = new HttpCookieCollection();
            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);
            var tempData = new Dictionary<string, object>();
            tempData.Add("Testing", "Turn it up to 11");
            tempData.Add("Testing2", 1.23);

            provider.SaveTempData(null, tempData);
            HttpCookie cookie = cookies["__ControllerTempData"];
            string serialized = cookie.Value;
            IDictionary<string, object> deserializedTempData = CookieTempDataProvider.Base64StringToDictionary(serialized);
            Assert.Equal("Turn it up to 11", deserializedTempData["Testing"]);
            Assert.Equal(1.23, deserializedTempData["Testing2"]);
        }

        [Fact]
        public void CanLoadTempDataFromCookie()
        {
            var tempData = new Dictionary<string, object>();
            tempData.Add("abc", "easy as 123");
            tempData.Add("price", 1.234);
            string serializedTempData = CookieTempDataProvider.DictionaryToBase64String(tempData);

            var cookies = new HttpCookieCollection();
            var httpCookie = new HttpCookie("__ControllerTempData");
            httpCookie.Value = serializedTempData;
            cookies.Add(httpCookie);

            var requestMock = new Mock<HttpRequestBase>();
            requestMock.Setup(r => r.Cookies).Returns(cookies);

            var responseMock = new Mock<HttpResponseBase>();
            responseMock.Setup(r => r.Cookies).Returns(cookies);

            var httpContextMock = new Mock<HttpContextBase>();
            httpContextMock.Setup(c => c.Request).Returns(requestMock.Object);
            httpContextMock.Setup(c => c.Response).Returns(responseMock.Object);

            ITempDataProvider provider = new CookieTempDataProvider(httpContextMock.Object);
            IDictionary<string, object> loadedTempData = provider.LoadTempData(null /* controllerContext */);
            Assert.Equal(2, loadedTempData.Count);
            Assert.Equal("easy as 123", loadedTempData["abc"]);
            Assert.Equal(1.234, loadedTempData["price"]);
        }
    }
}
