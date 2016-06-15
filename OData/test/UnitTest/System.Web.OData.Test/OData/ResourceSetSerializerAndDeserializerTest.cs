// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
    public class ResourceSetSerializerAndDeserializerTest
    {
        [Fact]
        public void Get_ResourceSetType()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/Customers";
            var configuration = new[] {typeof (CustomersController)}.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Console.WriteLine(result);
        }

        [Fact]
        public void Get_ResourceSetType2()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/Customers(2)/Locations";
            var configuration = new[] { typeof(CustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Console.WriteLine(result);
        }

        [Fact]
        public void Get_OpenEntityType2222222222222222222()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/Customers(1)/Address";
            var configuration = new[] {typeof (CustomersController)}.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Console.WriteLine(result);
        }

        [Theory]
        [InlineData("http://localhost/odata/SimpleOpenCustomers?$orderby=Token&$filter=Token ne null", new[] {2, 4})]
        [InlineData("http://localhost/odata/SimpleOpenCustomers?$orderby=Token desc&$filter=Token ne null", new[] {4, 2}
            )]
        [InlineData("http://localhost/odata/SimpleOpenCustomers?$filter=Token ne null", new[] {2, 4})]
        public void Get_OpenEntityTypeWithOrderbyAndFilter(string uri, int[] customerIds)
        {
            // Arrange
            var configuration = new[] {typeof (SimpleOpenCustomersController)}.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var resultArray = result["value"] as JArray;
            Assert.Equal(2, resultArray.Count);
            for (var i = 0; i < customerIds.Length; i++)
                Assert.Equal(customerIds[i], resultArray[i]["CustomerId"]);
        }

        [Fact]
        public void Get_OpenEntityTypeWithSelect()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers?$select=Token";
            var configuration = new[] {typeof (SimpleOpenCustomersController)}.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            var resultArray = result["value"] as JArray;
            Assert.Equal(6, resultArray.Count);
            Assert.NotNull(resultArray[2]["Token"]); //customer 2 has a token
            Assert.NotNull(resultArray[4]["Token"]); //customer 4 has a token
        }

        [Fact]
        public void Post_OpenEntityType()
        {
            // Arrange
            const string Payload = "{" +
              "\"@odata.context\":\"http://localhost/odata/$metadata#Customers/$entity\"," +
              "\"Id\":6,\"Name\":\"Sam\"," +
              "\"Location\":{" +
                "\"Street\":\"Street 6\",\"City\":\"City 6\"" +
              "}," +
              "\"Locations\": [" +
              "  {" +
              "    \"@odata.type\": \"#System.Web.OData.Address\"," +
              "    \"City\": \"City 2 L1\"," +
              "    \"Street\": \"Street 2 L1\"" +
              "  }," +
              "  {" +
              "    \"@odata.type\": \"#System.Web.OData.Address\"," +
              "    \"City\": \"City 2 L2\"," +
              "    \"Street\": \"Street 2 L2\"" +
              "  }" +
              "]" +
            "}";

            const string RequestUri = "http://localhost/odata/Customers";

            var configuration = new[] { typeof(CustomersController) }.GetHttpConfiguration();
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

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            //builder.EntitySet<Customer>("SimpleOpenOrders");
            return builder.GetEdmModel();
        }

        public class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Address Location { get; set; }
            public IList<Address> Locations { get; set; }
        }

        public class Address
        {
            public string City { get; set; }
            public string Street { get; set; }
        }

        // Controller
        public class CustomersController : ODataController
        {
            [EnableQuery]
            public IQueryable<Customer> Get()
            {
                return CreateCustomers().AsQueryable();
            }

            public IHttpActionResult Get(int key)
            {
                IList<Customer> customers = CreateCustomers();
                Customer customer = customers.FirstOrDefault(c => c.Id == key);
                if (customer == null)
                {
                    return NotFound();
                }

                return Ok(customer);
            }

            public IHttpActionResult GetLocation(int key)
            {
                IList<Customer> customers = CreateCustomers();
                Customer customer = customers.FirstOrDefault(c => c.Id == key);
                if (customer == null)
                {
                    return NotFound();
                }

                return Ok(customer.Location);
            }

            public IHttpActionResult GetLocations(int key)
            {
                IList<Customer> customers = CreateCustomers();
                Customer customer = customers.FirstOrDefault(c => c.Id == key);
                if (customer == null)
                {
                    return NotFound();
                }

                return Ok(customer.Locations);
            }

            public IHttpActionResult PostCustomer([FromBody]Customer customer)
            {
                Assert.Equal("Sam", customer.Name);
                return Ok();
            }

            private static IList<Customer> CreateCustomers()
            {
                IList<Customer> customers = Enumerable.Range(0, 5).Select(i =>
                    new Customer
                    {
                        Id = i,
                        Name = "Name " + i,
                        Location = new Address
                        {
                            Street = "Street " + i,
                            City = "City " + i
                        },
                        Locations = new[]
                        {
                            new Address
                            {
                                Street = "Street " + i + " L1",
                                City = "City " + i + " L1",
                            },
                            new Address
                            {
                                Street = "Street " + i + " L2",
                                City = "City " + i + " L2",
                            }
                        }
                    }).ToList();
                return customers;
            }
        }
    }
}
