// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.TestCommon;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Http.Hosting
{
    public class HttpRouteTest
    {
        [Theory]
        [InlineData("{controller}/{id}", "/SelfHostServer", "http://localhost/SelfHostServer/Customer/999")]
        [InlineData("{controller}/{id}", "", "http://localhost/Customer/999")]
        [InlineData("{controller}", "", "http://localhost/")]
        [InlineData("{controller}", "/SelfHostServer", "http://localhost/SelfHostServer")]
        [InlineData("{controller}", "", "http://localhost")]
        [InlineData("{controller}/{id}", "", "http://localhost/")]
        [InlineData("{controller}/{id}", "/SelfHostServer", "http://localhost/SelfHostServer")]
        [InlineData("{controller}/{id}", "", "http://localhost")]
        public void GetRouteDataShouldMatch(string uriTemplate, string virtualPathRoot, string requestUri)
        {
            HttpRoute route = new HttpRoute(uriTemplate);
            route.Defaults.Add("controller", "Customer");
            route.Defaults.Add("id", "999");
            HttpRequestMessage request = new HttpRequestMessage();
            request.RequestUri = new Uri(requestUri);
            IHttpRouteData data = route.GetRouteData(virtualPathRoot, request);

            // Assert
            Assert.NotNull(data);
            IDictionary<string, object> expectedResult = new Dictionary<string, object>();
            expectedResult["controller"] = "Customer";
            expectedResult["id"] = "999";
            Assert.Equal(expectedResult, data.Values, new DictionaryEqualityComparer());
        }
    }
}
