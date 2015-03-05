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
using System.Web.OData.Formatter;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Values;
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
            Assert.Equal("0001-01-01", result["DateList"][0]);
            Assert.Equal("9999-12-31", result["DateList"][1]);
        }

        [Fact]
        public void Get_OpenEntityType_Enum_Collection_Property()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/UntypedSimpleOpenCustomers(1)";
            var configuration = new[] { typeof(UntypedSimpleOpenCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetUntypedEdmModel());

            HttpClient client = new HttpClient(new HttpServer(configuration));

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("http://localhost/odata/$metadata#UntypedSimpleOpenCustomers/$entity", result["@odata.context"]);
            Assert.Equal("#Collection(NS.Color)", result["Colors@odata.type"]);
            Assert.Equal(new JArray(new[] { "Red", "0" }), result["Colors"]);
            Assert.Equal("Red", result["Color"]);
        }

        [Fact]
        public void CanDispatch_ActionPayload_With_EdmEnumObject()
        {
            const string RequestUri = "http://localhost/odata/UntypedSimpleOpenCustomers(1)/NS.AddColor";
            const string Payload = @"{ 
                ""Color"": ""0""
            }";

            var configuration = new[] { typeof(UntypedSimpleOpenCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetUntypedEdmModel());

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
        public void Post_UnTyped_OpenEntityType()
        { 
            // Arrange
            const string Payload = "{" + 
              "\"@odata.context\":\"http://localhost/odata/$metadata#UntypedSimpleOpenCustomer/$entity\"," +
              "\"CustomerId\":6,\"Name\":\"FirstName 6\"," +
              "\"Address\":{" +
                "\"@odata.type\":\"#NS.Address\",\"Street\":\"Street 6\",\"City\":\"City 6\"" +
              "}," + 
              "\"Addresses@odata.type\":\"#Collection(NS.Address)\"," +
              "\"Addresses\":[{" +
                "\"@odata.type\":\"#NS.Address\",\"Street\":\"Street 7\",\"City\":\"City 7\"" +
              "}]," +
              "\"DoubleList@odata.type\":\"#Collection(Double)\"," +
              "\"DoubleList\":[5.5, 4.4, 3.3]," +
              "\"FavoriteColor@odata.type\":\"#NS.Color\"," +
              "\"FavoriteColor\":\"Red\"," +
              "\"Color\":\"Red\"," +
              "\"FavoriteColors@odata.type\":\"#Collection(NS.Color)\"," +
              "\"FavoriteColors\":[\"0\", \"1\"]" +
            "}";

            const string RequestUri = "http://localhost/odata/UntypedSimpleOpenCustomers";

            var configuration = new[] { typeof(UntypedSimpleOpenCustomersController) }.GetHttpConfiguration();
            configuration.MapODataServiceRoute("odata", "odata", GetUntypedEdmModel());

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

        private static IEdmModel GetUntypedEdmModel()
        {
            var model = new EdmModel();
            // complex type address
            EdmComplexType address = new EdmComplexType("NS", "Address", null, false, true);
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            model.AddElement(address);

            // enum type color
            EdmEnumType color = new EdmEnumType("NS", "Color");
            color.AddMember(new EdmEnumMember(color, "Red", new EdmIntegerConstant(0)));
            model.AddElement(color);

            // entity type customer
            EdmEntityType customer = new EdmEntityType("NS", "UntypedSimpleOpenCustomer", null, false, true);
            customer.AddKeys(customer.AddStructuralProperty("CustomerId", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Color", new EdmEnumTypeReference(color, isNullable: true));
            model.AddElement(customer);

            EdmAction action = new EdmAction(
                "NS", 
                "AddColor", 
                null, 
                isBound: true, 
                entitySetPathExpression: null);
            action.AddParameter("bindingParameter", new EdmEntityTypeReference(customer, false));
            action.AddParameter("Color", new EdmEnumTypeReference(color, true));
            model.AddElement(action);

            EdmEntityContainer container = new EdmEntityContainer("NS", "Container");
            container.AddEntitySet("UntypedSimpleOpenCustomers", customer);

            model.AddElement(container);
            return model;
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
                CustomerProperties = new Dictionary<string, object>
                {
                    { "ListProp", IntValues },
                    { "DateList", new[] { Date.MinValue, Date.MaxValue } }
                }
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
    
    // Controller
    public class UntypedSimpleOpenCustomersController : ODataController
    {
        public IHttpActionResult Get(int key)
        {
            EdmEntityType customerType = new EdmEntityType("NS", "UntypedSimpleOpenCustomer", null, false, true);
            customerType.AddKeys(customerType.AddStructuralProperty("CustomerId", EdmPrimitiveTypeKind.Int32));
            EdmEntityObject customer = new EdmEntityObject(customerType);
            customer.TrySetPropertyValue("CustomerId", 1);

            EdmEnumType colorType = new EdmEnumType("NS", "Color");
            colorType.AddMember(new EdmEnumMember(colorType, "Red", new EdmIntegerConstant(0)));

            EdmEnumObject color = new EdmEnumObject(colorType, "Red");
            EdmEnumObject color2 = new EdmEnumObject(colorType, "0");
            customer.TrySetPropertyValue("Color", color);

            List<IEdmEnumObject> colorList = new List<IEdmEnumObject>();
            colorList.Add(color);
            colorList.Add(color2);
            IEdmCollectionTypeReference collectionType = new EdmCollectionTypeReference(new EdmCollectionType(colorType.ToEdmTypeReference(false)));
            EdmEnumObjectCollection colors = new EdmEnumObjectCollection(collectionType, colorList);
            customer.TrySetPropertyValue("Colors", colors);

            return Ok(customer);
        }

        [HttpPost]
        public IHttpActionResult AddColor(int key, ODataUntypedActionParameters parameters)
        {
            Assert.Equal("0", ((EdmEnumObject)parameters["Color"]).Value);
            return Ok();
        }

        public IHttpActionResult PostUntypedSimpleOpenCustomer(EdmEntityObject customer)
        {
            // Verify there is a string dynamic property in OpenEntityType
            object nameValue;
            customer.TryGetPropertyValue("Name", out nameValue);
            Type nameType;
            customer.TryGetPropertyType("Name", out nameType);

            Assert.NotNull(nameValue);
            Assert.Equal(typeof(String), nameType);
            Assert.Equal("FirstName 6", nameValue);

            // Verify there is a collection of double dynamic property in OpenEntityType
            object doubleListValue;
            customer.TryGetPropertyValue("DoubleList", out doubleListValue);
            Type doubleListType;
            customer.TryGetPropertyType("DoubleList", out doubleListType);

            Assert.NotNull(doubleListValue);
            Assert.Equal(typeof(List<Double>), doubleListType);

            // Verify there is a collection of complex type dynamic property in OpenEntityType
            object addressesValue;
            customer.TryGetPropertyValue("Addresses", out addressesValue);

            Assert.NotNull(addressesValue);

            // Verify there is a complex type dynamic property in OpenEntityType
            object addressValue;
            customer.TryGetPropertyValue("Address", out addressValue);

            Type addressType;
            customer.TryGetPropertyType("Address", out addressType);

            Assert.NotNull(addressValue);
            Assert.Equal(typeof(EdmComplexObject), addressType);

            // Verify there is a collection of enum type dynamic property in OpenEntityType
            object favoriteColorsValue;
            customer.TryGetPropertyValue("FavoriteColors", out favoriteColorsValue);
            EdmEnumObjectCollection favoriteColors = favoriteColorsValue as EdmEnumObjectCollection;

            Assert.NotNull(favoriteColorsValue);
            Assert.NotNull(favoriteColors);
            Assert.Equal(typeof(EdmEnumObject), favoriteColors[0].GetType());

            // Verify there is an enum type dynamic property in OpenEntityType
            object favoriteColorValue;
            customer.TryGetPropertyValue("FavoriteColor", out favoriteColorValue);

            Assert.NotNull(favoriteColorValue);
            Assert.Equal("Red", ((EdmEnumObject)favoriteColorValue).Value);

            Type favoriteColorType;
            customer.TryGetPropertyType("FavoriteColor", out favoriteColorType);

            Assert.Equal(typeof(EdmEnumObject), favoriteColorType);

            // Verify there is a string dynamic property in OpenComplexType
            EdmComplexObject address = addressValue as EdmComplexObject;
            object cityValue;
            address.TryGetPropertyValue("City", out cityValue);
            Type cityType;
            address.TryGetPropertyType("City", out cityType);

            Assert.NotNull(cityValue);
            Assert.Equal(typeof(String), cityType);
            Assert.Equal("City 6", cityValue);

            return Ok(customer);
        }
    }
}
