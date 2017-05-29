using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.Untyped
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class UntypedSerializationTests
    {
        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.MapODataServiceRoute("untyped", "untyped", GetEdmModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
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
        public void UntypedWorksInAllFormats(string acceptHeader)
        {
            string url = "/untyped/UntypedCustomers";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
            HttpResponseMessage response = Client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
        }

        public static TheoryDataSet<string, object> UntypedWorksForAllKindsOfDataTypesPropertyData
        {
            get
            {
                TheoryDataSet<string, object> data = new TheoryDataSet<string, object>();
                data.Add("PrimitiveCollection", JToken.FromObject(new { value = Enumerable.Range(1, 10) }));
                data.Add("ComplexObjectCollection", JToken.FromObject(new { value = CreateAddresses(10) }));
                data.Add("EntityCollection", JToken.FromObject(new { value = CreateOrders(10) }));
                data.Add("SinglePrimitive", JToken.FromObject(new { value = 10 }));
                data.Add("SingleComplexObject", JToken.FromObject(CreateAddress(10)));
                data.Add("SingleEntity", JToken.FromObject(CreateOrder(10)));
                return data;
            }
        }

        [Theory]
        [PropertyData("UntypedWorksForAllKindsOfDataTypesPropertyData")]
        public void UntypedWorksForAllKindsOfDataTypes(string actionName, JToken expectedPayload)
        {
            string url = "/untyped/UntypedCustomers/Default." + actionName;
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + url);
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            JToken result = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(expectedPayload, result, JToken.EqualityComparer);
        }

        [Fact]
        public void RoundTripEntityWorks()
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
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.True(response.IsSuccessStatusCode);

            HttpRequestMessage getRequest = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}{1}({2})?$expand=Orders", BaseAddress, url, i));
            getRequest.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=none"));
            HttpResponseMessage getResponse = Client.SendAsync(getRequest).Result;
            Assert.True(getResponse.IsSuccessStatusCode);
            Assert.NotNull(getResponse.Content);
            JObject returnedObject = getResponse.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(untypedCustomer, returnedObject, JToken.EqualityComparer);
        }


        [Fact]
        public void UntypedActionParametersRoundtrip()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + "/untyped/UntypedCustomers/Default.UntypedParameters");
            object payload = new { address = CreateAddress(5), value = 5, addresses = CreateAddresses(10), values = Enumerable.Range(0, 5) };
            request.Content = new ObjectContent<object>(payload, new JsonMediaTypeFormatter());
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            var body = request.Content.ReadAsStringAsync().Result;
            HttpResponseMessage response = Client.SendAsync(request).Result;
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

    public class UntypedCustomersController : ODataController
    {
        private static IEdmEntityObject postedCustomer = null;
        public IEdmEntityType CustomerType
        {
            get
            {
                return Request.ODataProperties().Model.FindType("WebStack.QA.Test.OData.Formatter.Untyped.UntypedCustomer") as IEdmEntityType;
            }
        }

        public IEdmEntityType OrderType
        {
            get
            {
                return Request.ODataProperties().Model.FindType("WebStack.QA.Test.OData.Formatter.Untyped.UntypedOrder") as IEdmEntityType;
            }
        }

        public IEdmComplexType AddressType
        {
            get
            {
                return Request.ODataProperties().Model.FindType("WebStack.QA.Test.OData.Formatter.Untyped.UntypedAddress") as IEdmComplexType;
            }
        }

        public IHttpActionResult Get()
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

        public IHttpActionResult Get([FromODataUri] int key)
        {
            object id;
            if (postedCustomer == null || !postedCustomer.TryGetPropertyValue("Id", out id) || key != (int)id)
            {
                return BadRequest("The key isn't the one posted to the customer");
            }

            ODataQueryContext context = new ODataQueryContext(Request.ODataProperties().Model, CustomerType, path: null);
            ODataQueryOptions query = new ODataQueryOptions(context, Request);
            if (query.SelectExpand != null)
            {
                Request.ODataProperties().SelectExpandClause = query.SelectExpand.SelectExpandClause;
            }
            return Ok(postedCustomer);
        }

        public IHttpActionResult Post(IEdmEntityObject customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("customer is null");
            }
            postedCustomer = customer;
            object id;
            customer.TryGetPropertyValue("Id", out id);
            return Created(Url.CreateODataLink(new EntitySetPathSegment("UntypedCustomer"), new KeyValuePathSegment(id.ToString())), customer);
        }

        public IHttpActionResult PrimitiveCollection()
        {
            return Ok(Enumerable.Range(1, 10));
        }

        public IHttpActionResult ComplexObjectCollection()
        {
            return Ok(CreateAddresses(10));
        }

        public IHttpActionResult EntityCollection()
        {
            return Ok(CreateOrders(10));
        }

        public IHttpActionResult SinglePrimitive()
        {
            return Ok(10);
        }

        public IHttpActionResult SingleComplexObject()
        {
            return Ok(CreateAddress(10));
        }

        public IHttpActionResult SingleEntity()
        {
            return Ok(CreateOrder(10));
        }

        public IHttpActionResult EnumerableOfIEdmObject()
        {
            IList<IEdmEntityObject> result = Enumerable.Range(0, 10).Select(i => (IEdmEntityObject)CreateOrder(i)).ToList();
            return Ok(result);
        }

        public IHttpActionResult UntypedParameters(ODataUntypedActionParameters parameters)
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
