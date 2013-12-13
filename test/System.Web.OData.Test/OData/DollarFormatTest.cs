// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData
{
    public class DollarFormatTest
    {
        [Fact]
        public void DollarFormat_Applies_IfPresent()
        {
            // Arrange
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("FormatCustomers");
            IEdmModel model = builder.GetEdmModel();
            HttpConfiguration configuration = new HttpConfiguration();
            HttpServer server = new HttpServer(configuration);
            HttpClient client = new HttpClient(server);
            configuration.Routes.MapODataRoute("odata", routePrefix: null, model: model);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(
                HttpMethod.Get,
                "http://localhost/FormatCustomers/?$format=atom");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/atom+xml", response.Content.Headers.ContentType.MediaType);
        }

        public class Customer
        {
            public int ID { get; set; }

            public string Name { get; set; }
        }

        public class FormatCustomersController : ODataController
        {
            public Customer[] Get()
            {
                return new Customer[] { };
            }
        }
    }
}
