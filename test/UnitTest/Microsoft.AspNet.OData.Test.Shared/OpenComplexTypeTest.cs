// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
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
            HttpClient client = GetClient(enableNullDynamicProperty);

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
            HttpClient client = GetClient();

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
              "\"Publish\":\"2016-02-02\"," +
              "\"LineA\": {" +
                "\"Description\": \"DescLineA.\"," +
                "\"Fee\" : 0," +
                "\"PhoneInfo\" : {" +
                    "\"ContactName\" : \"ContactNameA\"," +
                    "\"PhoneNumber\" : 9876543," +
                    "\"Spec\" : { \"Make\" : \"Apple\",\"ScreenSize\" : 6 }" +
                "}" +
              "}," +
              "\"LineB\": {" +
                "\"Description\": \"DescLineB.\"," +
                "\"Fee\" : 0," +
                "\"PhoneInfo\" : {" +
                    "\"ContactName\" : \"ContactNameA\"," +
                    "\"PhoneNumber\" : 9876543," +
                    "\"Spec\" : { \"Make\" : \"Apple\",\"ScreenSize\" : 6 }" +
                "}" +
              "}" +
            "}";

            const string requestUri = "http://localhost/odata/OpenCustomers(1)/Address";
            HttpClient client = GetClient();

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
            string payload = "{" +
              "\"Street\":\"UpdatedStreet\"," +
              "\"Token@odata.type\":\"#Guid\"," +
              "\"Token\":\"2E724E81-8462-4BA0-B920-DC87A61C8EA3\"," +
              "\"BirthDay@odata.type\":\"#Date\"," +
              "\"BirthDay\":\"2016-01-29\"" +
            "}";

            await ExecutePatchRequest(payload);
        }

        [Fact]
        public async Task OpenComplexType_PatchNestedComplexTypeProperty()
        {
            string payload = @"{
                ""Street"":""UpdatedStreet"",
                ""Token@odata.type"":""#Guid"",
                ""Token"":""9B198CA0-9546-4162-A4C0-14EAA255ACA7"",
                ""BirthDay@odata.type"":""#Date"",
                ""BirthDay"":""2016-01-29"",
                ""LineB"": {""Description"": ""DescriptionB""}
            }";

            await ExecutePatchRequest(payload);
        }

        [Fact]
        public async Task OpenComplexType_PatchComplexTypeDynamicProperty()
        {
            string payload = @"{
                ""Street"":""UpdatedStreet"",
                ""Token@odata.type"":""#Guid"",
                ""Token"":""250EFC6E-8FA4-4B68-951C-F1E26DE09D1D"",
                ""BirthDay@odata.type"":""#Date"",
                ""BirthDay"":""2016-01-29"",
                ""Telephone"":{
                    ""@odata.type"":""#Microsoft.AspNet.OData.Test.Phone"",
                    ""ContactName"":""ContactNameX"",
                    ""PhoneNumber"":13,
                    ""Spec"":{""ScreenSize"":7}
                }
            }";

            await ExecutePatchRequest(payload);
        }

        [Fact]
        public async Task OpenComplexType_PatchComplexTypeDynamicProperty_Nested()
        {
            string payload = @"{
                ""Street"":""UpdatedStreet"",
                ""Token@odata.type"":""#Guid"",
                ""Token"":""40CEEEDE-031C-45CB-9E44-E6017D635814"",
                ""BirthDay@odata.type"":""#Date"",
                ""BirthDay"":""2016-01-29"",
                ""Building"":{
                    ""@odata.type"":""#Microsoft.AspNet.OData.Test.Building"",
                    ""BuildingName"":""BuildingNameY"",
                    ""Telephone"":{
                        ""@odata.type"":""#Microsoft.AspNet.OData.Test.Phone"",
                        ""ContactName"":""ContactNameZ"",
                        ""PhoneNumber"":17,
                        ""Spec"":{""ScreenSize"":5}
                    }
                }
            }";

            await ExecutePatchRequest(payload);
        }

        [Fact]
        public async Task OpenComplexType_PatchNestedComplexTypeProperty_DoubleNested()
        {
            string payload = @"{
                ""Street"":""UpdatedStreet"",
                ""Token@odata.type"":""#Guid"",
                ""Token"":""A4D09554-5551-4B36-A1CB-CFBCDB1F4EAD"",
                ""BirthDay@odata.type"":""#Date"",
                ""BirthDay"":""2016-01-29"",
                ""LineA"": {
                    ""Description"": ""DescriptionA"",
                    ""PhoneInfo"" : {""ContactName"":""ContactNameA"", ""PhoneNumber"": 7654321}
                },
                ""LineB"": {
                    ""PhoneInfo"": {""ContactName"": ""ContactNameB""}
                }
            }";

            await ExecutePatchRequest(payload);
        }

        [Fact]
        public async Task OpenComplexType_PatchNestedComplexTypeProperty_DeepNestedResourceOnNewSubNode()
        {
            // Arrange
            string payload = @"{
                ""Street"":""UpdatedStreet"",
                ""Token@odata.type"":""#Guid"",
                ""Token"":""2D071BD4-E4FB-4639-8024-BBC173850441"",
                ""BirthDay@odata.type"":""#Date"",
                ""BirthDay"":""2016-01-29"",
                ""LineA"": {
                    ""Description"": ""LineDetailsWithNewDeepSubNode."",
                    ""PhoneInfo"" : {
                        ""ContactName"":""ContactNameA"",
                        ""Spec"" : { ""ScreenSize"" : 6 }
                    }
                }
            }";

            await ExecutePatchRequest(payload);
        }

        [Fact]
        public async Task OpenComplexType_PatchNestedComplexTypeProperty_GetInstance()
        {
            string payload = @"{
                ""Street"":""UpdatedStreet"",
                ""Token@odata.type"":""#Guid"",
                ""Token"":""3CA243CF-460A-4144-B6EB-F5E1180ABDC8"",
                ""BirthDay@odata.type"":""#Date"",
                ""BirthDay"":""2016-01-29"",
                ""LineA"": {
                    ""Description"": ""DescriptionA"",
                    ""PhoneInfo"" : {""ContactName"":""ContactNameA"", ""PhoneNumber"": 7654321}
                }
            }";

            await ExecutePatchRequest(payload);
        }

        private async Task ExecutePatchRequest(string payload)
        {
            // Arrange
            payload = Regex.Replace(payload, @"\s*", "", RegexOptions.Multiline);

            const string requestUri = "http://localhost/odata/OpenCustomers(1)/Address";
            HttpClient client = GetClient();

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
            HttpClient client = GetClient();

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
            builder.ComplexType<Building>();
            return builder.GetEdmModel();
        }

        private HttpClient GetClient(bool enableNullDynamicProperty = false)
        {
            var controllers = new[] { typeof(OpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                if (enableNullDynamicProperty)
                {
                    config.SetSerializeNullDynamicProperty(enableNullDynamicProperty);
                }

                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            return TestServerFactory.CreateClient(server);
        }
    }

    // Controller
    public class OpenCustomersController : TestODataController
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
                        DynamicProperties = new Dictionary<string, object> { { "IntProp", IntValues[i] } },
                        LineA = null, // Leaving LineA as null
                        LineB = new LineDetails() { Fee = LineDetails.DefaultValue_Fee }
                    }
                }).ToList();

            return customers;
        }

        public ITestActionResult GetAddress(int key)
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

        public ITestActionResult PostOpenCustomer([FromBody]OpenCustomer customer)
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

        public ITestActionResult PutToAddress(int key, Delta<OpenAddress> address)
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

        public ITestActionResult PatchToAddress(int key, Delta<OpenAddress> address)
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

            Assert.True(origin.DynamicProperties.Count >= 3); // Including the origin dynamic properties

            KeyValuePair<string, object> dynamicPropertyBirthDay = origin.DynamicProperties.FirstOrDefault(e => e.Key == "BirthDay");
            Assert.Equal(new Date(2016, 1, 29), dynamicPropertyBirthDay.Value);

            string dynamicPropertyToken = origin.DynamicProperties.FirstOrDefault(e => e.Key == "Token")
                .Value.ToString().ToUpperInvariant();

            switch (dynamicPropertyToken)
            {
                case "2E724E81-8462-4BA0-B920-DC87A61C8EA3":
                    // All changed non-dynamic properties: ["Street"]
                    Assert.True(address.GetChangedPropertyNames().Count() == 1);
                    break;

                case "9B198CA0-9546-4162-A4C0-14EAA255ACA7":

                    // All changed non-dynamic properties: ["Street", "LineB"]
                    Assert.True(address.GetChangedPropertyNames().Count() == 2);

                    Assert.Null(origin.LineA);

                    Assert.NotNull(origin.LineB);
                    Assert.Equal("DescriptionB", origin.LineB.Description);

                    // Fee is not overwritten.
                    Assert.Equal(LineDetails.DefaultValue_Fee, origin.LineB.Fee);
                    break;

                case "A4D09554-5551-4B36-A1CB-CFBCDB1F4EAD":

                    // All changed non-dynamic properties: ["Street", "LineA", "LineB"]
                    Assert.True(address.GetChangedPropertyNames().Count() == 3);

                    // --- LineA ---
                    Assert.NotNull(origin.LineA);
                    Assert.Equal("DescriptionA", origin.LineA.Description);

                    // LineA.Fee is left as uninitialized.
                    Assert.Equal(LineDetails.UninitializedValue_Fee, origin.LineA.Fee);

                    Assert.NotNull(origin.LineA.PhoneInfo);
                    Assert.Equal("ContactNameA", origin.LineA.PhoneInfo.ContactName);
                    Assert.Equal(7654321, origin.LineA.PhoneInfo.PhoneNumber);

                    // --- LineB ---
                    Assert.NotNull(origin.LineB);
                    Assert.Null(origin.LineB.Description);

                    // LineB.Fee is originally initialized for OpenAddress is created for each customer.
                    Assert.Equal(LineDetails.DefaultValue_Fee, origin.LineB.Fee);

                    Assert.NotNull(origin.LineB.PhoneInfo);
                    Assert.Equal("ContactNameB", origin.LineB.PhoneInfo.ContactName);
                    Assert.Equal(0, origin.LineB.PhoneInfo.PhoneNumber);
                    break;

                case "2D071BD4-E4FB-4639-8024-BBC173850441":
                    // All changed non-dynamic properties: ["Street", "LineA"]
                    Assert.True(address.GetChangedPropertyNames().Count() == 2);

                    // --- LineA ---
                    Assert.NotNull(origin.LineA);
                    Assert.Equal("LineDetailsWithNewDeepSubNode.", origin.LineA.Description);
                    Assert.Equal(LineDetails.UninitializedValue_Fee, origin.LineA.Fee);

                    Assert.NotNull(origin.LineA.PhoneInfo);
                    Assert.Equal("ContactNameA", origin.LineA.PhoneInfo.ContactName);
                    Assert.Equal(0, origin.LineA.PhoneInfo.PhoneNumber);

                    Assert.NotNull(origin.LineA.PhoneInfo.Spec);
                    Assert.Null(origin.LineA.PhoneInfo.Spec.Make);
                    Assert.Equal(6, origin.LineA.PhoneInfo.Spec.ScreenSize);

                    // --- LineB ---
                    Assert.NotNull(origin.LineB);
                    break;

                case "250EFC6E-8FA4-4B68-951C-F1E26DE09D1D":
                    Assert.True(origin.DynamicProperties.ContainsKey("Telephone"));
                    // Telephone dynamic property
                    Phone telephone = origin.DynamicProperties["Telephone"] as Phone;
                    Assert.NotNull(telephone);
                    Assert.Equal("ContactNameX", telephone.ContactName);
                    Assert.Equal(13, telephone.PhoneNumber);

                    // Nested complex property
                    Assert.NotNull(telephone.Spec);
                    Assert.Equal(7, telephone.Spec.ScreenSize);
                    break;

                case "40CEEEDE-031C-45CB-9E44-E6017D635814":
                    Assert.True(origin.DynamicProperties.ContainsKey("Building"));
                    // Building dynamic property
                    Building building = origin.DynamicProperties["Building"] as Building;
                    Assert.NotNull(building);
                    Assert.Equal("BuildingNameY", building.BuildingName);

                    // Nested telephone complex dynamic property
                    Assert.True(building.DynamicProperties.ContainsKey("Telephone"));
                    Phone phone = building.DynamicProperties["Telephone"] as Phone;
                    Assert.NotNull(phone);
                    Assert.Equal("ContactNameZ", phone.ContactName);
                    Assert.Equal(17, phone.PhoneNumber);

                    // Nested complex property
                    Assert.NotNull(phone.Spec);
                    Assert.Equal(5, phone.Spec.ScreenSize);
                    break;
                case "3CA243CF-460A-4144-B6EB-F5E1180ABDC8":
                    OpenAddress addressInstance = address.GetInstance();
                    Assert.NotNull(addressInstance);

                    // Complex property
                    Assert.NotNull(addressInstance.LineA);
                    Assert.NotNull(addressInstance.LineA.PhoneInfo);
                    Assert.Equal(7654321, addressInstance.LineA.PhoneInfo.PhoneNumber);

                    object lineAValue;
                    // Fetch LineA property using TryGetPropertyValue
                    Assert.True(address.TryGetPropertyValue("LineA", out lineAValue));
                    LineDetails lineA = lineAValue as LineDetails;
                    Assert.NotNull(lineA);

                    // Nested complex property
                    Assert.NotNull(lineA.PhoneInfo);
                    Assert.Equal(7654321, lineA.PhoneInfo.PhoneNumber);
                    break;
                default:
                    // Error
                    Assert.True(false, "Unexpected token value " + dynamicPropertyToken);
                    break;
            }

            return Updated(customer);
        }

        public ITestActionResult DeleteToAddress(int key)
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

        public LineDetails LineA { get; set; }

        public LineDetails LineB { get; set; }
    }

    public class LineDetails
    {
        internal const int UninitializedValue_Fee = -1;
        internal const int DefaultValue_Fee = 20;

        public LineDetails()
        {
            Fee = UninitializedValue_Fee;
        }

        public string Description { get; set; }

        public int Fee { get; set; }

        public Phone PhoneInfo { get; set; }
    }

    public class Phone
    {
        public string ContactName { get; set; }

        public int PhoneNumber { get; set; }

        public PhoneHardwareSpec Spec { get; set; }
    }

    public class PhoneHardwareSpec
    {
        public string Make { get; set; }

        public int ScreenSize { get; set; }
    }

    public class Building
    {
        public string BuildingName { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; } = new Dictionary<string, object>();
    }
}
