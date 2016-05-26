using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
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

    public class MixScenarioTests_ODataController : InMemoryODataController<Vehicle, int>
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
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
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

        public async Task ODataCRUDShouldWork()
        {
            var rand = new Random(RandomSeedGenerator.GetRandomSeed());
            var entitySetName = "MixScenarioTests_OData";
            var uri = new Uri(this.BaseAddress + "/odata");
            this.ClearRepository(entitySetName);

            // post new entity to repository
            var baseline = InstanceCreator.CreateInstanceOf<Vehicle>(rand);
            await PostNewEntityAsync(uri, baseline, entitySetName);

            // get collection of entities from repository
            var entities = await GetEntitiesAsync(uri, entitySetName);
            var firstVersion = entities.ToList().FirstOrDefault();
            Assert.NotNull(firstVersion);
            AssertExtension.PrimitiveEqual(baseline, firstVersion);

            // update entity and verify if it's saved
            await UpdateEntityAsync(
                uri,
                firstVersion,
                data =>
                {
                    data.Model = InstanceCreator.CreateInstanceOf<string>(rand);
                    data.Name = InstanceCreator.CreateInstanceOf<string>(rand);
                    data.WheelCount = InstanceCreator.CreateInstanceOf<int>(rand);
                    return data;
                },
                entitySetName);

            var entitiesAgain = await GetEntitiesAsync(uri, entitySetName);
            var secondVersion = entitiesAgain.ToList().FirstOrDefault();
            Assert.NotNull(secondVersion);
            // firstVersion is updated in UpdatedEntityAsync
            AssertExtension.PrimitiveEqual(firstVersion, secondVersion);

            // delete entity
            await DeleteEntityAsync(uri, secondVersion, entitySetName);
            var entitiesFinal = await GetEntitiesAsync(uri, entitySetName);
            Assert.Equal(0, entitiesFinal.ToList().Count());
        }

        private async Task<DataServiceResponse> PostNewEntityAsync(Uri baseAddress, Vehicle entity, string entitySetName)
        {
            var context = WriterClient(baseAddress, ODataProtocolVersion.V4);
            context.AddObject(entitySetName, entity);

            return await context.SaveChangesAsync();
        }

        private async Task<IEnumerable<Vehicle>> GetEntitiesAsync(Uri baseAddress, string entitySetName)
        {
            var context = ReaderClient(baseAddress, ODataProtocolVersion.V4);
            var query = context.CreateQuery<Vehicle>(entitySetName);

            return await query.ExecuteAsync();
        }

        private async Task<DataServiceResponse> UpdateEntityAsync(Uri baseAddress, Vehicle from, Func<Vehicle, Vehicle> update, string entitySetName)
        {
            var context = WriterClient(baseAddress, ODataProtocolVersion.V4);

            context.AttachTo(entitySetName, from);
            var to = update(from);
            context.UpdateObject(to);

            return await context.SaveChangesAsync();
        }

        private async Task<DataServiceResponse> DeleteEntityAsync(Uri baseAddress, Vehicle entity, string entitySetName)
        {
            var context = WriterClient(baseAddress, ODataProtocolVersion.V4);
            context.AttachTo(entitySetName, entity);
            context.DeleteObject(entity);

            return await context.SaveChangesAsync();
        }
    }
}
