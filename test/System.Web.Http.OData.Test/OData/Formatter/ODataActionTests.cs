// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Microsoft.TestCommon;

namespace System.Web.Http.OData.Formatter
{
    public class ODataActionTests
    {
        ODataMediaTypeFormatter _formatter;
        HttpConfiguration _configuration;
        HttpServer _server;
        HttpClient _client;
        IEdmModel _model;

        public ODataActionTests()
        {
            _configuration = new HttpConfiguration();
            _model = GetModel();
            _formatter = new ODataMediaTypeFormatter(_model);
            _configuration.Formatters.Clear();
            _configuration.SetODataFormatter(_formatter);

            _configuration.Routes.MapHttpRoute("default", "{action}", new { Controller = "ODataActions" });
            _configuration.Routes.MapHttpRoute(ODataRouteNames.GetById, "{controller}({id})");
            _configuration.Routes.MapHttpRoute(ODataRouteNames.Default, "{controller}");

            _server = new HttpServer(_configuration);
            _client = new HttpClient(_server);
        }

        [Fact]
        public void Can_dispatch_actionPayload_to_action()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/DoSomething");
            request.Headers.Add("accept", "application/json;odata=verbose");
            string payload = @"{ 
                ""p1"": 1, 
                ""p2"": { ""StreetAddress"": ""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 }, 
                ""p3"": [ ""one"", ""two"" ],  
                ""p4"": [ { ""StreetAddress"": ""1 Microsoft Way"", ""City"": ""Redmond"", ""State"": ""WA"", ""ZipCode"": 98052 } ]
            }";

            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");

            HttpResponseMessage response = _client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
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

    public class ODataActionsController : ApiController
    {
        [HttpPost]
        public bool DoSomething(ODataActionParameters parameters)
        {
            Assert.Equal(1, parameters["p1"]);
            ValidateAddress(parameters["p2"] as ODataActionTests.Address);
            ValidateNumbers(parameters["p3"] as IList<string>);
            ValidateAddresses(parameters["p4"] as IList<ODataActionTests.Address>);
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
