﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;

namespace WebStack.QA.Test.OData.ODataPathHandler
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

    public class UnicodeRouteTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.EnableODataSupport(GetEdmModel(), "odataü");
        }

        protected static IEdmModel GetEdmModel()
        {
            var mb = new ODataConventionModelBuilder();
            mb.EntitySet<UnicodeRouteTests_Todoü>("UnicodeRouteTests_Todoü");

            return mb.GetEdmModel();
        }

        [Fact]
        public async Task CRUDEntitySetShouldWork()
        {
            var rand = new Random(RandomSeedGenerator.GetRandomSeed());
            var entitySetName = "UnicodeRouteTests_Todoü";
            var uri = new Uri(this.BaseAddress + "/odataü");
            var context = new DataServiceContext(uri, ODataProtocolVersion.V4);

            // post new entity to repository
            var baseline = InstanceCreator.CreateInstanceOf<UnicodeRouteTests_Todoü>(rand);
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
            Assert.Equal(0, entities.ToList().Count());
        }

        private DataServiceContext CreateClient(Uri address)
        {
            var client = new DataServiceContext(address, ODataProtocolVersion.V4);
            client.Format.UseJson(GetEdmModel());

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
