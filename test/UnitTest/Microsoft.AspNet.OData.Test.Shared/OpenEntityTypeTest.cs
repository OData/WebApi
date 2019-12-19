// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.AspNet.OData.Test
{
    public class OpenEntityTypeTest
    {
        private const string _untypedCustomerRequestRooturl = "http://localhost/odata/UntypedSimpleOpenCustomers";

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Get_OpenEntityType(bool enableNullDynamicProperty)
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers(9)";

            var controllers = new[] { typeof(SimpleOpenCustomersController) };

            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.SetSerializeNullDynamicProperty(enableNullDynamicProperty);
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("http://localhost/odata/$metadata#SimpleOpenCustomers/Microsoft.AspNet.OData.Test.Common.SimpleVipCustomer/$entity", result["@odata.context"]);
            Assert.Equal("#Microsoft.AspNet.OData.Test.Common.SimpleVipCustomer", result["@odata.type"]);
            Assert.Equal(9, result["CustomerId"]);
            Assert.Equal("VipCustomer", result["Name"]);
            Assert.Equal("#Collection(Int32)", result["ListProp@odata.type"]);
            Assert.Equal(new JArray(new[] { 200, 100, 300, 0, 400 }), result["ListProp"]);
            Assert.Equal("0001-01-01", result["DateList"][0]);
            Assert.Equal("9999-12-31", result["DateList"][1]);
            if (enableNullDynamicProperty)
            {
                Assert.NotNull(result["Receipt"]);
                Assert.Equal(JValue.CreateNull(), result["Receipt"]);
            }
            else
            {
                Assert.Null(result["Receipt"]);
            }
        }

        [Theory]
        [InlineData("http://localhost/odata/SimpleOpenCustomers?$orderby=Token&$filter=Token ne null", new[] { 2, 4 })]
        [InlineData("http://localhost/odata/SimpleOpenCustomers?$orderby=Token desc&$filter=Token ne null", new[] { 4, 2 })]
        [InlineData("http://localhost/odata/SimpleOpenCustomers?$filter=Token ne null", new[] { 2, 4 })]
        public async Task Get_OpenEntityTypeWithOrderbyAndFilter(string uri, int[] customerIds)
        {
            // Arrange
            var controllers = new[] { typeof(SimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            var resultArray = result["value"] as JArray;
            Assert.Equal(2, resultArray.Count);
            for (var i = 0; i < customerIds.Length; i++)
                Assert.Equal(customerIds[i], resultArray[i]["CustomerId"]);
        }

        [Fact]
        public async Task Get_OpenEntityType_Enum_Collection_Property()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/UntypedSimpleOpenCustomers(1)";
            var controllers = new[] { typeof(UntypedSimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetUntypedEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("http://localhost/odata/$metadata#UntypedSimpleOpenCustomers/$entity", result["@odata.context"]);
            Assert.Equal("#Collection(NS.Color)", result["Colors@odata.type"]);
            Assert.Equal(new JArray(new[] { "Red", "0" , "Red"}), result["Colors"]);
            Assert.Equal("Red", result["Color"]);
        }

        [Theory]
        [InlineData("/$count", "1")]
        [InlineData("(1)/DeclaredNumbers/$count", "2")]
        [InlineData("(1)/DeclaredColors/$count", "3")]
        [InlineData("(1)/DeclaredAddresses/$count", "2")]
        public async Task Get_UnTyped_DollarCount(string requestUri, string expectedResult)
        {
            // Arrange
            var controllers = new[] { typeof(UntypedSimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.Count();
                config.MapODataServiceRoute("odata", "odata", GetUntypedEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpResponseMessage response = await client.GetAsync(_untypedCustomerRequestRooturl + requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(expectedResult, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("(1)/Color/$value", "Red")]
        [InlineData("(1)/Color", "Red")]
        [InlineData("(1)/DeclaredColors", "0")]
        public async Task Get_UnTyped_Enum_Collection_Property(string requestUri, string expectedContainsResult)
        {
            // Arrange
            var controllers = new[] { typeof(UntypedSimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetUntypedEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpResponseMessage response = await client.GetAsync(_untypedCustomerRequestRooturl + requestUri);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Contains(expectedContainsResult, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task CanDispatch_ActionPayload_With_EdmEnumObject()
        {
            const string RequestUri = "http://localhost/odata/UntypedSimpleOpenCustomers(1)/NS.AddColor";
            const string Payload = @"{ 
                ""Color"": ""0""
            }";

            var controllers = new[] { typeof(UntypedSimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetUntypedEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentLength = Payload.Length;
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Get_OpenEntityTypeWithSelect()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers?$select=Token";
            var controllers = new[] { typeof(SimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            var resultArray = result["value"] as JArray;
            Assert.Equal(6, resultArray.Count);
            Assert.NotNull(resultArray[2]["Token"]);//customer 2 has a token
            Assert.NotNull(resultArray[4]["Token"]);//customer 4 has a token
        }

        [Theory]
        [InlineData("$select=Address/Street,Address/City")]
        [InlineData("$select=Address($select=Street,City)")]
        public async Task Get_OpenEntityTypeWithMultiplePropertySelect(string select)
        {
            // Arrange
            string requestUri = "http://localhost/odata/SimpleOpenCustomers?" + select;
            var controllers = new[] { typeof(SimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.Select();
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
          //  Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            var resultArray = result["value"] as JArray;
            Assert.Equal(6, resultArray.Count);
            Assert.Equal(@"[
  {
    ""Address"": {
      ""Street"": ""Street 0"",
      ""City"": ""City 0""
    }
  },
  {
    ""Address"": {
      ""Street"": ""Street 1"",
      ""City"": ""City 1""
    }
  },
  {
    ""Address"": {
      ""Street"": ""Street 2"",
      ""City"": ""City 2""
    }
  },
  {
    ""Address"": {
      ""Street"": ""Street 3"",
      ""City"": ""City 3""
    }
  },
  {
    ""Address"": {
      ""Street"": ""Street 4"",
      ""City"": ""City 4""
    }
  },
  {
    ""@odata.type"": ""#Microsoft.AspNet.OData.Test.Common.SimpleVipCustomer"",
    ""Address"": {
      ""Street"": ""Vip Street "",
      ""City"": ""Vip City ""
    }
  }
]", resultArray.ToString());
        }

        [Fact]
        public async Task Get_OpenEntityTypeWithSelectWildcard()
        {
            // Arrange
            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers(1)?$select=*";
            var controllers = new[] { typeof(SimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, RequestUri);
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(result["Token"]);
            Assert.NotNull(result["MyList"]);
        }

        [Fact]
        public async Task Post_OpenEntityType()
        {
            // Arrange
            const string Payload = "{" +
              "\"@odata.context\":\"http://localhost/odata/$metadata#OpenCustomers/$entity\"," +
              "\"CustomerId\":6,\"Name\":\"FirstName 6\"," +
              "\"Address\":{" +
                "\"Street\":\"Street 6\",\"City\":\"City 6\",\"Place\":\"Earth\",\"Token@odata.type\":\"#Guid\"," +
                "\"Token\":\"4DB52263-4382-4BCB-A63E-3129C1B5FA0D\"," +
                "\"Number\":990" +
              "}," +
              "\"Website\": \"WebSite #6\"," +
              "\"Place@odata.type\":\"#String\",\"Place\":\"My Dynamic Place\"," + // odata.type is necessary, otherwise it will get an ODataUntypedValue
              "\"Token@odata.type\":\"#Guid\",\"Token\":\"2c1f450a-a2a7-4fe1-a25d-4d9332fc0694\"," +
              "\"DoubleList@odata.type\":\"#Collection(Double)\"," +
              "\"DoubleList\":[5.5, 4.4, 3.3]" +
            "}";

            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers";

            var controllers = new[] { typeof(SimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentLength = Payload.Length;
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Post_OpenEntityTypeWithNullComplexTypeProperty()
        {
            // Arrange
            const string Payload = "{" +
              "\"@odata.context\":\"http://localhost/odata/$metadata#OpenCustomers/$entity\"," +
              "\"CustomerId\":99,\"Name\":\"FirstName 99\"," + // special CustomerId to test the Address == null in the controller.
              "\"Address\": null," +
              "\"Website\": \"WebSite #6\"," +
              "\"Place@odata.type\":\"#String\",\"Place\":\"My Dynamic Place\"," + // odata.type is necessary, otherwise it will get an ODataUntypedValue
              "\"Token@odata.type\":\"#Guid\",\"Token\":\"2c1f450a-a2a7-4fe1-a25d-4d9332fc0694\"," +
              "\"DoubleList@odata.type\":\"#Collection(Double)\"," +
              "\"DoubleList\":[5.5, 4.4, 3.3]" +
            "}";

            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers";
            var controllers = new[] { typeof(SimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentLength = Payload.Length;
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Post_UnTyped_OpenEntityType()
        {
            // Arrange
            const string Payload = "{" +
              "\"@odata.context\":\"http://localhost/odata/$metadata#UntypedSimpleOpenCustomers/$entity\"," +
              "\"CustomerId\":6,\"Name@odata.type\":\"#String\",\"Name\":\"FirstName 6\"," +
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

            var controllers = new[] { typeof(UntypedSimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetUntypedEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _untypedCustomerRequestRooturl);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentLength = Payload.Length;
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task Patch_OpenEntityType()
        {
            // Arrange
            const string Payload = "{" +
              "\"CustomerId\":99,\"Name\":\"ChangedName\"," +
              "\"Token@odata.type\":\"#DateTimeOffset\",\"Token\":\"2014-01-01T00:00:00Z\"," +
              "\"DoubleList@odata.type\":\"#Collection(Double)\"," +
              "\"DoubleList\":[5.5, 4.4, 3.3]" +
              "}";

            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers(2)";

            var controllers = new[] { typeof(SimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("PATCH"), RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentLength = Payload.Length;
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

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
        public async Task Put_OpenEntityType()
        {
            // Arrange
            const string Payload = "{" +
              "\"CustomerId\":99,\"Name\":\"ChangedName\"," +
              "\"Token@odata.type\":\"#DateTimeOffset\",\"Token\":\"2014-01-01T00:00:00Z\"" +
              "}";

            const string RequestUri = "http://localhost/odata/SimpleOpenCustomers(2)";

            var controllers = new[] { typeof(SimpleOpenCustomersController) };
            var server = TestServerFactory.Create(controllers, (config) =>
            {
                config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            });
            var client = TestServerFactory.CreateClient(server);

            // Act
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("Put"), RequestUri);
            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentLength = Payload.Length;
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = ODataConventionModelBuilderFactory.Create();
            builder.EntitySet<SimpleOpenCustomer>("SimpleOpenCustomers");
            builder.EntitySet<SimpleOpenOrder>("SimpleOpenOrders");
            return builder.GetEdmModel();
        }

        private static IEdmModel _untypedEdmModel;
        public static IEdmModel GetUntypedEdmModel()
        {
            if (_untypedEdmModel != null)
            {
                return _untypedEdmModel;
            }

            var model = new EdmModel();
            // complex type address
            EdmComplexType address = new EdmComplexType("NS", "Address", null, false, true);
            address.AddStructuralProperty("Street", EdmPrimitiveTypeKind.String);
            model.AddElement(address);
            IEdmCollectionTypeReference complexCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(address.ToEdmTypeReference(false)));

            // enum type color
            EdmEnumType color = new EdmEnumType("NS", "Color");
            color.AddMember(new EdmEnumMember(color, "Red", new EdmEnumMemberValue(0)));
            model.AddElement(color);
            IEdmCollectionTypeReference enumCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(color.ToEdmTypeReference(false)));

            // primitive collection type
            IEdmTypeReference intType = EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.Int32, isNullable: true);
            EdmCollectionTypeReference primitiveCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(intType));

            // entity type customer
            EdmEntityType customer = new EdmEntityType("NS", "UntypedSimpleOpenCustomer", null, false, true);
            customer.AddKeys(customer.AddStructuralProperty("CustomerId", EdmPrimitiveTypeKind.Int32));
            customer.AddStructuralProperty("Color", new EdmEnumTypeReference(color, isNullable: true));
            customer.AddStructuralProperty("DeclaredAddresses", complexCollectionType);
            customer.AddStructuralProperty("DeclaredColors", enumCollectionType);
            customer.AddStructuralProperty("DeclaredNumbers", primitiveCollectionType);

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
            _untypedEdmModel = model;
            return model;
        }
    }

    // Controller
    public class SimpleOpenCustomersController : TestODataController
    {
        [EnableQuery]
        public IQueryable<SimpleOpenCustomer> Get()
        {
            return CreateCustomers().AsQueryable();
        }

        public ITestActionResult Get(int key)
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

        public ITestActionResult GetAddress(int key)
        {
            IList<SimpleOpenCustomer> customers = CreateCustomers();
            SimpleOpenCustomer customer = customers.FirstOrDefault(c => c.CustomerId == key);
            if (customer == null)
            {
                return NotFound();
            }

            return Ok(customer.Address);
        }

        public ITestActionResult PostSimpleOpenCustomer([FromBody]SimpleOpenCustomer customer)
        {
            // Verify there is a string dynamic property
            object countryValue;
            customer.CustomerProperties.TryGetValue("Place", out countryValue);
            Assert.NotNull(countryValue);
            Assert.Equal(typeof(String), countryValue.GetType());
            Assert.Equal("My Dynamic Place", countryValue);

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

            // special test cases to test the complex type property value is null.
            if (customer.CustomerId == 99)
            {
                Assert.Null(customer.Address);
            }

            return Ok();
        }

        public ITestActionResult Patch(int key, Delta<SimpleOpenCustomer> patch)
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

        public ITestActionResult Put(int key, [FromBody]SimpleOpenCustomer changedCustomer)
        {
            Assert.Equal(99, changedCustomer.CustomerId);
            Assert.Equal("ChangedName", changedCustomer.Name);
            Assert.Null(changedCustomer.Address);
            Assert.Null(changedCustomer.Website);
            Assert.Single(changedCustomer.CustomerProperties);
            return Updated(changedCustomer); // Updated(customer);
        }

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

            customers[4].CustomerProperties = new Dictionary<string, object>
            {
                {"Token", new Guid("A6A594ED-375B-424E-AC0A-945D89CF7B9B")},
                {"IntList", new List<int> { 1, 2, 3, 4, 5, 6, 7 }},
            };

            SimpleOpenAddress address = new SimpleOpenAddress
            {
                Street = "SubStreet",
                City = "City"
            };
            customers[3].CustomerProperties = new Dictionary<string, object> { { "ComplexList", new[] { address, address } } };

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
                    { "DateList", new[] { Date.MinValue, Date.MaxValue } },
                    { "Receipt", null }
                }
            };

            customers.Add(vipCustomer);
            return customers;
        }
    }

    // Controller
    public class UntypedSimpleOpenCustomersController : TestODataController
    {
        private static EdmEntityObjectCollection _untypedSimpleOpenCustormers;

        [EnableQuery]
        public ITestActionResult Get()
        {
            return Ok(GetCustomers());
        }

        public ITestActionResult Get(int key)
        {
            return Ok(GetCustomers()[0]);
        }

        [EnableQuery]
        public ITestActionResult GetDeclaredAddresses(int key)
        {
            object addresses;
            GetCustomers()[0].TryGetPropertyValue("DeclaredAddresses", out addresses);
            return Ok((EdmComplexObjectCollection)addresses);
        }


        [EnableQuery]
        public ITestActionResult GetColor(int key)
        {
            object color;
            GetCustomers()[0].TryGetPropertyValue("Color", out color);
            return Ok((EdmEnumObject)color);
        }

        [EnableQuery]
        public ITestActionResult GetDeclaredColors(int key)
        {
            object colors;
            GetCustomers()[0].TryGetPropertyValue("DeclaredColors", out colors);
            return Ok((EdmEnumObjectCollection)colors);
        }

        [EnableQuery]
        public ITestActionResult GetDeclaredNumbers(int key)
        {
            object numbers;
            GetCustomers()[0].TryGetPropertyValue("DeclaredNumbers", out numbers);
            return Ok(numbers as int[]);
        }

        [HttpPost]
        public ITestActionResult AddColor(int key, ODataUntypedActionParameters parameters)
        {
            Assert.Equal("0", ((EdmEnumObject)parameters["Color"]).Value);
            return Ok();
        }

        public ITestActionResult PostUntypedSimpleOpenCustomer(EdmEntityObject customer)
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
            Assert.Equal("City 6", cityValue); // It reads as ODataUntypedValue, and the RawValue is the string with the ""

            return Ok(customer);
        }

        private static EdmEntityObjectCollection GetCustomers()
        {
            if (_untypedSimpleOpenCustormers != null)
            {
                return _untypedSimpleOpenCustormers;
            }

            IEdmModel edmModel = OpenEntityTypeTest.GetUntypedEdmModel();
            IEdmEntityType customerType = edmModel.SchemaElements.OfType<IEdmEntityType>().First(c => c.Name == "UntypedSimpleOpenCustomer");
            EdmEntityObject customer = new EdmEntityObject(customerType);
            customer.TrySetPropertyValue("CustomerId", 1);

            //Add Numbers primitive collection property
            customer.TrySetPropertyValue("DeclaredNumbers", new[] { 1, 2 });

            //Add Color, Colors enum(collection) property
            IEdmEnumType colorType = edmModel.SchemaElements.OfType<IEdmEnumType>().First(c => c.Name == "Color");
            EdmEnumObject color = new EdmEnumObject(colorType, "Red");
            EdmEnumObject color2 = new EdmEnumObject(colorType, "0");
            EdmEnumObject color3 = new EdmEnumObject(colorType, "Red");
            customer.TrySetPropertyValue("Color", color);

            List<IEdmEnumObject> colorList = new List<IEdmEnumObject>();
            colorList.Add(color);
            colorList.Add(color2);
            colorList.Add(color3);
            IEdmCollectionTypeReference enumCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(colorType.ToEdmTypeReference(false)));
            EdmEnumObjectCollection colors = new EdmEnumObjectCollection(enumCollectionType, colorList);
            customer.TrySetPropertyValue("Colors", colors);
            customer.TrySetPropertyValue("DeclaredColors", colors);

            //Add Addresses complex(collection) property
            EdmComplexType addressType =
                edmModel.SchemaElements.OfType<IEdmComplexType>().First(c => c.Name == "Address") as EdmComplexType;
            EdmComplexObject address = new EdmComplexObject(addressType);
            address.TrySetPropertyValue("Street", "No1");
            EdmComplexObject address2 = new EdmComplexObject(addressType);
            address2.TrySetPropertyValue("Street", "No2");

            List<IEdmComplexObject> addressList = new List<IEdmComplexObject>();
            addressList.Add(address);
            addressList.Add(address2);
            IEdmCollectionTypeReference complexCollectionType = new EdmCollectionTypeReference(new EdmCollectionType(addressType.ToEdmTypeReference(false)));
            EdmComplexObjectCollection addresses = new EdmComplexObjectCollection(complexCollectionType, addressList);
            customer.TrySetPropertyValue("DeclaredAddresses", addresses);

            EdmEntityObjectCollection customers = new EdmEntityObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(customerType.ToEdmTypeReference(false))));
            customers.Add(customer);
            _untypedSimpleOpenCustormers = customers;
            return _untypedSimpleOpenCustormers;
        }
    }
}
