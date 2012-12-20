// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    [PartialTrustRunner]
    public class PartialTrustTest : MarshalByRefObject
    {
        const string baseAddress = "http://localhost:8081/";

        [Fact]
        public void PostEntry_InODataAtomFormat()
        {
            var _config = new HttpConfiguration();
            _config.EnableOData(ODataTestUtil.GetEdmModel());

            using (HttpServer host = new HttpServer(_config))
            {
                var _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(baseAddress + "People"));
                requestMessage.Content = new StringContent(BaselineResource.PersonEntryInAtom, Encoding.UTF8, "application/atom+xml");
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                    Assert.Equal("application/atom+xml", response.Content.Headers.ContentType.MediaType);

                    ODataTestUtil.VerifyResponse(response.Content, BaselineResource.PersonEntryInAtom);
                }
            }
        }

        [Fact]
        public void PostEntry_InODataJsonLightFormat()
        {
            var _config = new HttpConfiguration();
            _config.EnableOData(ODataTestUtil.GetEdmModel());

            using (HttpServer host = new HttpServer(_config))
            {
                var _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, new Uri(baseAddress + "People"));
                requestMessage.Content = new StringContent(BaselineResource.PersonRequestEntryInPlainOldJson);
                requestMessage.Content.Headers.ContentType = ODataMediaTypes.ApplicationJsonODataFullMetadata;
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

                    ODataTestUtil.VerifyJsonResponse(response.Content, BaselineResource.PersonEntryInJsonFullMetadata);
                }
            }
        }

        [Fact]
        public void PostEntry_InODataJsonVerboseFormat()
        {
            var _config = new HttpConfiguration();
            _config.EnableOData(ODataTestUtil.GetEdmModel());

            using (HttpServer host = new HttpServer(_config))
            {
                var _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, new Uri(baseAddress + "People"));
                requestMessage.Content = new StringContent(BaselineResource.PersonRequestEntryInPlainOldJson);
                requestMessage.Content.Headers.ContentType = ODataTestUtil.ApplicationJsonMediaType;
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

                    ODataTestUtil.VerifyJsonResponse(response.Content, BaselineResource.PersonEntryInJsonVerbose);
                }
            }
        }
    }
}
