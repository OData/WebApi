// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData
{
    public class OpenEntityTypeTest
    {
        [Fact]
        public void Get_OpenEntityType()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers(9)";
            var configuration = new[] { typeof(SimpleOpenCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("http://localhost/odata/$metadata#SimpleOpenCustomers/$entity", result["@odata.context"]);
            Assert.Equal("#System.Web.OData.SimpleVipCustomer", result["@odata.type"]);
            Assert.Equal(9, result["CustomerId"]);
            Assert.Equal("VipCustomer", result["Name"]);
            Assert.Equal("#Collection(Int32)", result["ListProp@odata.type"]);
            Assert.Equal(new JArray(new[] { 200, 100, 300, 0, 400 }), result["ListProp"]);
        }

        [Fact]
        public void Post_OpenEntityType()
        {
            // Arrange
            const string Payload = "{" +
              "\"@odata.context\":\"http://localhost/odata/$metadata#OpenCustomers/$entity\"," +
              "\"CustomerId\":6,\"Name\":\"FirstName 6\"," +
              "\"Address\":{" +
                "\"Street\":\"Street 6\",\"City\":\"City 6\",\"Country\":\"Earth\",\"Token@odata.type\":\"#Guid\"," +
                "\"Token\":\"4DB52263-4382-4BCB-A63E-3129C1B5FA0D\"," +
                "\"Number\":990" +
              "}," +
              "\"Website\": \"WebSite #6\",\"Country\":\"My Dynamic Country\",\"Token@odata.type\":\"#Guid\",\"Token\":\"2c1f450a-a2a7-4fe1-a25d-4d9332fc0694\"," +
              "\"DoubleList@odata.type\":\"#Collection(Double)\"," +
              "\"DoubleList\":[5.5, 4.4, 3.3]" +
            "}";

            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers";

            var configuration = new[] { typeof(SimpleOpenCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public void Patch_OpenEntityType()
        {
            // Arrange
            const string Payload = "{" +
              "\"CustomerId\":99,\"Name\":\"ChangedName\"," +
              "\"Token@odata.type\":\"#DateTimeOffset\",\"Token\":\"2014-01-01T00:00:00Z\"," +
              "\"DoubleList@odata.type\":\"#Collection(Double)\"," +
              "\"DoubleList\":[5.5, 4.4, 3.3]" +
              "}";

            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers(2)";

            var configuration = new[] { typeof(SimpleOpenCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(99, result["CustomerId"]);
            Assert.Equal("ChangedName", result["Name"]);

            // The type and the value of "Token" are changed.
            Assert.Equal("#DateTimeOffset", result["Token@odata.type"]);

            // The type and the value of "IntList" are un-changed.
            Assert.Equal(new JArray(new[] { 1, 2, 3, 4, 5, 6, 7 }), result["IntList"]);

            // New dynamic property "DoubleList" is added.
            Assert.Equal(new JArray(new[] { 5.5, 4.4, 3.3 }), result["DoubleList"]);
        }

        [Fact]
        public void Put_OpenEntityType()
        {
            // Arrange
            const string Payload = "{" +
              "\"CustomerId\":99,\"Name\":\"ChangedName\"," +
              "\"Token@odata.type\":\"#DateTimeOffset\",\"Token\":\"2014-01-01T00:00:00Z\"" +
              "}";

            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers(2)";

            var configuration = new[] { typeof(SimpleOpenCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Put"), RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<SimpleOpenCustomer>("SimpleOpenCustomers");
            builder.EntitySet<SimpleOpenOrder>("SimpleOpenOrders");
            return builder.GetEdmModel();
        }
    }

    // Controller
    public class SimpleOpenCustomersController : ODataController
    {
        private static IList<SimpleOpenCustomer> CreateCustomers()
        {
            int[] IntValues = { 200, 100, 300, 0, 400 };
            IList<SimpleOpenCustomer> customers = Enumerable.Range(0, 5).Select(i =>
                new SimpleOpenCustomer
                {
                    CustomerId = i,
                    Name = "FirstName " + i,
                    Address = new SimpleOpenAddress
                    {
                        Street = "Street " + i,
                        City = "City " + i,
                        Properties = new Dictionary<string, object> { { "IntProp", IntValues[i] } }
                    },
                    Website = "WebSite #" + i
                }).ToList();

            customers[2].CustomerProperties = new Dictionary<string, object>
            {
                {"Token", new Guid("2C1F450A-A2A7-4FE1-A25D-4D9332FC0694")},
                {"IntList", new List<int> { 1, 2, 3, 4, 5, 6, 7 }},
            };

            SimpleOpenAddress address = new SimpleOpenAddress
            {
                Street = "SubStreet",
                City = "City"
            };
            customers[3].CustomerProperties = new Dictionary<string, object> {{"ComplexList", new[] {address, address}}};

            SimpleVipCustomer vipCustomer = new SimpleVipCustomer
            {
                CustomerId = 9,
                Name = "VipCustomer",
                Address = new SimpleOpenAddress
                {
                    Street = "Vip Street ",
                    City = "Vip City ",
                },
                VipNum = "99-001",
                CustomerProperties = new Dictionary<string, object> { { "ListProp", IntValues } }
            };

            customers.Add(vipCustomer);
            return customers;
        }

        public IHttpActionResult Get(int key)
        {
            IList<SimpleOpenCustomer> customers = CreateCustomers();
            SimpleOpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            if (customer.CustomerProperties == null)
            {
                customer.CustomerProperties = new Dictionary<string, object>();
                customer.CustomerProperties.Add("Token", new Guid("2C1F450A-A2A7-4FE1-A25D-4D9332FC0694"));
                IList<int> lists = new List<int> {1, 2, 3, 4, 5, 6, 7};
                customer.CustomerProperties.Add("MyList", lists);
            }

            return Ok(customer);
        }

        public IHttpActionResult PostSimpleOpenCustomer([FromBody]SimpleOpenCustomer customer)
        {
            // Verify there is a string dynamic property
            object countryValue;
            customer.CustomerProperties.TryGetValue("Country", out countryValue);
            Assert.NotNull(countryValue);
            Assert.Equal(typeof(String), countryValue.GetType());
            Assert.Equal("My Dynamic Country", countryValue);

            // Verify there is a Guid dynamic property
            object tokenValue;
            customer.CustomerProperties.TryGetValue("Token", out tokenValue);
            Assert.NotNull(tokenValue);
            Assert.Equal(typeof(Guid), tokenValue.GetType());
            Assert.Equal(new Guid("2c1f450a-a2a7-4fe1-a25d-4d9332fc0694"), tokenValue);

            // Verify there is an dynamic collection property
            object value;
            customer.CustomerProperties.TryGetValue("DoubleList", out value);
            Assert.NotNull(value);
            List<double> doubleValues = Assert.IsType<List<double>>(value);
            Assert.Equal(new[] { 5.5, 4.4, 3.3 }, doubleValues);
            return Ok();
        }

        public IHttpActionResult Patch(int key, Delta<SimpleOpenCustomer> patch)
        {
            IList<SimpleOpenCustomer> customers = CreateCustomers();
            SimpleOpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            patch.Patch(customer);

            // As normal, return Updated(customer) for *PATCH*.
            // But, Here returns the *Ok* to test the payload
            return Ok(customer);
        }

        public IHttpActionResult Put(int key, SimpleOpenCustomer changedCustomer)
        {
            Assert.Equal(99, changedCustomer.CustomerId);
            Assert.Equal("ChangedName", changedCustomer.Name);
            Assert.Null(changedCustomer.Address);
            Assert.Null(changedCustomer.Website);
            Assert.Equal(1, changedCustomer.CustomerProperties.Count);
            return Updated(changedCustomer); // Updated(customer);
        }
    }
}
