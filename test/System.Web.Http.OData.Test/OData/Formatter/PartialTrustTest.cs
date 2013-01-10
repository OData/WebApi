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
            var config = new HttpConfiguration();
            config.Routes.MapODataRoute(ODataTestUtil.GetEdmModel());

            using (HttpServer host = new HttpServer(config))
            {
                var client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri(baseAddress + "People"));
                requestMessage.Content = new StringContent(Resources.PersonEntryInAtom, Encoding.UTF8, "application/atom+xml");
                using (HttpResponseMessage response = client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                    Assert.Equal("application/atom+xml", response.Content.Headers.ContentType.MediaType);

                    ODataTestUtil.VerifyResponse(response.Content, Resources.PersonEntryInAtom);
                }
            }
        }

        [Fact]
        public void PostEntry_InODataJsonLightFormat()
        {
            var config = new HttpConfiguration();
            config.Routes.MapODataRoute(ODataTestUtil.GetEdmModel());

            using (HttpServer host = new HttpServer(config))
            {
                var client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, new Uri(baseAddress + "People"));
                requestMessage.Content = new StringContent(Resources.PersonRequestEntryInPlainOldJson);
                requestMessage.Content.Headers.ContentType = ODataMediaTypes.ApplicationJsonODataFullMetadata;
                using (HttpResponseMessage response = client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

                    ODataTestUtil.VerifyJsonResponse(response.Content, Resources.PersonEntryInJsonFullMetadata);
                }
            }
        }

        [Fact]
        public void PostEntry_InODataJsonVerboseFormat()
        {
            var config = new HttpConfiguration();
            config.Routes.MapODataRoute(ODataTestUtil.GetEdmModel());

            using (HttpServer host = new HttpServer(config))
            {
                var client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Post, new Uri(baseAddress + "People"));
                requestMessage.Content = new StringContent(Resources.PersonRequestEntryInPlainOldJson);
                requestMessage.Content.Headers.ContentType = ODataTestUtil.ApplicationJsonMediaType;
                using (HttpResponseMessage response = client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
                    Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);

                    ODataTestUtil.VerifyJsonResponse(response.Content, Resources.PersonEntryInJsonVerbose);
                }
            }
        }
    }
}
