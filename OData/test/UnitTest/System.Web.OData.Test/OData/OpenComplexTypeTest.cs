﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;
using Newtonsoft.Json.Linq;

namespace System.Web.OData
{
    public class OpenComplexTypeTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OpenComplexType_SimpleSerialization(bool enableNullDynamicProperty)
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/OpenCustomers(2)/Address";
            var configuration = new[] { typeof(OpenCustomersController) }.GetHttpConfiguration();
            configuration.SetSerializeNullDynamicProperty(enableNullDynamicProperty);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("http://localhost/odata/$metadata#OpenCustomers(2)/Address", result["@odata.context"]);
            Assert.Equal("Street 2", result["Street"]);
            Assert.Equal("City 2", result["City"]);
            Assert.Equal("300", result["IntProp"]);
            Assert.Equal("My Dynamic Country", result["Country"]);
            Assert.Equal("2c1f450a-a2a7-4fe1-a25d-4d9332fc0694", result["Token"]);
            Assert.Equal("2015-03-02", result["Birthday"]);
            if (enableNullDynamicProperty)
            {
                Assert.NotNull(result["Region"]);
                Assert.Equal(JValue.CreateNull(), result["Region"]);
            }
            else
            {
                Assert.Null(result["Region"]);
            }
        }

        [Fact]
        public void OpenComplexType_SimpleDeserialization()
        {
            // Arrange
            const string Payload = "{" +
              "\"@odata.context\":\"http://localhost/odata/$metadata#OpenCustomers/$entity\"," +
              "\"CustomerId\":6,\"Name\":\"FirstName 6\"," +
              "\"Address\":{" +
                "\"Street\":\"Street 6\",\"City\":\"City 6\",\"Country\":\"Earth\",\"Token@odata.type\":\"#Guid\"," +
                "\"Token\":\"4DB52263-4382-4BCB-A63E-3129C1B5FA0D\"," +
                "\"Number\":990," +
                "\"BirthTime@odata.type\":\"#TimeOfDay\"," +
                "\"BirthTime\":\"11:12:13.0140000\"" +
              "}" +
            "}";

            const string RequestUri = "http://localhost/odata/OpenCustomers";

            HttpConfiguration configuration = new[] { typeof(OpenCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("http://localhost/odata/$metadata#OpenCustomers/$entity", result["@odata.context"]);
            Assert.Equal("Earth", result["Address"]["Country"]);
            Assert.Equal(990, result["Address"]["Number"]);
            Assert.Equal("4DB52263-4382-4BCB-A63E-3129C1B5FA0D".ToLower(), result["Address"]["Token"]);
            Assert.Equal("11:12:13.0140000", result["Address"]["BirthTime"]);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<OpenCustomer>("OpenCustomers");
            return builder.GetEdmModel();
        }
    }

    // Controller
    public class OpenCustomersController : ODataController
    {
        private static IList<OpenCustomer> CreateCustomers()
        {
            int[] IntValues = { 200, 100, 300, 0, 400 };
            IList<OpenCustomer> customers = Enumerable.Range(0, 5).Select(i =>
                new OpenCustomer
                {
                    CustomerId = i,
                    Name = "FirstName " + i,
                    Address = new OpenAddress
                    {
                        Street = "Street " + i,
                        City = "City " + i,
                        DynamicProperties = new Dictionary<string, object> { { "IntProp", IntValues[i] } }
                    }
                }).ToList();

            return customers;
        }

        public IHttpActionResult GetAddress(int key)
        {
            IList<OpenCustomer> customers = CreateCustomers();
            OpenCustomer customer = customers.Where(c => c.CustomerId == key).FirstOrDefault();
            if (customer == null)
            {
                return NotFound();
            }

            OpenAddress address = customer.Address;

            // Add more dynamic properties
            address.DynamicProperties.Add("Country", "My Dynamic Country");
            address.DynamicProperties.Add("Token", new Guid("2C1F450A-A2A7-4FE1-A25D-4D9332FC0694"));
            address.DynamicProperties.Add("Birthday", new Date(2015, 3, 2));
            address.DynamicProperties.Add("Region", null);
            return Ok(address);
        }

        public IHttpActionResult PostOpenCustomer([FromBody]OpenCustomer customer)
        {
            // Verify there is a string dynamic property
            object countryValue;
            customer.Address.DynamicProperties.TryGetValue("Country", out countryValue);
            Assert.NotNull(countryValue);
            Assert.IsType<string>(countryValue);
            Assert.Equal("Earth", countryValue);

            // Verify there is an int dynamic property
            object numberValue;
            customer.Address.DynamicProperties.TryGetValue("Number", out numberValue);
            Assert.NotNull(numberValue);
            Assert.IsType<Int32>(numberValue);
            Assert.Equal(990, numberValue);

            // Verify there is a Guid dynamic property
            object tokenValue;
            customer.Address.DynamicProperties.TryGetValue("Token", out tokenValue);
            Assert.NotNull(tokenValue);
            Assert.IsType<Guid>(tokenValue);
            Assert.Equal(new Guid("4DB52263-4382-4BCB-A63E-3129C1B5FA0D"), tokenValue);

            // Verify there is a TimeOfDay dynamic property
            object timeOfDayValue;
            customer.Address.DynamicProperties.TryGetValue("BirthTime", out timeOfDayValue);
            Assert.NotNull(timeOfDayValue);
            Assert.IsType<TimeOfDay>(timeOfDayValue);
            Assert.Equal(new TimeOfDay(11, 12, 13, 14), timeOfDayValue);

            return Ok(customer);
        }
    }

    // Models
    public class OpenCustomer
    {
        [Key]
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public OpenAddress Address { get; set; }
    }

    public class OpenAddress
    {
        public OpenAddress()
        {
            DynamicProperties = new Dictionary<string, object>();
        }

        public string Street { get; set; }
        public string City { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }
}
