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

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    [EntitySet("ComplexTypeTests_Entity")]
    [Key("ID")]
    public class ComplexTypeTests_Entity
    {
        public int ID { get; set; }
        public ComplexTypeTests_ComplexType ComplexType { get; set; }
    }

    public class ComplexTypeTests_ComplexTypeBase
    {
        public string BaseProperty { get; set; }
    }
    public class ComplexTypeTests_ComplexType : ComplexTypeTests_ComplexTypeBase
    {
        public string ChildProperty { get; set; }
    }

    public class ComplexTypeTests_EntityController : InMemoryODataController<ComplexTypeTests_Entity, int>
    {
        public ComplexTypeTests_EntityController()
            : base("ID")
        {
        }
    }

    public abstract class ComplexTypeTests<TTest> : ODataFormatterTestBase<TTest>
    {
        public ComplexTypeTests(WebHostTestFixture<TTest> fixture)
            :base(fixture)
        {
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<ComplexTypeTests_Entity>("ComplexTypeTests_Entity");
            return mb.GetEdmModel();
        }

        public async Task ShouldSupportDerivedComplexTypeAsync()
        {
            var settings = new CreatorSettings() { NullValueProbability = 0.0 };
            var uri = new Uri(this.BaseAddress);
            var entitySetName = "ComplexTypeTests_Entity";

            // clear respository
            await this.ClearRepositoryAsync("ComplexTypeTests_Entity");

            var rand = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            var baseline = InstanceCreator.CreateInstanceOf<ComplexTypeTests_Entity>(rand, settings);
            await PostNewEntityAsync(uri, entitySetName, baseline);

            int id = baseline.ID;
            var actual = (await GetEntitiesAsync(uri, entitySetName)).Where(t => t.ID == id).First();
            AssertExtension.DeepEqual(baseline, actual);

            await UpdateEntityAsync(uri, entitySetName, actual, data =>
            {
                data.ComplexType = InstanceCreator.CreateInstanceOf<ComplexTypeTests_ComplexType>(rand, settings);
            });

            var afterUpdate = (await GetEntitiesAsync(uri, entitySetName)).Where(t => t.ID == id).First();
            AssertExtension.DeepEqual(actual, afterUpdate);
        }

        private async Task<DataServiceResponse> PostNewEntityAsync(Uri address, string entitySetName, ComplexTypeTests_Entity entity)
        {
            var context = WriterClient(address, ODataProtocolVersion.V4);
            context.AddObject(entitySetName, entity);
            return await context.SaveChangesAsync();
        }

        private async Task<IEnumerable<ComplexTypeTests_Entity>> GetEntitiesAsync(Uri address, string entitySetName)
        {
            var context = ReaderClient(address, ODataProtocolVersion.V4);
            var query = context.CreateQuery<ComplexTypeTests_Entity>(entitySetName);

            return await query.ExecuteAsync();
        }

        private async Task<DataServiceResponse> UpdateEntityAsync(Uri address, string entitySetName, ComplexTypeTests_Entity entity, Action<ComplexTypeTests_Entity> update)
        {
            var context = WriterClient(address, ODataProtocolVersion.V4);
            context.AttachTo(entitySetName, entity);
            update(entity);
            context.UpdateObject(entity);

            return await context.SaveChangesAsync();
        }
    }
}
