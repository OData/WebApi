// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Builder.Conventions;
using System.Xml.Linq;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ODataFormatterTests
    {
        const string baseAddress = "http://localhost:8081/";

        private IEnumerable<ODataMediaTypeFormatter> _serverFormatters;

        private static readonly MediaTypeWithQualityHeaderValue _atomMediaType = ODataTestUtil.ApplicationAtomMediaTypeWithQuality;
        private static readonly MediaTypeWithQualityHeaderValue _jsonMediaType = ODataTestUtil.ApplicationJsonMediaTypeWithQuality;

        HttpConfiguration _config;
        HttpClient _client;

        public ODataFormatterTests()
        {
            _config = new HttpConfiguration();
            _config.EnableOData(ODataTestUtil.GetEdmModel());
            _serverFormatters = _config.Formatters.OfType<ODataMediaTypeFormatter>();
        }

        [Fact]
        [Trait("Description", "Demonstrates how to get the response from an Http GET in OData atom format when the accept header is application/atom+xml")]
        public void Get_Entry_In_OData_Atom_Format()
        {
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
            foreach (ODataMediaTypeFormatter odataFormatter in _serverFormatters)
            {
                odataFormatter.SupportedMediaTypes.Remove(ODataTestUtil.ApplicationJsonMediaType);
            }

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
            foreach (ODataMediaTypeFormatter odataFormatter in _serverFormatters)
            {
                odataFormatter.SupportedMediaTypes.Clear();
                odataFormatter.MediaTypeMappings.Add(new ODataMediaTypeMapping(ODataTestUtil.ApplicationAtomMediaTypeWithQuality));
                odataFormatter.MediaTypeMappings.Add(new ODataMediaTypeMapping(ODataTestUtil.ApplicationJsonMediaTypeWithQuality));
            }

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

        [Fact]
        public void GetFeedInODataAtomFormat_HasSelfLink()
        {
            using (HttpServer host = new HttpServer(_config))
            {
                _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(baseAddress + "People"));
                requestMessage.Headers.Accept.Add(_atomMediaType);
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    XElement xml = XElement.Load(response.Content.ReadAsStreamAsync().Result);
                    XElement[] links = xml.Elements(XName.Get("link", "http://www.w3.org/2005/Atom")).ToArray();
                    Assert.Equal("self", links.First().Attribute("rel").Value);
                    Assert.Equal(baseAddress + "People", links.First().Attribute("href").Value);
                }
            }
        }

        [Fact]
        public void GetFeedInODataAtomFormat_LimitsResults()
        {
            using (HttpServer host = new HttpServer(_config))
            {
                _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(baseAddress + "People?$orderby=Name"));
                requestMessage.Headers.Accept.Add(_atomMediaType);
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                    XElement xml = XElement.Load(response.Content.ReadAsStreamAsync().Result);
                    XElement[] entries = xml.Elements(XName.Get("entry", "http://www.w3.org/2005/Atom")).ToArray();
                    XElement nextPageLink = xml.Elements(XName.Get("link", "http://www.w3.org/2005/Atom"))
                        .Where(link => link.Attribute(XName.Get("rel")).Value == "next")
                        .SingleOrDefault();

                    // Assert the ResultLimit correctly limits three results to two
                    Assert.Equal(2, entries.Length);
                    // Assert there is a next page link
                    Assert.NotNull(nextPageLink);
                }
            }
        }

        [Fact]
        public void HttpErrorInODataFormat_GetsSerializedCorrectly()
        {
            _config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            using (HttpServer host = new HttpServer(_config))
            {
                _client = new HttpClient(host);
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(baseAddress + "People?$filter=abc+eq+null"));
                requestMessage.Headers.Accept.Add(_atomMediaType);
                using (HttpResponseMessage response = _client.SendAsync(requestMessage).Result)
                {
                    Assert.NotNull(response);
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

                    XElement xml = XElement.Load(response.Content.ReadAsStreamAsync().Result);

                    Assert.Equal("error", xml.Name.LocalName);
                    Assert.Equal("The query specified in the URI is not valid.", xml.Element(XName.Get("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}message")).Value);
                    XElement innerErrorXml = xml.Element(XName.Get("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}innererror"));
                    Assert.NotNull(innerErrorXml);
                    Assert.Equal("Type 'System.Web.Http.OData.Formatter.FormatterPerson' does not have a property 'abc'.", innerErrorXml.Element(XName.Get("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}message")).Value);
                    Assert.Equal("Microsoft.Data.OData.ODataException", innerErrorXml.Element(XName.Get("{http://schemas.microsoft.com/ado/2007/08/dataservices/metadata}type")).Value);
                }
            }
        }
    }
}
