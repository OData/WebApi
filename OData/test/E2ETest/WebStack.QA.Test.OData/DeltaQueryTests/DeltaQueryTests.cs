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

            Assert.True(results.value.Count == 5);

            var changeEntity = results.value[0];
            Assert.Equal(3, ((JToken)changeEntity).Count());
            Assert.Equal(1, changeEntity.Id.Value);
            Assert.Equal("Name", changeEntity.Name.Value);
            Assert.Equal(2, ((JToken)changeEntity.Address).Count());
            Assert.Equal("State", changeEntity.Address.State.Value);
            Assert.True(changeEntity.Address.ZipCode.Value == (int?)null);

            var newEntity = results.value[1];
            Assert.Equal(10, newEntity.Id.Value);
            Assert.Equal("NewCustomer", newEntity.Name.Value);
            Assert.Equal(2, ((JToken)newEntity).Count());

            var deletedEntity = results.value[2];
            Assert.Equal("7", deletedEntity.id.Value);
            Assert.Equal("changed", deletedEntity.reason.Value);

            var deletedLink = results.value[3];
            Assert.Equal("http://localhost/odata/DeltaCustomers(1)", deletedLink.source.Value);
            Assert.Equal("http://localhost/odata/DeltaOrders(1)", deletedLink.target.Value);
            Assert.Equal("Orders", deletedLink.relationship.Value);

            var addedLink = results.value[4];
            Assert.Equal("http://localhost/odata/DeltaCustomers(10)", addedLink.source.Value);
            Assert.Equal("http://localhost/odata/DeltaOrders(1)", addedLink.target.Value);
            Assert.Equal("Orders", addedLink.relationship.Value);
        }
    }

    public class TestCustomersController : ODataController
    {

        public IHttpActionResult Get()
        {
            List<IEdmChangedObject> changedObjects = new List<IEdmChangedObject>();
            IEdmEntityType entityType = Request.GetModel().FindDeclaredType("WebStack.QA.Test.OData.TestCustomer") as IEdmEntityType;
            IEdmComplexType addressType = Request.GetModel().FindDeclaredType("WebStack.QA.Test.OData.TestAddress") as IEdmComplexType;

            EdmDeltaComplexObject a = new EdmDeltaComplexObject(addressType);
            a.TrySetPropertyValue("State", "State");
            a.TrySetPropertyValue("ZipCode", null);

            EdmDeltaEntityObject changedEntity = new EdmDeltaEntityObject(entityType);
            changedEntity.TrySetPropertyValue("Id", 1);
            changedEntity.TrySetPropertyValue("Name", "Name");
            changedEntity.TrySetPropertyValue("Address", a);
            changedObjects.Add(changedEntity);

            var newEntity = new EdmDeltaEntityObject(entityType);
            newEntity.TrySetPropertyValue("Id", 10);
            newEntity.TrySetPropertyValue("Name", "NewCustomer");
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

            return Ok(new EdmChangedObjectCollection(entityType, changedObjects));
        }
    }

    public class TestCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public virtual IList<TestOrder> Orders { get; set; }
        public virtual TestAddress Address { get; set; }
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
