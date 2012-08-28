// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Builder.Conventions;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ODataFormatterTests
    {
        const string baseAddress = "http://localhost:8081/";

        private ODataMediaTypeFormatter _serverFormatter = new ODataMediaTypeFormatter(ODataTestUtil.GetEdmModel());

        private static readonly MediaTypeWithQualityHeaderValue _atomMediaType = ODataTestUtil.ApplicationAtomMediaTypeWithQuality;
        private static readonly MediaTypeWithQualityHeaderValue _jsonMediaType = ODataTestUtil.ApplicationJsonMediaTypeWithQuality;

        HttpConfiguration _config;
        HttpClient _client;

        public ODataFormatterTests()
        {
            _config = new HttpConfiguration();
            _config.Routes.MapHttpRoute(ODataRouteNames.GetById, "{controller}({id})");
            _config.Routes.MapHttpRoute(ODataRouteNames.Default, "{controller}");
        }

        [Fact]
        [Trait("Description", "Demonstrates how to get the response from an Http GET in OData atom format when the accept header is application/atom+xml")]
        public void Get_Entry_In_OData_Atom_Format()
        {
            _config.Formatters.Insert(0, _serverFormatter);

            using (HttpServer host = new HttpServer(_config))
            {
                _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, new Uri(baseAddress + "People(10)"));
                requestMessage.Headers.Accept.Add(_atomMediaType);
                requestMessage.Headers.Add("DataServiceVersion", "2.0");
                requestMessage.Headers.Add("MaxDataServiceVersion", "3.0");
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(response.Content.Headers.ContentType.MediaType, _atomMediaType.MediaType);
                    Assert.Equal(ODataTestUtil.GetDataServiceVersion(response.Content.Headers), ODataTestUtil.Version3NumberString);

                    ODataTestUtil.VerifyResponse(response.Content, BaselineResource.EntryTypePersonAtom);
                }
            }
        }

        [Fact]
        [Trait("Description", "Demonstrates how to get the response from an Http GET in OData atom format when the accept header is application/json")]
        public void Get_Entry_In_OData_Json_Format()
        {
            _config.Formatters.Insert(0, _serverFormatter);

            using (HttpServer host = new HttpServer(_config))
            {
                _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, new Uri(baseAddress + "People(10)"));
                requestMessage.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));
                requestMessage.Headers.Add("DataServiceVersion", "2.0");
                requestMessage.Headers.Add("MaxDataServiceVersion", "3.0");

                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(response.Content.Headers.ContentType.MediaType, _jsonMediaType.MediaType);
                    Assert.Equal(ODataTestUtil.GetDataServiceVersion(response.Content.Headers), ODataTestUtil.Version3NumberString);

                    // this request should be handled by OData Json
                    ODataTestUtil.VerifyJsonResponse(response.Content, BaselineResource.EntryTypePersonODataJson);
                }
            }
        }

        [Fact]
        [Trait("Description", "Demonstrates how to get the ODataMediaTypeFormatter to only support application/atom+xml")]
        public void Support_Only_OData_Atom_Format()
        {
            ODataMediaTypeFormatter odataFormatter = _serverFormatter;
            odataFormatter.SupportedMediaTypes.Remove(ODataTestUtil.ApplicationJsonMediaType);
            _config.Formatters.Insert(0, odataFormatter);

            using (HttpServer host = new HttpServer(_config))
            {
                _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, new Uri(baseAddress + "People(10)"));
                requestMessage.Headers.Accept.Add(_atomMediaType);
                requestMessage.Headers.Add("DataServiceVersion", "2.0");
                requestMessage.Headers.Add("MaxDataServiceVersion", "3.0");

                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(response.Content.Headers.ContentType.MediaType, _atomMediaType.MediaType);
                    Assert.Equal(ODataTestUtil.GetDataServiceVersion(response.Content.Headers), ODataTestUtil.Version3NumberString);

                    ODataTestUtil.VerifyResponse(response.Content, BaselineResource.EntryTypePersonAtom);
                }

                HttpRequestMessage messageWithJsonHeader = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, new Uri(baseAddress + "People(10)"));
                messageWithJsonHeader.Headers.Accept.Add(ODataTestUtil.ApplicationJsonMediaTypeWithQuality);
                messageWithJsonHeader.Headers.Add("DataServiceVersion", "2.0");
                messageWithJsonHeader.Headers.Add("MaxDataServiceVersion", "3.0");

                using (HttpResponseMessage response = _client.SendAsync(messageWithJsonHeader).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(response.Content.Headers.ContentType.MediaType, ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType);
                    Assert.Null(ODataTestUtil.GetDataServiceVersion(response.Content.Headers));

                    ODataTestUtil.VerifyJsonResponse(response.Content, BaselineResource.EntryTypePersonRegularJson);
                }
            }
        }

        [Fact]
        [Trait("Description", "Demonstrates how ODataMediaTypeFormatter would conditionally support application/atom+xml and application/json only if format=odata is present in the QueryString")]
        public void Conditionally_Support_OData_If_Query_String_Present()
        {
            ODataMediaTypeFormatter odataFormatter = _serverFormatter;
            odataFormatter.SupportedMediaTypes.Clear();
            odataFormatter.MediaTypeMappings.Add(new ODataMediaTypeMapping(ODataTestUtil.ApplicationAtomMediaTypeWithQuality));
            odataFormatter.MediaTypeMappings.Add(new ODataMediaTypeMapping(ODataTestUtil.ApplicationJsonMediaTypeWithQuality));
            _config.Formatters.Insert(0, odataFormatter);

            using (HttpServer host = new HttpServer(_config))
            {
                _client = new HttpClient(host);
                // this request should return response in OData atom format
                HttpRequestMessage requestMessage = ODataTestUtil.GenerateRequestMessage(new Uri(baseAddress + "People(10)?format=odata"), isAtom: true);
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(ODataTestUtil.ApplicationAtomMediaTypeWithQuality.MediaType, response.Content.Headers.ContentType.MediaType);
                    Assert.Equal(ODataTestUtil.Version3NumberString, ODataTestUtil.GetDataServiceVersion(response.Content.Headers));
                    ODataTestUtil.VerifyResponse(response.Content, BaselineResource.EntryTypePersonAtom);
                }

                // this request should return response in OData json format
                HttpRequestMessage messageWithJsonHeader = ODataTestUtil.GenerateRequestMessage(new Uri(baseAddress + "People(10)?format=odata"), isAtom: false);
                using (HttpResponseMessage response = _client.SendAsync(messageWithJsonHeader).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(ODataTestUtil.ApplicationJsonMediaType.MediaType, response.Content.Headers.ContentType.MediaType);
                    Assert.Equal(ODataTestUtil.Version3NumberString, ODataTestUtil.GetDataServiceVersion(response.Content.Headers));

                    // this request should be handled by OData Json
                    ODataTestUtil.VerifyJsonResponse(response.Content, BaselineResource.EntryTypePersonODataJson);
                }

                // when the query string is not present, request should be handled by the regular Json Formatter
                messageWithJsonHeader = ODataTestUtil.GenerateRequestMessage(new Uri(baseAddress + "People(10)"), isAtom: false);

                using (HttpResponseMessage response = _client.SendAsync(messageWithJsonHeader).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                    Assert.Equal(response.Content.Headers.ContentType.MediaType, ODataTestUtil.ApplicationJsonMediaTypeWithQuality.MediaType);
                    Assert.Null(ODataTestUtil.GetDataServiceVersion(response.Content.Headers));

                    ODataTestUtil.VerifyJsonResponse(response.Content, BaselineResource.EntryTypePersonRegularJson);
                }
            }
        }
    }
}
