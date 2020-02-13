// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ODataPathHandler
{
    [EntitySet("UnicodeRouteTests_Todoü")]
    [Key("ID")]
    public class UnicodeRouteTests_Todoü
    {
        public int ID { get; set; }
        public string Nameü { get; set; }
    }

    public class UnicodeRouteTests_TodoüController : InMemoryODataController<UnicodeRouteTests_Todoü, int>
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

    public class UnicodeRouteTests : WebHostTestBase<UnicodeRouteTests>
    {
        private static IEdmModel Model;

        public UnicodeRouteTests(WebHostTestFixture<UnicodeRouteTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            Model = GetEdmModel(configuration);
            configuration.EnableODataSupport(Model, "odataü");
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<UnicodeRouteTests_Todoü>("UnicodeRouteTests_Todoü");

            return mb.GetEdmModel();
        }

#if NETFX
        /// <remarks>
        /// This test fails on AspNetCore due to Kestrel not allowing non-ASCII characters in headers.
        /// See: https://github.com/aspnet/KestrelHttpServer/issues/1144
        /// </remarks>
        [Fact]
        public async Task CRUDEntitySetShouldWork()
        {
            var rand = new Random(RandomSeedGenerator.GetRandomSeed());
            var entitySetName = "UnicodeRouteTests_Todoü";
            var uri = new Uri(this.BaseAddress + "/odataü");
            var context = new DataServiceContext(uri, ODataProtocolVersion.V4);

            // post new entity to repository
            CreatorSettings creatorSettings = new CreatorSettings()
            {
                NullValueProbability = 0,
            };
            var baseline = InstanceCreator.CreateInstanceOf<UnicodeRouteTests_Todoü>(rand, creatorSettings);
            await PostNewEntityAsync(uri, entitySetName, baseline);

            // get collection of entities from repository
            var firstVersion = await GetFirstEntityAsync(uri, entitySetName);
            Assert.NotNull(firstVersion);
            AssertExtension.PrimitiveEqual(baseline, firstVersion);

            // update entity and verify if it's saved
            await UpdateEntityAsync(uri, entitySetName, firstVersion, data =>
            {
                data.Nameü = InstanceCreator.CreateInstanceOf<string>(rand);
            });

            var secondVersion = await GetFirstEntityAsync(uri, entitySetName);
            Assert.NotNull(secondVersion);
            AssertExtension.PrimitiveEqual(firstVersion, secondVersion);

            var response = await LoadPropertyAsync(uri, entitySetName, secondVersion, "Nameü");
            Assert.Equal(200, response.StatusCode);

            // delete entity
            await DeleteEntityAsync(uri, entitySetName, secondVersion);
            var entities = await GetEntitiesAsync(uri, entitySetName);
            Assert.Empty(entities.ToList());
        }
#endif
        private DataServiceContext CreateClient(Uri address)
        {
            var client = new DataServiceContext(address, ODataProtocolVersion.V4);
            client.Format.UseJson(Model);

            return client;
        }

        private async Task<DataServiceResponse> PostNewEntityAsync(Uri address, string entitySetName, UnicodeRouteTests_Todoü entity)
        {
            var context = CreateClient(address);
            context.AddObject(entitySetName, entity);
            return await context.SaveChangesAsync();
        }

        private async Task<IEnumerable<UnicodeRouteTests_Todoü>> GetEntitiesAsync(Uri address, string entitySetName)
        {
            var context = CreateClient(address);
            var query = context.CreateQuery<UnicodeRouteTests_Todoü>(entitySetName);

            return await query.ExecuteAsync();
        }

        private async Task<UnicodeRouteTests_Todoü> GetFirstEntityAsync(Uri address, string entitySetName)
        {
            var entities = await GetEntitiesAsync(address, entitySetName);

            return entities.FirstOrDefault();
        }

        private async Task<DataServiceResponse> UpdateEntityAsync(Uri address, string entitySetName, UnicodeRouteTests_Todoü entity, Action<UnicodeRouteTests_Todoü> update)
        {
            var context = CreateClient(address);
            context.AttachTo(entitySetName, entity);
            update(entity);
            context.UpdateObject(entity);

            return await context.SaveChangesAsync();
        }

        private async Task<QueryOperationResponse> LoadPropertyAsync(Uri address, string entitySetName, UnicodeRouteTests_Todoü entity, string propertyName)
        {
            var context = CreateClient(address);
            context.AttachTo(entitySetName, entity);

            return await context.LoadPropertyAsync(entity, propertyName);
        }

        private async Task<DataServiceResponse> DeleteEntityAsync(Uri address, string entitySetName, UnicodeRouteTests_Todoü entity)
        {
            var context = CreateClient(address);
            context.AttachTo(entitySetName, entity);
            context.DeleteObject(entity);

            return await context.SaveChangesAsync();
        }
    }
}
