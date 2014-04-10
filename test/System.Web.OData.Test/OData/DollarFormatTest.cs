// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;

namespace System.Web.OData
{
    public class DollarFormatTest
    {
        [Theory]
        [InlineData("FormatCustomers/?$format=application/json;odata.metadata=full")]
        [InlineData("This/?$format=application/json;odata.metadata=full")]
        public async Task DollarFormat_Applies_IfPresent(string path)
        {
            // Arrange
            string url = "http://localhost/" + path;

            IEdmModel model = GetEdmModel();
            HttpConfiguration configuration = new HttpConfiguration();
            HttpServer server = new HttpServer(configuration);
            HttpClient client = new HttpClient(server);
            configuration.Routes.MapODataServiceRoute("odata", routePrefix: null, model: model);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal("application/json", response.Content.Headers.ContentType.MediaType);
            Assert.Equal(2, response.Content.Headers.ContentType.Parameters.Count);
        }

        private IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("FormatCustomers");
            builder.Singleton<Customer>("This"); // Singleton
            return builder.GetEdmModel();
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

        public class ThisController : ODataController
        {
            public Customer Get()
            {
                return new Customer();
            }
        }
    }
}
