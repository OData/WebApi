using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Nop.Core.Domain.Blogs;
using Nuwa;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.Models.Vehicle;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
{
    public class MixScenarioTests_WebApiController : ApiController
    {
        public IEnumerable<string> Get()
        {
            return new string[] 
            {
                "value1",
                "value2"
            };
        }

        public IQueryable<BlogPost> GetQueryableData()
        { 
            return new BlogPost[] 
            {
                new BlogPost
                {
                    Id = 1,
                    Title = "aaa"
                },
                new BlogPost
                {
                    Id = 2,
                    Title = "bbb"
                },
                new BlogPost
                {
                    Id = 3,
                    Title = "ccc"
                }
            }.AsQueryable();
        }

        [HttpGet]
        public void ThrowExceptionInAction()
        {
            throw new Exception("Something wrong");
        }
    }

    public class MixScenarioTests_ODataController : InMemoryEntitySetController<Vehicle, int>
    {
        public MixScenarioTests_ODataController()
            : base("Id")
        { 
        }
    }

    public class MixScenarioTestsWebApi : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.EnableODataSupport(GetEdmModel(configuration), "odata");
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<Vehicle>("MixScenarioTests_OData");
            return mb.GetEdmModel();
        }

        [Fact]
        public void WebAPIShouldWorkWithPrimitiveTypeCollectionInJSON()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/MixScenarioTests_WebApi/Get").Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsAsync<IEnumerable<string>>().Result;
            Assert.Equal(2, result.Count());
            Assert.Equal("value1", result.First());
        }

        [Fact]
        public void WebAPIShouldWorkWithHttpErrorInJSON()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/MixScenarioTests_WebApi/ThrowExceptionInAction").Result;
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var result = response.Content.ReadAsAsync<HttpError>().Result;
            Assert.Contains("Something wrong", result["ExceptionMessage"].ToString());
        }

        [Fact]
        public void WebAPIQueryableShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/MixScenarioTests_WebApi/GetQueryableData?$filter=Id gt 1").Result;
            response.EnsureSuccessStatusCode();
            var result = response.Content.ReadAsAsync<IEnumerable<BlogPost>>().Result;
            Assert.Equal(2, result.Count());
        }
    }

    public class MixScenarioTestsOData : ODataFormatterTestBase
    {
        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<Vehicle>("MixScenarioTests_OData");
            return mb.GetEdmModel();
        }

        public void ODataCRUDShouldWork()
        {
            Random r = new Random(RandomSeedGenerator.GetRandomSeed());
            var entitySetName = "MixScenarioTests_OData";
            var uri = new Uri(this.BaseAddress + "/odata");
            this.ClearRepository(entitySetName);
            // post new entity to repository
            var value = InstanceCreator.CreateInstanceOf<Vehicle>(r);
            var ctx = WriterClient(uri, DataServiceProtocolVersion.V3);
            ctx.AddObject(entitySetName, value);
            ctx.SaveChanges();

            // get collection of entities from repository
            ctx = ReaderClient(uri, DataServiceProtocolVersion.V3);
            IEnumerable<Vehicle> entities = ctx.CreateQuery<Vehicle>(entitySetName);
            var beforeUpdate = entities.ToList().First();
            AssertExtension.PrimitiveEqual(value, beforeUpdate);

            // update entity and verify if it's saved
            ctx = WriterClient(uri, DataServiceProtocolVersion.V3);
            ctx.AttachTo(entitySetName, beforeUpdate);
            beforeUpdate.Model = InstanceCreator.CreateInstanceOf<string>(r);
            beforeUpdate.Name = InstanceCreator.CreateInstanceOf<string>(r);
            beforeUpdate.WheelCount = InstanceCreator.CreateInstanceOf<int>(r);

            ctx.UpdateObject(beforeUpdate);
            ctx.SaveChanges();
            ctx = ReaderClient(uri, DataServiceProtocolVersion.V3);
            entities = ctx.CreateQuery<Vehicle>(entitySetName);
            var afterUpdate = entities.ToList().First();
            AssertExtension.PrimitiveEqual(beforeUpdate, afterUpdate);
            //var afterUpdate = entities.Where(FilterByPk(entityType, GetIDValue(beforeUpdate))).First();

            // delete entity
            ctx = WriterClient(uri, DataServiceProtocolVersion.V3);
            ctx.AttachTo(entitySetName, afterUpdate);
            ctx.DeleteObject(afterUpdate);
            ctx.SaveChanges();
            ctx = ReaderClient(uri, DataServiceProtocolVersion.V3);
            entities = ctx.CreateQuery<Vehicle>(entitySetName);
            Assert.Equal(0, entities.ToList().Count());
        }
    }
}
