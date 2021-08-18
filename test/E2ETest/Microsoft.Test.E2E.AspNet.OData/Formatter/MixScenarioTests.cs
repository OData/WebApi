//-----------------------------------------------------------------------------
// <copyright file="MixScenarioTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Nop.Core.Domain.Blogs;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class MixScenarioTests_WebApiController : TestNonODataController
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

    [Key("Id")]
    public class MixVehicle
    {
        public int Id { get; set; }

        public string Model { get; set; }

        public string Name { get; set; }

        public virtual int WheelCount { get; set; }
    }

    public class MixScenarioTests_ODataController : InMemoryODataController<MixVehicle, int>
    {
        public MixScenarioTests_ODataController()
            : base("Id")
        {
        }
    }

    public class MixScenarioTestsWebApi : WebHostTestBase
    {
        public MixScenarioTestsWebApi(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.EnableODataSupport(GetEdmModel(configuration), "odata");
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<MixVehicle>("MixScenarioTests_OData");
            return mb.GetEdmModel();
        }

        [Fact]
        public async Task WebAPIShouldWorkWithPrimitiveTypeCollectionInJSON()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/MixScenarioTests_WebApi/Get");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsObject<IEnumerable<string>>();
            Assert.Equal(2, result.Count());
            Assert.Equal("value1", result.First());
        }

        [Fact]
        public async Task WebAPIShouldWorkWithHttpErrorInJSON()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/MixScenarioTests_WebApi/ThrowExceptionInAction");
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();
            Assert.Contains("Something wrong", result);
        }

        [Fact]
        public async Task WebAPIQueryableShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/MixScenarioTests_WebApi/GetQueryableData?$filter=Id gt 1");
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsObject<IEnumerable<BlogPost>>();
            Assert.Equal(2, result.Count());
        }
    }

    public abstract class MixScenarioTestsOData : ODataFormatterTestBase
    {
        public MixScenarioTestsOData(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<MixVehicle>("MixScenarioTests_OData");
            return mb.GetEdmModel();
        }

        public async Task ODataCRUDShouldWorkAsync()
        {
            // RandomSeedGenerator.GetRandomSeed is based on date
            // and these tests need something with more variance.
            var randSeed = new Random();
            var rand = new Random((int)randSeed.NextDouble());
            var entitySetName = "MixScenarioTests_OData";
            var uri = new Uri(this.BaseAddress + "/odata");
            await this.ClearRepositoryAsync(entitySetName);

            // post new entity to repository
            var baseline = InstanceCreator.CreateInstanceOf<MixVehicle>(rand);
            await PostNewEntityAsync(uri, baseline, entitySetName);

            // get collection of entities from repository
            var entities = await GetEntitiesAsync(uri, entitySetName);
            var firstVersion = entities.ToList().Where(v => v.Id == baseline.Id).FirstOrDefault();
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
            var secondVersion = entitiesAgain.ToList().Where(v => v.Id == firstVersion.Id).FirstOrDefault();
            Assert.NotNull(secondVersion);
            // firstVersion is updated in UpdatedEntityAsync
            AssertExtension.PrimitiveEqual(firstVersion, secondVersion);

            // delete entity
            await DeleteEntityAsync(uri, secondVersion, entitySetName);
            var entitiesFinal = await GetEntitiesAsync(uri, entitySetName);
            var thirdVersion = entitiesFinal.ToList().Where(v => v.Id == firstVersion.Id).FirstOrDefault();
            Assert.Null(thirdVersion);
        }

        private async Task<DataServiceResponse> PostNewEntityAsync(Uri baseAddress, MixVehicle entity, string entitySetName)
        {
            var context = WriterClient(baseAddress, ODataProtocolVersion.V4);
            context.AddObject(entitySetName, entity);

            return await context.SaveChangesAsync();
        }

        private async Task<IEnumerable<MixVehicle>> GetEntitiesAsync(Uri baseAddress, string entitySetName)
        {
            var context = ReaderClient(baseAddress, ODataProtocolVersion.V4);
            var query = context.CreateQuery<MixVehicle>(entitySetName);

            return await query.ExecuteAsync();
        }

        private async Task<DataServiceResponse> UpdateEntityAsync(Uri baseAddress, MixVehicle from, Func<MixVehicle, MixVehicle> update, string entitySetName)
        {
            var context = WriterClient(baseAddress, ODataProtocolVersion.V4);

            context.AttachTo(entitySetName, from);
            var to = update(from);
            context.UpdateObject(to);

            return await context.SaveChangesAsync();
        }

        private async Task<DataServiceResponse> DeleteEntityAsync(Uri baseAddress, MixVehicle entity, string entitySetName)
        {
            var context = WriterClient(baseAddress, ODataProtocolVersion.V4);
            context.AttachTo(entitySetName, entity);
            context.DeleteObject(entity);

            return await context.SaveChangesAsync();
        }
    }
}
