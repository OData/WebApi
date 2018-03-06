// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE // TODO #939: Enable these test on AspNetCore.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.Factories;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.AspNet.OData
{
    public class OpenComplexTypeTest
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task OpenComplexType_SimpleSerialization(bool enableNullDynamicProperty)
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/OpenCustomers(2)/Address";
            var configuration = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(OpenCustomersController) });
            configuration.SetSerializeNullDynamicProperty(enableNullDynamicProperty);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("http://localhost/odata/$metadata#OpenCustomers(2)/Address", result["@odata.context"]);
            Assert.Equal("Street 2", result["Street"]);
            Assert.Equal("City 2", result["City"]);
            Assert.Equal("300", result["IntProp"]);
            Assert.Equal("My Dynamic Place", result["Place"]);
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
        public async Task OpenComplexType_SimpleDeserialization()
        {
            // Arrange
            const string Payload = "{" +
              "\"@odata.context\":\"http://localhost/odata/$metadata#OpenCustomers/$entity\"," +
              "\"CustomerId\":6,\"Name\":\"FirstName 6\"," +
              "\"Address\":{" +
                "\"Street\":\"Street 6\",\"City\":\"City 6\",\"Place@odata.type\":\"#String\",\"Place\":\"Earth\",\"Token@odata.type\":\"#Guid\"," +
                "\"Token\":\"4DB52263-4382-4BCB-A63E-3129C1B5FA0D\"," +
                "\"Number@odata.type\":\"#Int32\"," +
                "\"Number\":990," +
                "\"BirthTime@odata.type\":\"#TimeOfDay\"," +
                "\"BirthTime\":\"11:12:13.0140000\"" +
              "}" +
            "}";

            const string RequestUri = "http://localhost/odata/OpenCustomers";

            var configuration = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(OpenCustomersController) });
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("http://localhost/odata/$metadata#OpenCustomers/$entity", result["@odata.context"]);
            Assert.Equal("Earth", result["Address"]["Place"]);
            Assert.Equal(990, result["Address"]["Number"]);
            Assert.Equal("4DB52263-4382-4BCB-A63E-3129C1B5FA0D".ToLower(), result["Address"]["Token"]);
            Assert.Equal("11:12:13.0140000", result["Address"]["BirthTime"]);
        }

        [Fact]
        public async Task OpenComplexType_PutComplexTypeProperty()
        {
            // Arrange
            const string payload = "{" +
              "\"Street\":\"UpdatedStreet\"," +
              "\"City\":\"UpdatedCity\"," +
              "\"Publish@odata.type\":\"#Date\"," +
              "\"Publish\":\"2016-02-02\"" +
            "}";

            const string requestUri = "http://localhost/odata/OpenCustomers(1)/Address";

            var configuration = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(OpenCustomersController) });
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task OpenComplexType_PatchComplexTypeProperty()
        {
            // Arrange
            const string payload = "{" +
              "\"Street\":\"UpdatedStreet\"," +
              "\"Token@odata.type\":\"#Guid\"," +
              "\"Token\":\"2E724E81-8462-4BA0-B920-DC87A61C8EA3\"," +
              "\"BirthDay@odata.type\":\"#Date\"," +
              "\"BirthDay\":\"2016-01-29\"" +
            "}";

            const string requestUri = "http://localhost/odata/OpenCustomers(1)/Address";

            var configuration = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(OpenCustomersController) });
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Patch"), requestUri);
            request.Content = new StringContent(payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task OpenComplexType_DeleteComplexTypeProperty()
        {
            // Arrange
            const string requestUri = "http://localhost/odata/OpenCustomers(1)/Address";

            var configuration = RoutingConfigurationFactory.CreateWithTypes(new[] { typeof(OpenCustomersController) });
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
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
            OpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            OpenAddress address = customer.Address;

            // Add more dynamic properties
            address.DynamicProperties.Add("Place", "My Dynamic Place");
            address.DynamicProperties.Add("Token", new Guid("2C1F450A-A2A7-4FE1-A25D-4D9332FC0694"));
            address.DynamicProperties.Add("Birthday", new Date(2015, 3, 2));
            address.DynamicProperties.Add("Region", null);
            return Ok(address);
        }

        public IHttpActionResult PostOpenCustomer([FromBody]OpenCustomer customer)
        {
            // Verify there is a string dynamic property
            object countryValue;
            customer.Address.DynamicProperties.TryGetValue("Place", out countryValue);
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

        public IHttpActionResult PutToAddress(int key, Delta<OpenAddress> address)
        {
            IList<OpenCustomer> customers = CreateCustomers();
            OpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            // Verify the origin address
            OpenAddress origin = customer.Address;
            VerifyOriginAddress(key, origin);

            address.Put(origin); // Do put

            // Verify the put address
            Assert.Equal("UpdatedStreet", origin.Street);
            Assert.Equal("UpdatedCity", origin.City);

            Assert.NotNull(origin.DynamicProperties);
            KeyValuePair<string, object> dynamicProperty = Assert.Single(origin.DynamicProperties); // only one
            Assert.Equal("Publish", dynamicProperty.Key);
            Assert.Equal(new Date(2016, 2, 2), dynamicProperty.Value);

            return Updated(customer);
        }

        public IHttpActionResult PatchToAddress(int key, Delta<OpenAddress> address)
        {
            IList<OpenCustomer> customers = CreateCustomers();
            OpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            // Verify the origin address
            OpenAddress origin = customer.Address;
            VerifyOriginAddress(key, origin);

            address.Patch(origin); // Do patch

            // Verify the patched address
            Assert.Equal("UpdatedStreet", origin.Street);
            Assert.Equal("City " + key, origin.City); // not changed
            Assert.NotNull(origin.DynamicProperties);

            Assert.Equal(3, origin.DynamicProperties.Count); // include the origin dynamic properties

            KeyValuePair<string, object> dynamicProperty = origin.DynamicProperties.FirstOrDefault(e => e.Key == "Token");
            Assert.Equal(new Guid("2E724E81-8462-4BA0-B920-DC87A61C8EA3"), dynamicProperty.Value);

            dynamicProperty = origin.DynamicProperties.FirstOrDefault(e => e.Key == "BirthDay");
            Assert.Equal(new Date(2016, 1, 29), dynamicProperty.Value);

            return Updated(customer);
        }

        public IHttpActionResult DeleteToAddress(int key)
        {
            IList<OpenCustomer> customers = CreateCustomers();
            OpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            customer.Address = null; // A successful DELETE request to the edit URL for a structural property, ... sets the property to null.
            return Updated(customer); // On success, the service MUST respond with 204 No Content and an empty body.
        }

        private static void VerifyOriginAddress(int key, OpenAddress address)
        {
            Assert.NotNull(address);
            Assert.Equal("Street " + key, address.Street);
            Assert.Equal("City " + key, address.City);
            Assert.NotNull(address.DynamicProperties);
            KeyValuePair<string, object> dynamicProperty = Assert.Single(address.DynamicProperties);
            Assert.Equal("IntProp", dynamicProperty.Key);
            Assert.Equal(new int[] { 200, 100, 300, 0, 400 }[key], dynamicProperty.Value);
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
#endif