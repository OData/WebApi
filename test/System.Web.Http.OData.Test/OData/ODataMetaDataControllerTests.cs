// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Builder.Conventions;
using System.Web.Http.OData.Formatter;
using Xunit;

namespace System.Web.Http.OData.Builder
{
    public class ODataMetaDataControllerTests
    {
        public void DollarMetaData_Works_WithoutAcceptHeader()
        {
            HttpServer server = new HttpServer();
            ODataMediaTypeFormatter odataFormatter = new ODataMediaTypeFormatter(ODataTestUtil.GetEdmModel());
            server.Configuration.Formatters.Add(odataFormatter);
            server.Configuration.Routes.MapHttpRoute(ODataRouteNames.Metadata, "$metadata", new { Controller = "ODataMetadata", Action = "GetMetadata" });

            HttpClient client = new HttpClient(server);
            var response = client.GetAsync("http://localhost/$metadata").Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(response.Content.Headers.ContentType.MediaType, "application/xml");
            Assert.Contains("<edmx:Edmx", response.Content.ReadAsStringAsync().Result);
        }
    }
}
