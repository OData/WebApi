// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.TestCommon;
using Microsoft.OData.Edm;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData.Formatter
{
    public class ODataActionTests
    {
        HttpServer _server;
        HttpClient _client;
        IEdmModel _model;

        public ODataActionTests()
        {
            HttpConfiguration configuration = new HttpConfiguration();
            _model = GetModel();
            configuration.Formatters.Clear();
            configuration.Formatters.AddRange(ODataMediaTypeFormatters.Create());
            configuration.MapODataServiceRoute(_model);
            var controllers = new[] { typeof(CustomersController) };
            var assembliesResolver = new TestAssemblyResolver(new MockAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), assembliesResolver);

            _server = new HttpServer(configuration);
            _client = new HttpClient(_server);
        }

        [Fact]
        public void CanDispatch_ActionPayload_ToBoundAction()
        {
            // Arrange
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/Customers(1)/org.odata.DoSomething");
            request.Headers.Add("accept", "application/json");
            string payload = @"{ 
                ""p1"": 1, 
                ""p2"": { ""StreetAddress"": ""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 }, 
                ""p3"": [ ""one"", ""two"" ],  
                ""p4"": [ { ""StreetAddress"": ""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } ]
            }";

            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains("\"@odata.context\":\"http://localhost/$metadata#Edm.Boolean\",\"value\":true", responseString);
        }

        [Fact]
        public void Response_Includes_ActionLink_WithAcceptHeader()
        {
            // Arrange
            string editLink = "http://localhost/Customers(1)";
            string expectedTarget = editLink + "/org.odata.DoSomething";
            string expectedMetadata = "#org.odata.DoSomething";

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, editLink);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));

            // Act
            HttpResponseMessage response = _client.SendAsync(request).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;
            dynamic result = JObject.Parse(responseString);
            dynamic doSomething = result[expectedMetadata];

            // Assert
            Assert.NotNull(doSomething);
            Assert.Equal(expectedTarget, (string)doSomething.target);
            Assert.Equal("DoSomething", (string)doSomething.title);
        }

        [Fact]
        public void Response_Includes_ActionLink_WithDollarFormat()
        {
            // Arrange
            string requestUri = "http://localhost/Customers?$format=application/json;odata.metadata=full";

            // Act
            HttpResponseMessage response = _client.GetAsync(requestUri).Result;
            string responseString = response.Content.ReadAsStringAsync().Result;

            // Assert
            Assert.Contains("\"target\":\"http://localhost/Customers(4)/org.odata.DoSomething\"", responseString);
        }

        private IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.ContainerName = "Container";
            builder.Namespace = "org.odata";
            EntityTypeConfiguration<Customer> customer = builder.EntitySet<Customer>("Customers").EntityType;
            ActionConfiguration action = customer.Action("DoSomething");
            action.Parameter<int>("p1");
            action.Parameter<Address>("p2");
            action.CollectionParameter<string>("p3");
            action.CollectionParameter<Address>("p4");
            return builder.GetEdmModel();
        }

        public class Customer
        {
            public int ID { get; set; }
            public string Name { get; set; }
        }

        public class Address
        {
            public string StreetAddress { get; set; }
            public string City { get; set; }
            public string State { get; set; }
            public int ZipCode { get; set; }
        }
    }

    public class CustomersController : ODataController
    {
        [HttpGet]
        public IHttpActionResult Get()
        {
            var customers = Enumerable.Range(1, 6).Select(i => new ODataActionTests.Customer
                    {
                        ID = i,
                        Name = "Name " + i
                    }).ToList();

            return Ok(customers);
        }

        [HttpGet]
        public ODataActionTests.Customer Get(int key)
        {
            return new ODataActionTests.Customer { ID = key, Name = "Name" + key.ToString() };
        }

        [HttpPost]
        public bool DoSomething(int key, ODataActionParameters parameters)
        {
            Assert.Equal(1, key);
            Assert.Equal(1, parameters["p1"]);
            ValidateAddress(parameters["p2"] as ODataActionTests.Address);
            ValidateNumbers((parameters["p3"] as IEnumerable<string>).ToList());
            ValidateAddresses((parameters["p4"] as IEnumerable<ODataActionTests.Address>).ToList());
            return true;
        }

        private void ValidateAddress(ODataActionTests.Address address)
        {
            Assert.NotNull(address);
            Assert.Equal("1 Microsoft Way", address.StreetAddress);
            Assert.Equal("Redmond", address.City);
            Assert.Equal("WA", address.State);
            Assert.Equal(98052, address.ZipCode);
        }

        private void ValidateNumbers(IList<string> numbers)
        {
            Assert.NotNull(numbers);
            Assert.Equal(2, numbers.Count);
            Assert.Equal("one", numbers[0]);
            Assert.Equal("two", numbers[1]);
        }

        private void ValidateAddresses(IList<ODataActionTests.Address> addresses)
        {
            Assert.NotNull(addresses);
            Assert.Equal(1, addresses.Count);
            ValidateAddress(addresses[0]);
        }
    }
}
