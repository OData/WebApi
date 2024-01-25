//-----------------------------------------------------------------------------
// <copyright file="DeltaQueryTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData
{
    public class DeltaQueryTests : WebHostTestBase
    {
        public DeltaQueryTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<TestCustomer>("TestCustomers");
            builder.EntitySet<TestOrder>("TestOrders");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task DeltaVerifyResult()
        {
            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/TestCustomers?$deltaToken=abc");
            get.Headers.Add("Accept", "application/json;odata.metadata=minimal");
            get.Headers.Add("OData-Version", "4.01");
            HttpResponseMessage response = await Client.SendAsync(get);
            Assert.True(response.IsSuccessStatusCode);
            dynamic results = await response.Content.ReadAsObject<JObject>();

            Assert.True(results.value.Count == 7, "There should be 7 entries in the response");

            var changeEntity = results.value[0];
            Assert.True(((JToken)changeEntity).Count() == 7, "The changed customer should have 6 properties plus type written.");
            string changeEntityType = changeEntity["@type"].Value as string;
            Assert.True(changeEntityType != null, "The changed customer should have type written");
            Assert.True(changeEntityType.Contains("#Microsoft.Test.E2E.AspNet.OData.TestCustomerWithAddress"), "The changed order should be a TestCustomerWithAddress");
            Assert.True(changeEntity.Id.Value == 1, "The ID Of changed customer should be 1.");
            Assert.True(changeEntity.OpenProperty.Value == 10, "The OpenProperty property of changed customer should be 10.");
            Assert.True(changeEntity.NullOpenProperty.Value == null, "The NullOpenProperty property of changed customer should be null.");
            Assert.True(changeEntity.Name.Value == "Name", "The Name of changed customer should be 'Name'");
            Assert.True(((JToken)changeEntity.Address).Count() == 2, "The changed entity's Address should have 2 properties written.");
            Assert.True(changeEntity.Address.State.Value == "State", "The changed customer's Address.State should be 'State'.");
            Assert.True(changeEntity.Address.ZipCode.Value == (int?)null, "The changed customer's Address.ZipCode should be null.");

            var phoneNumbers = changeEntity.PhoneNumbers;
            Assert.True(((JToken)phoneNumbers).Count() == 2, "The changed customer should have 2 phone numbers");
            Assert.True(phoneNumbers[0].Value == "123-4567", "The first phone number should be '123-4567'");
            Assert.True(phoneNumbers[1].Value == "765-4321", "The second phone number should be '765-4321'");

            var newCustomer = results.value[1];
            Assert.True(((JToken)newCustomer).Count() == 3, "The new customer should have 3 properties written");
            Assert.True(newCustomer.Id.Value == 10, "The ID of the new customer should be 10");
            Assert.True(newCustomer.Name.Value == "NewCustomer", "The name of the new customer should be 'NewCustomer'");

            var places = newCustomer.FavoritePlaces;
            Assert.True(((JToken)places).Count() == 2, "The new customer should have 2 favorite places");

            var place1 = places[0];
            Assert.True(((JToken)place1).Count() == 2, "The first favorite place should have 2 properties written.");
            Assert.True(place1.State.Value == "State", "The first favorite place's state should be 'State'.");
            Assert.True(place1.ZipCode.Value == (int?)null, "The first favorite place's Address.ZipCode should be null.");

            var place2 = places[1];
            Assert.True(((JToken)place2).Count() == 5, "The second favorite place should have 5 properties written.");
            Assert.True(place2.City.Value == "City2", "The second favorite place's Address.City should be 'City2'.");
            Assert.True(place2.State.Value == "State2", "The second favorite place's Address.State should be 'State2'.");
            Assert.True(place2.ZipCode.Value == 12345, "The second favorite place's Address.ZipCode should be 12345.");
            Assert.True(place2.OpenProperty.Value == 10, "The second favorite place's Address.OpenProperty should be 10.");
            Assert.True(place2.NullOpenProperty.Value == null, "The second favorite place's Address.NullOpenProperty should be null.");

            var newOrder = results.value[2];
            Assert.True(((JToken)newOrder).Count() == 3, "The new order should have 2 properties plus context written");
            string newOrderContext = newOrder["@context"].Value as string;
            Assert.True(newOrderContext != null, "The new order should have a context written");
            Assert.True(newOrderContext.Contains("$metadata#TestOrders"), "The new order should come from the TestOrders entity set");
            Assert.True(newOrder.Id.Value == 27, "The ID of the new order should be 27");
            Assert.True(newOrder.Amount.Value == 100, "The amount of the new order should be 100");

            var deletedEntity = results.value[3];
            Assert.True(deletedEntity["@id"].Value == "7", "The ID of the deleted customer should be 7");
            Assert.True(deletedEntity["@removed"].reason.Value == "changed", "The reason for the deleted customer should be 'changed'");

            var deletedOrder = results.value[4];
            string deletedOrderContext = deletedOrder["@context"].Value as string;
            Assert.True(deletedOrderContext != null, "The deleted order should have a context written");
            Assert.True(deletedOrderContext.Contains("$metadata#TestOrders"), "The deleted order should come from the TestOrders entity set");
            Assert.True(deletedOrder["@id"].Value == "12", "The ID of the deleted order should be 12");
            Assert.True(deletedOrder["@removed"].reason.Value == "deleted", "The reason for the deleted order should be 'deleted'");

            var deletedLink = results.value[5];
            Assert.True(deletedLink.source.Value == "http://localhost/odata/DeltaCustomers(1)", "The source of the deleted link should be 'http://localhost/odata/DeltaCustomers(1)'");
            Assert.True(deletedLink.target.Value == "http://localhost/odata/DeltaOrders(12)", "The target of the deleted link should be 'http://localhost/odata/DeltaOrders(12)'");
            Assert.True(deletedLink.relationship.Value == "Orders", "The relationship of the deleted link should be 'Orders'");

            var addedLink = results.value[6];
            Assert.True(addedLink.source.Value == "http://localhost/odata/DeltaCustomers(10)", "The source of the added link should be 'http://localhost/odata/DeltaCustomers(10)'");
            Assert.True(addedLink.target.Value == "http://localhost/odata/DeltaOrders(27)", "The target of the added link should be 'http://localhost/odata/DeltaOrders(27)'");
            Assert.True(addedLink.relationship.Value == "Orders", "The relationship of the added link should be 'Orders'");
        }

        [Fact]
        public async Task DeltaVerifyResult_ContainsDynamicComplexProperties()
        {
            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/TestOrders?$deltaToken=abc");
            get.Headers.Add("Accept", "application/json;odata.metadata=minimal");
            get.Headers.Add("OData-Version", "4.01");
            HttpResponseMessage response = await Client.SendAsync(get);
            Assert.True(response.IsSuccessStatusCode);

            string result = await response.Content.ReadAsStringAsync();
            Assert.Contains("odata/$metadata#TestOrders/$delta\"," +
            "\"value\":[" +
              "{" +
                "\"Id\":1," +
                "\"Amount\":42," +
                "\"Location\":" +
                "{" +
                  "\"State\":\"State\"," +
                  "\"ZipCode\":null," +
                  "\"OpenProperty\":10," +
                  "\"key-samplelist\":{" +
                    "\"@type\":\"#Microsoft.Test.E2E.AspNet.OData.TestAddress\"," +
                    "\"State\":\"sample state\"," +
                    "\"ZipCode\":9," +
                    "\"title\":\"sample title\"" +
                  "}" +
                "}" +
              "}" +
            "]" +
          "}",
                result);
        }

        [Fact]
        public async Task DeltaVerifyResult_ContainsDynamicComplexProperties_UsingDefaultVersion()
        {
            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/TestOrders?$deltaToken=abc");
            HttpResponseMessage response = await Client.SendAsync(get);
            Assert.True(response.IsSuccessStatusCode);

            string result = await response.Content.ReadAsStringAsync();
            Assert.Contains("odata/$metadata#TestOrders/$delta\"," +
            "\"value\":[" +
              "{" +
                "\"Id\":1," +
                "\"Amount\":42," +
                "\"Location\":" +
                "{" +
                  "\"State\":\"State\"," +
                  "\"ZipCode\":null," +
                  "\"OpenProperty\":10," +
                  "\"key-samplelist\":{" +
                    "\"@odata.type\":\"#Microsoft.Test.E2E.AspNet.OData.TestAddress\"," +
                    "\"State\":\"sample state\"," +
                    "\"ZipCode\":9," +
                    "\"title\":\"sample title\"" +
                  "}" +
                "}" +
              "}" +
            "]" +
          "}",
                result);
        }
    }

    public class TestCustomersController : TestODataController
    {
        public ITestActionResult Get()
        {
            IEdmEntityType customerType = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.TestCustomer") as IEdmEntityType;
            IEdmEntityType customerWithAddressType = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.TestCustomerWithAddress") as IEdmEntityType;
            IEdmComplexType addressType = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.TestAddress") as IEdmComplexType;
            IEdmEntityType orderType = Request.GetModel().FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.TestOrder") as IEdmEntityType;
            IEdmEntitySet ordersSet = Request.GetModel().FindDeclaredEntitySet("TestOrders") as IEdmEntitySet;
            EdmChangedObjectCollection changedObjects = new EdmChangedObjectCollection(customerType);

            EdmDeltaComplexObject a = new EdmDeltaComplexObject(addressType);
            a.TrySetPropertyValue("State", "State");
            a.TrySetPropertyValue("ZipCode", null);

            EdmDeltaEntityObject changedEntity = new EdmDeltaEntityObject(customerWithAddressType);
            changedEntity.TrySetPropertyValue("Id", 1);
            changedEntity.TrySetPropertyValue("Name", "Name");
            changedEntity.TrySetPropertyValue("Address", a);
            changedEntity.TrySetPropertyValue("PhoneNumbers", new List<String> { "123-4567", "765-4321" });
            changedEntity.TrySetPropertyValue("OpenProperty", 10);
            changedEntity.TrySetPropertyValue("NullOpenProperty", null);
            changedObjects.Add(changedEntity);

            EdmComplexObjectCollection places = new EdmComplexObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(addressType, true))));
            EdmDeltaComplexObject b = new EdmDeltaComplexObject(addressType);
            b.TrySetPropertyValue("City", "City2");
            b.TrySetPropertyValue("State", "State2");
            b.TrySetPropertyValue("ZipCode", 12345);
            b.TrySetPropertyValue("OpenProperty", 10);
            b.TrySetPropertyValue("NullOpenProperty", null);
            places.Add(a);
            places.Add(b);

            var newCustomer = new EdmDeltaEntityObject(customerType);
            newCustomer.TrySetPropertyValue("Id", 10);
            newCustomer.TrySetPropertyValue("Name", "NewCustomer");
            newCustomer.TrySetPropertyValue("FavoritePlaces", places);
            changedObjects.Add(newCustomer);

            var newOrder = new EdmDeltaEntityObject(orderType);
            newOrder.NavigationSource = ordersSet;
            newOrder.TrySetPropertyValue("Id", 27);
            newOrder.TrySetPropertyValue("Amount", 100);
            changedObjects.Add(newOrder);

            var deletedCustomer = new EdmDeltaDeletedEntityObject(customerType);
            deletedCustomer.Id = "7";
            deletedCustomer.Reason = DeltaDeletedEntryReason.Changed;
            changedObjects.Add(deletedCustomer);

            var deletedOrder = new EdmDeltaDeletedEntityObject(orderType);
            deletedOrder.NavigationSource = ordersSet;
            deletedOrder.Id = "12";
            deletedOrder.Reason = DeltaDeletedEntryReason.Deleted;
            changedObjects.Add(deletedOrder);

            var deletedLink = new EdmDeltaDeletedLink(customerType);
            deletedLink.Source = new Uri("http://localhost/odata/DeltaCustomers(1)");
            deletedLink.Target = new Uri("http://localhost/odata/DeltaOrders(12)");
            deletedLink.Relationship = "Orders";
            changedObjects.Add(deletedLink);

            var addedLink = new EdmDeltaLink(customerType);
            addedLink.Source = new Uri("http://localhost/odata/DeltaCustomers(10)");
            addedLink.Target = new Uri("http://localhost/odata/DeltaOrders(27)");
            addedLink.Relationship = "Orders";
            changedObjects.Add(addedLink);

            return Ok(changedObjects);
        }
    }

    public class TestOrdersController : TestODataController
    {
        public ITestActionResult Get()
        {
            IEdmModel model = Request.GetModel();
            IEdmComplexType addressType = model.FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.TestAddress") as IEdmComplexType;
            IEdmEntityType orderType = model.FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.TestOrder") as IEdmEntityType;
            IEdmEntitySet ordersSet = model.FindDeclaredEntitySet("TestOrders") as IEdmEntitySet;
            EdmChangedObjectCollection changedObjects = new EdmChangedObjectCollection(orderType);

            EdmDeltaComplexObject sampleList = new EdmDeltaComplexObject(addressType);
            sampleList.TrySetPropertyValue("State", "sample state");
            sampleList.TrySetPropertyValue("ZipCode", 9);
            sampleList.TrySetPropertyValue("title", "sample title"); // primitive dynamic

            EdmDeltaComplexObject location = new EdmDeltaComplexObject(addressType);
            location.TrySetPropertyValue("State", "State");
            location.TrySetPropertyValue("ZipCode", null);
            location.TrySetPropertyValue("OpenProperty", 10); // primitive dynamic
            location.TrySetPropertyValue("key-samplelist", sampleList); // complex dynamic

            EdmDeltaEntityObject changedOrder = new EdmDeltaEntityObject(orderType);
            changedOrder.TrySetPropertyValue("Id", 1);
            changedOrder.TrySetPropertyValue("Amount", 42);
            changedOrder.TrySetPropertyValue("Location", location);
            changedObjects.Add(changedOrder);

            return Ok(changedObjects);
        }
    }

    public class TestCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public virtual IList<string> PhoneNumbers { get; set; }
        public virtual IList<TestOrder> Orders { get; set; }
        public virtual IList<TestAddress> FavoritePlaces { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    public class TestCustomerWithAddress : TestCustomer
    {
        public virtual TestAddress Address { get; set; }
    }

    public class TestOrder
    {
        public int Id { get; set; }
        public int Amount { get; set; }

        public TestAddress Location { get; set; }
    }

    public class TestAddress
    {
        public string State { get; set; }
        public string City { get; set; }
        public int? ZipCode { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }
}
