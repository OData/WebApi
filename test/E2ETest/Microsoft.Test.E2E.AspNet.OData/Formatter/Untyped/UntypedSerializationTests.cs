// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped
{
    public class UntypedSerializationTests : WebHostTestBase<UntypedSerializationTests>
    {
        public UntypedSerializationTests(WebHostTestFixture<UntypedSerializationTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("untyped", "untyped", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            var customers = builder.EntitySet<UntypedCustomer>("UntypedCustomers");
            customers.EntityType.Property(c => c.Name).IsRequired();
            var orders = builder.EntitySet<UntypedOrder>("UntypedOrders");
            customers.EntityType.Collection.Action("PrimitiveCollection").ReturnsCollection<int>();
            customers.EntityType.Collection.Action("ComplexObjectCollection").ReturnsCollection<UntypedAddress>();
            customers.EntityType.Collection.Action("EntityCollection").ReturnsCollectionFromEntitySet<UntypedOrder>("UntypedOrders");
            customers.EntityType.Collection.Action("SinglePrimitive").Returns<int>();
            customers.EntityType.Collection.Action("SingleComplexObject").Returns<UntypedAddress>();
            customers.EntityType.Collection.Action("SingleEntity").ReturnsFromEntitySet<UntypedOrder>("UntypedOrders");
            customers.EntityType.Collection.Action("EnumerableOfIEdmObject").ReturnsFromEntitySet<UntypedOrder>("UntypedOrders");

            var untypedAction = customers.EntityType.Collection.Action("UntypedParameters");
            untypedAction.Parameter<UntypedAddress>("address");
            untypedAction.Parameter<int>("value");
            untypedAction.CollectionParameter<UntypedAddress>("addresses");
            untypedAction.CollectionParameter<int>("values");
            untypedAction.Returns<UntypedAddress>();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("application/json")]
        [InlineData("application/json;odata.metadata=none")]
        [InlineData("application/json;odata.metadata=minimal")]
        [InlineData("application/json;odata.metadata=full")]
        public async Task UntypedWorksInAllFormats(string acceptHeader)
        {
            string url = "/untyped/UntypedCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            HttpResponseMessage response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        [Theory]
        [InlineData("PrimitiveCollection")]
        [InlineData("ComplexObjectCollection")]
        [InlineData("EntityCollection")]
        [InlineData("SinglePrimitive")]
        [InlineData("SingleComplexObject")]
        [InlineData("SingleEntity")]
        public async Task UntypedWorksForAllKindsOfDataTypes(string actionName)
        {
            object expectedPayload = null;
            expectedPayload = (actionName == "PrimitiveCollection") ? new { value = Enumerable.Range(1, 10) } : expectedPayload;
            expectedPayload = (actionName == "ComplexObjectCollection") ? new { value = CreateAddresses(10) } : expectedPayload;
            expectedPayload = (actionName == "EntityCollection") ? new { value = CreateOrders(10) } : expectedPayload;
            expectedPayload = (actionName == "SinglePrimitive") ? new { value = 10 } : expectedPayload;
            expectedPayload = (actionName == "SingleComplexObject") ? CreateAddress(10) : expectedPayload;
            expectedPayload = (actionName == "SingleEntity") ? CreateOrder(10) : expectedPayload;

            string url = "/untyped/UntypedCustomers/Default." + actionName;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            JToken result = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(JToken.FromObject(expectedPayload), result, JToken.EqualityComparer);
        }

        [Fact]
        public async Task RoundTripEntityWorks()
        {
            int i = 10;
            JObject untypedCustomer = new JObject();
            untypedCustomer["Id"] = i;
            untypedCustomer["Name"] = string.Format("Name {0}", i);
            untypedCustomer["Orders"] = CreateOrders(i);
            untypedCustomer["Addresses"] = CreateAddresses(i);
            untypedCustomer["FavoriteNumbers"] = new JArray(Enumerable.Range(0, i).ToArray());

            string url = "/untyped/UntypedCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + url);
            request.Content = new StringContent(untypedCustomer.ToString());
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            HttpRequestMessage getRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}{1}({2})?$expand=Orders", BaseAddress, url, i));
            getRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpResponseMessage getResponse = await Client.SendAsync(getRequest);
            Assert.True(getResponse.IsSuccessStatusCode);
            Assert.NotNull(getResponse.Content);
            JObject returnedObject = await getResponse.Content.ReadAsObject<JObject>();
            Assert.Equal(untypedCustomer, returnedObject, JToken.EqualityComparer);
        }


        [Fact]
        public async Task UntypedActionParametersRoundtrip()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "/untyped/UntypedCustomers/Default.UntypedParameters");
            object payload = new { address = CreateAddress(5), value = 5, addresses = CreateAddresses(10), values = Enumerable.Range(0, 5) };
            request.Content = new StringContent((JToken.FromObject(payload) as JObject).ToString());
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            var body = await request.Content.ReadAsStringAsync();
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
        }

        private static JArray CreateAddresses(int i)
        {
            JArray addresses = new JArray();
            for (int j = 0; j < i; j++)
            {
                JObject complexObject = CreateAddress(j);
                addresses.Add(complexObject);
            }
            return addresses;
        }

        private static JArray CreateOrders(int i)
        {
            JArray orders = new JArray();
            for (int j = 0; j < i; j++)
            {
                JObject order = new JObject();
                order["Id"] = j;
                order["ShippingAddress"] = CreateAddress(j);
                orders.Add(order);
            }
            return orders;
        }

        private static JObject CreateOrder(int j)
        {
            JObject order = new JObject();
            order["Id"] = j;
            order["ShippingAddress"] = CreateAddress(j);
            return order;
        }

        private static JObject CreateAddress(int j)
        {
            JObject address = new JObject();
            address["FirstLine"] = "First line " + j;
            address["SecondLine"] = "Second line " + j;
            address["ZipCode"] = j;
            address["City"] = "City " + j;
            address["State"] = "State " + j;
            return address;
        }

    }

    public class UntypedCustomersController : TestODataController
    {
        private static IEdmEntityObject postedCustomer = null;
        public IEdmEntityType CustomerType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped.UntypedCustomer") as IEdmEntityType;
            }
        }

        public IEdmEntityType OrderType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped.UntypedOrder") as IEdmEntityType;
            }
        }

        public IEdmComplexType AddressType
        {
            get
            {
                return Request.GetModel().FindType("Microsoft.Test.E2E.AspNet.OData.Formatter.Untyped.UntypedAddress") as IEdmComplexType;
            }
        }

        public ITestActionResult Get()
        {
            IEdmEntityObject[] untypedCustomers = new EdmEntityObject[20];
            for (int i = 0; i < 20; i++)
            {
                dynamic untypedCustomer = new EdmEntityObject(CustomerType);
                untypedCustomer.Id = i;
                untypedCustomer.Name = string.Format("Name {0}", i);
                untypedCustomer.Orders = CreateOrders(i);
                untypedCustomer.Addresses = CreateAddresses(i);
                untypedCustomer.FavoriteNumbers = Enumerable.Range(0, i).ToArray();
                untypedCustomers[i] = untypedCustomer;
            }

            IEdmCollectionTypeReference entityCollectionType =
                new EdmCollectionTypeReference(
                    new EdmCollectionType(
                        new EdmEntityTypeReference(CustomerType, isNullable: false)));

            return Ok(new EdmEntityObjectCollection(entityCollectionType, untypedCustomers.ToList()));
        }

        public ITestActionResult Get([FromODataUri] int key)
        {
            object id;
            if (postedCustomer == null || !postedCustomer.TryGetPropertyValue("Id", out id) || key != (int)id)
            {
                return BadRequest("The key isn't the one posted to the customer");
            }

            ODataQueryContext context = new ODataQueryContext(Request.GetModel(), CustomerType, path: null);
            ODataQueryOptions query = new ODataQueryOptions(context, Request);
            if (query.SelectExpand != null)
            {
                Request.ODataContext().SelectExpandClause = query.SelectExpand.SelectExpandClause;
            }
            return Ok(postedCustomer);
        }

        public ITestActionResult Post([FromBody]IEdmEntityObject customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("customer is null");
            }
            postedCustomer = customer;
            object id;
            customer.TryGetPropertyValue("Id", out id);

            IEdmEntitySet entitySet = Request.GetModel().EntityContainer.FindEntitySet("UntypedCustomers");
            return Created(Url.CreateODataLink(new EntitySetSegment(entitySet),
                new KeySegment(new[] { new KeyValuePair<string, object>("Id", id) }, entitySet.EntityType(), null)), customer);
        }

        public ITestActionResult PrimitiveCollection()
        {
            return Ok(Enumerable.Range(1, 10));
        }

        public ITestActionResult ComplexObjectCollection()
        {
            return Ok(CreateAddresses(10));
        }

        public ITestActionResult EntityCollection()
        {
            return Ok(CreateOrders(10));
        }

        public ITestActionResult SinglePrimitive()
        {
            return Ok(10);
        }

        public ITestActionResult SingleComplexObject()
        {
            return Ok(CreateAddress(10));
        }

        public ITestActionResult SingleEntity()
        {
            return Ok(CreateOrder(10));
        }

        public ITestActionResult EnumerableOfIEdmObject()
        {
            IList<IEdmEntityObject> result = Enumerable.Range(0, 10).Select(i => (IEdmEntityObject)CreateOrder(i)).ToList();
            return Ok(result);
        }

        public ITestActionResult UntypedParameters(ODataUntypedActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("parameters is null");
            }
            object address;
            object addresses;
            object value;
            object values;
            if (!parameters.TryGetValue("address", out address) || address as IEdmComplexObject == null ||
                !parameters.TryGetValue("addresses", out addresses) || addresses as IEnumerable == null ||
                !parameters.TryGetValue("value", out value) || (int)value != 5 ||
                !parameters.TryGetValue("values", out values) || values as IEnumerable == null ||
                !(values as IEnumerable).Cast<int>().SequenceEqual(Enumerable.Range(0, 5)))
            {
                return BadRequest("Address is not present or is not a complex object");
            }
            return Ok(address as IEdmComplexObject);
        }

        private dynamic CreateAddresses(int i)
        {
            EdmComplexObject[] addresses = new EdmComplexObject[i];
            for (int j = 0; j < i; j++)
            {
                dynamic complexObject = CreateAddress(j);
                addresses[j] = complexObject;
            }
            var collection = new EdmComplexObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(AddressType, false))), addresses);
            return collection;
        }

        private dynamic CreateOrders(int i)
        {
            EdmEntityObject[] orders = new EdmEntityObject[i];
            for (int j = 0; j < i; j++)
            {
                dynamic order = new EdmEntityObject(OrderType);
                order.Id = j;
                order.ShippingAddress = CreateAddress(j);
                orders[j] = order;
            }
            var collection = new EdmEntityObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmEntityTypeReference(OrderType, false))), orders);
            return collection;
        }

        private dynamic CreateOrder(int j)
        {
            dynamic order = new EdmEntityObject(OrderType);
            order.Id = j;
            order.ShippingAddress = CreateAddress(j);
            return order;
        }

        private dynamic CreateAddress(int j)
        {
            dynamic address = new EdmComplexObject(AddressType);
            address.FirstLine = "First line " + j;
            address.SecondLine = "Second line " + j;
            address.ZipCode = j;
            address.City = "City " + j;
            address.State = "State " + j;
            return address;
        }
    }

    public class UntypedCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual IList<UntypedOrder> Orders { get; set; }
        public virtual IList<UntypedAddress> Addresses { get; set; }
        public virtual IList<int> FavoriteNumbers { get; set; }
    }

    public class UntypedOrder
    {
        public int Id { get; set; }
        public UntypedAddress ShippingAddress { get; set; }
    }

    public class UntypedAddress
    {
        public string FirstLine { get; set; }
        public string SecondLine { get; set; }
        public int ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
    }
}
