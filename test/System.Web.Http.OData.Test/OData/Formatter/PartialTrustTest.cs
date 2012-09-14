// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    [PartialTrustRunner]
    public class PartialTrustTest : MarshalByRefObject
    {
        const string baseAddress = "http://localhost:8081/";

        [Fact]
        public void GetEntry_InODataAtomFormat()
        {
            var _config = new HttpConfiguration();
            _config.Routes.MapHttpRoute(ODataRouteNames.GetById, "{controller}({id})");
            _config.Routes.MapHttpRoute(ODataRouteNames.Default, "{controller}");
            _config.Formatters.Insert(0, new ODataMediaTypeFormatter(ODataTestUtil.GetEdmModel()));

            using (HttpServer host = new HttpServer(_config))
            {
                var _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(baseAddress + "People(10)"));
                requestMessage.Headers.Accept.Add(ODataTestUtil.ApplicationAtomMediaTypeWithQuality);
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(ODataTestUtil.ApplicationAtomMediaType.MediaType, response.Content.Headers.ContentType.MediaType);

                    ODataTestUtil.VerifyResponse(response.Content, BaselineResource.EntryTypePersonAtom);
                }
            }
        }

        [Fact]
        public void GetEntry_InODataJsonFormat()
        {
            var _config = new HttpConfiguration();
            _config.Routes.MapHttpRoute(ODataRouteNames.GetById, "{controller}({id})");
            _config.Routes.MapHttpRoute(ODataRouteNames.Default, "{controller}");
            _config.Formatters.Insert(0, new ODataMediaTypeFormatter(ODataTestUtil.GetEdmModel()));

            using (HttpServer host = new HttpServer(_config))
            {
                var _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, new Uri(baseAddress + "People(10)"));
                requestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(ODataTestUtil.ApplicationJsonMediaType.MediaType, response.Content.Headers.ContentType.MediaType);

                    ODataTestUtil.VerifyJsonResponse(response.Content, BaselineResource.EntryTypePersonODataJson);
                }
            }
        }
    }
}
