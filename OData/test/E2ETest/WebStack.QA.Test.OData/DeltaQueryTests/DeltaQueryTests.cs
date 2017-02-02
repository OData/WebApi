using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;
using Microsoft.OData;

namespace WebStack.QA.Test.OData
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    public class DeltaQueryTests
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
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<TestCustomer>("TestCustomers");
            builder.EntitySet<TestOrder>("TestOrders");
            return builder.GetEdmModel();
        }

        [Fact]
        public void DeltaContainsExpectedProperties()
        {
            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/TestCustomers?$deltaToken=abc");
            HttpResponseMessage response = Client.SendAsync(get).Result;
            Assert.True(response.IsSuccessStatusCode);
            dynamic results = response.Content.ReadAsAsync<JObject>().Result;

            Assert.True(results.value.Count == 5, "There should be 5 entries in the response");

            var changeEntity = results.value[0];
            Assert.True(((JToken)changeEntity).Count() == 4, "The changed entity should have 4 properties written.");
            Assert.True(changeEntity.Id.Value == 1, "The ID Of changed entity should be 1.");
            Assert.True(changeEntity.Name.Value == "Name", "The Name of changed entity should be 'Name'");
            Assert.True(((JToken)changeEntity.Address).Count() == 2, "The changed entity's Address should have 2 properties written.");
            Assert.True(changeEntity.Address.State.Value == "State", "The changed entity's Address.State should be 'State'.");
            Assert.True(changeEntity.Address.ZipCode.Value == (int?)null, "The changed entity's Address.ZipCode should be null.");

            var phoneNumbers = changeEntity.PhoneNumbers;
            Assert.True(((JToken)phoneNumbers).Count() == 2, "The changed entity should have 2 phone numbers");
            Assert.True(phoneNumbers[0].Value == "123-4567", "The first phone number should be '123-4567'");
            Assert.True(phoneNumbers[1].Value == "765-4321", "The second phone number should be '765-4321'");

            var newEntity = results.value[1];
            Assert.True(((JToken)newEntity).Count() == 3, "The new entity should have 3 properties written");
            Assert.True(newEntity.Id.Value == 10, "The ID of the new entity should be 10");
            Assert.True(newEntity.Name.Value == "NewCustomer", "The name of the new entity should be 'NewCustomer'");

            var places = newEntity.FavoritePlaces;
            Assert.True(((JToken)places).Count() == 2, "The new entity should have 2 favorite places");

            var place1 = places[0];
            Assert.True(((JToken)place1).Count() == 2, "The first favorite place should have 2 properties written.");
            Assert.True(place1.State.Value == "State", "The first favorite place's state should be 'State'.");
            Assert.True(place1.ZipCode.Value == (int?)null, "The first favorite place's Address.ZipCode should be null.");

            var place2 = places[1];
            Assert.True(((JToken)place2).Count() == 3, "The second favorite place should have 3 properties written.");
            Assert.True(place2.City.Value == "City2", "The second favorite place's Address.City should be 'City2'.");
            Assert.True(place2.State.Value == "State2", "The second favorite place's Address.State should be 'State2'.");
            Assert.True(place2.ZipCode.Value == 12345, "The second favorite place's Address.ZipCode should be 12345.");

            var deletedEntity = results.value[2];
            Assert.True(deletedEntity.id.Value == "7", "The ID of the deleted entity should be 7");
            Assert.True(deletedEntity.reason.Value == "changed", "The reason for the deleted entity shoudl be 'changed'");

            var deletedLink = results.value[3];
            Assert.True(deletedLink.source.Value == "http://localhost/odata/DeltaCustomers(1)", "The source of the deleted link should be 'http://localhost/odata/DeltaCustomers(1)'");
            Assert.True(deletedLink.target.Value == "http://localhost/odata/DeltaOrders(1)", "The target of the deleted link should be 'http://localhost/odata/DeltaOrders(1)'");
            Assert.True(deletedLink.relationship.Value == "Orders", "The relationship of the deleted link should be 'Orders'");

            var addedLink = results.value[4];
            Assert.True(addedLink.source.Value == "http://localhost/odata/DeltaCustomers(10)", "The source of the added link should be 'http://localhost/odata/DeltaCustomers(10)'");
            Assert.True(addedLink.target.Value == "http://localhost/odata/DeltaOrders(1)", "The target of the added link should be 'http://localhost/odata/DeltaOrders(1)'");
            Assert.True(addedLink.relationship.Value == "Orders", "The relationship of the added link should be 'Orders'");
        }
    }

    public class TestCustomersController : ODataController
    {

        public IHttpActionResult Get()
        {
            IEdmEntityType entityType = Request.GetModel().FindDeclaredType("WebStack.QA.Test.OData.TestCustomer") as IEdmEntityType;
            IEdmComplexType addressType = Request.GetModel().FindDeclaredType("WebStack.QA.Test.OData.TestAddress") as IEdmComplexType;
            EdmChangedObjectCollection changedObjects = new EdmChangedObjectCollection(entityType);

            EdmDeltaComplexObject a = new EdmDeltaComplexObject(addressType);
            a.TrySetPropertyValue("State", "State");
            a.TrySetPropertyValue("ZipCode", null);

            EdmDeltaEntityObject changedEntity = new EdmDeltaEntityObject(entityType);
            changedEntity.TrySetPropertyValue("Id", 1);
            changedEntity.TrySetPropertyValue("Name", "Name");
            changedEntity.TrySetPropertyValue("Address", a);
            changedEntity.TrySetPropertyValue("PhoneNumbers", new List<String> { "123-4567", "765-4321" });
            changedObjects.Add(changedEntity);

            EdmComplexObjectCollection places = new EdmComplexObjectCollection(new EdmCollectionTypeReference(new EdmCollectionType(new EdmComplexTypeReference(addressType,true))));
            EdmDeltaComplexObject b = new EdmDeltaComplexObject(addressType);
            b.TrySetPropertyValue("City", "City2");
            b.TrySetPropertyValue("State", "State2");
            b.TrySetPropertyValue("ZipCode", 12345);
            places.Add(a);
            places.Add(b);

            var newEntity = new EdmDeltaEntityObject(entityType);
            newEntity.TrySetPropertyValue("Id", 10);
            newEntity.TrySetPropertyValue("Name", "NewCustomer");
            newEntity.TrySetPropertyValue("FavoritePlaces", places);
            changedObjects.Add(newEntity);

            var deletedEntity = new EdmDeltaDeletedEntityObject(entityType);
            deletedEntity.Id = "7";
            deletedEntity.Reason = DeltaDeletedEntryReason.Changed;
            changedObjects.Add(deletedEntity);

            var deletedLink = new EdmDeltaDeletedLink(entityType);
            deletedLink.Source = new Uri("http://localhost/odata/DeltaCustomers(1)");
            deletedLink.Target = new Uri("http://localhost/odata/DeltaOrders(1)");
            deletedLink.Relationship = "Orders";
            changedObjects.Add(deletedLink);

            var addedLink = new EdmDeltaLink(entityType);
            addedLink.Source = new Uri("http://localhost/odata/DeltaCustomers(10)");
            addedLink.Target = new Uri("http://localhost/odata/DeltaOrders(1)");
            addedLink.Relationship = "Orders";
            changedObjects.Add(addedLink);

            return Ok(changedObjects);
        }
    }

    public class TestCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public virtual IList<string> PhoneNumbers {get; set;}
        public virtual IList<TestOrder> Orders { get; set; }
        public virtual TestAddress Address { get; set; }
        public virtual IList<TestAddress> FavoritePlaces { get; set; }
    }

    public class TestOrder
    {
        public int Id { get; set; }
        public int Amount { get; set; }
    }

    public class TestAddress
    {
        public string State { get; set; }
        public string City { get; set; }
        public int? ZipCode { get; set; }
    }
}
