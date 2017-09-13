// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;

namespace WebStack.QA.Test.OData.Formatter
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

    public class ComplexTypeTests : ODataFormatterTestBase
    {
        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<ComplexTypeTests_Entity>("ComplexTypeTests_Entity");
            return mb.GetEdmModel();
        }

        public async Task ShouldSupportDerivedComplexType()
        {
            var settings = new CreatorSettings() { NullValueProbability = 0.0 };
            var uri = new Uri(this.BaseAddress);
            var entitySetName = "ComplexTypeTests_Entity";

            // clear respository
            this.ClearRepository("ComplexTypeTests_Entity");

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
