using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;
using Xunit;

namespace WebStack.QA.Test.OData.ODataPathHandler
{
    [EntitySet("UnicodeRouteTests_Todoü")]
    [DataServiceKey("ID")]
    public class UnicodeRouteTests_Todoü
    {
        public int ID { get; set; }
        public string Nameü { get; set; }
    }

    public class UnicodeRouteTests_TodoüController : InMemoryEntitySetController<UnicodeRouteTests_Todoü, int>
    {
        public UnicodeRouteTests_TodoüController()
            : base("ID")
        { 
        }

        public string GetNameü(int key)
        {
            return this.LocalTable[key].Nameü;
        }
    }

    public class UnicodeRouteTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            //configuration.Formatters.Clear();
            configuration.EnableODataSupport(GetEdmModel(configuration), "odataü");
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<UnicodeRouteTests_Todoü>("UnicodeRouteTests_Todoü");
            return mb.GetEdmModel();
        }

        [Fact]
        public void CRUDEntitySetShouldWork()
        {
            Random r = new Random(RandomSeedGenerator.GetRandomSeed());
            var entitySetName = "UnicodeRouteTests_Todoü";
            var uri = new Uri(this.BaseAddress + "/odataü");
            // post new entity to repository
            var value = InstanceCreator.CreateInstanceOf<UnicodeRouteTests_Todoü>(r);
            var ctx = new DataServiceContext(uri, DataServiceProtocolVersion.V3);
            ctx.AddObject(entitySetName, value);
            ctx.SaveChanges();

            // get collection of entities from repository
            ctx = new DataServiceContext(uri, DataServiceProtocolVersion.V3);
            IEnumerable<UnicodeRouteTests_Todoü> entities = ctx.CreateQuery<UnicodeRouteTests_Todoü>(entitySetName);
            var beforeUpdate = entities.ToList().First();
            AssertExtension.PrimitiveEqual(value, beforeUpdate);

            // update entity and verify if it's saved
            ctx = new DataServiceContext(uri, DataServiceProtocolVersion.V3);
            ctx.AttachTo(entitySetName, beforeUpdate);
            beforeUpdate.Nameü = InstanceCreator.CreateInstanceOf<string>(r);

            ctx.UpdateObject(beforeUpdate);
            ctx.SaveChanges();
            ctx = new DataServiceContext(uri, DataServiceProtocolVersion.V3);
            entities = ctx.CreateQuery<UnicodeRouteTests_Todoü>(entitySetName);
            var afterUpdate = entities.ToList().First();
            AssertExtension.PrimitiveEqual(beforeUpdate, afterUpdate);
            //var afterUpdate = entities.Where(FilterByPk(entityType, GetIDValue(beforeUpdate))).First();

            var response = ctx.LoadProperty(afterUpdate, "Nameü");
            Assert.Equal(200, response.StatusCode);

            // delete entity
            ctx = new DataServiceContext(uri, DataServiceProtocolVersion.V3);
            ctx.AttachTo(entitySetName, afterUpdate);
            ctx.DeleteObject(afterUpdate);
            ctx.SaveChanges();
            ctx = new DataServiceContext(uri, DataServiceProtocolVersion.V3);
            entities = ctx.CreateQuery<UnicodeRouteTests_Todoü>(entitySetName);
            Assert.Equal(0, entities.ToList().Count());
        }
    }
}
