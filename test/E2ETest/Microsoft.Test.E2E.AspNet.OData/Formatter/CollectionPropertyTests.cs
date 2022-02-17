//-----------------------------------------------------------------------------
// <copyright file="CollectionPropertyTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    [EntitySet("CollectionProperty_Entity")]
    [Key("ID")]
    public class CollectionProperty_Entity
    {
        public int ID { get; set; }
        public List<string> StringList { get; set; }
        public Collection<CollectionProperty_ComplexType> ComplexTypeCollection { get; set; }
    }

    public class CollectionProperty_ComplexType
    {
        public List<string> StringList { get; set; }
        public Collection<CollectionProperty_ComplexType1> ComplexTypeCollection { get; set; }
    }
    public class CollectionProperty_ComplexType1
    {
        public List<string> StringList { get; set; }
    }

    public class CollectionProperty_EntityController : InMemoryODataController<CollectionProperty_Entity, int>
    {
        public CollectionProperty_EntityController()
            : base("ID")
        {
        }
    }

    public abstract class CollectionPropertyTests : ODataFormatterTestBase
    {
        public CollectionPropertyTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<CollectionProperty_Entity>("CollectionProperty_Entity");
            return mb.GetEdmModel();
        }

        public async Task SupportPostCollectionPropertyByEntityPayload()
        {
            var settings = new CreatorSettings() { NullValueProbability = 0.0 };
            var uri = new Uri(this.BaseAddress);
            var entitySetName = "CollectionProperty_Entity";

            // clear respository
            await this.ClearRepositoryAsync("CollectionProperty_Entity");

            var seed = RandomSeedGenerator.GetRandomSeed();
            Trace.WriteLine($"Generated seed for random number generator: {seed}");

            var rand = new Random(seed);

            // post new entity to repository
            var baseline = InstanceCreator.CreateInstanceOf<CollectionProperty_Entity>(rand, settings);
            await PostNewEntityAsync(uri, entitySetName, baseline);

            int id = baseline.ID;
            var actual = (await GetEntitiesAsync(uri, entitySetName)).Where(t => t.ID == id).First();
            AssertExtension.DeepEqual(baseline, actual);

            await UpdateEntityAsync(uri, entitySetName, actual, data =>
            {
                data.StringList = InstanceCreator.CreateInstanceOf<List<string>>(rand, settings);
                data.ComplexTypeCollection = InstanceCreator.CreateInstanceOf<Collection<CollectionProperty_ComplexType>>(rand, settings);
            });

            var afterUpdate = (await GetEntitiesAsync(uri, entitySetName)).Where(t => t.ID == id).First();
            AssertExtension.DeepEqual(actual, afterUpdate);
        }

        private async Task<DataServiceResponse> PostNewEntityAsync(Uri address, string entitySetName, CollectionProperty_Entity entity)
        {
            var context = WriterClient(address, ODataProtocolVersion.V4);
            context.AddObject(entitySetName, entity);
            return await context.SaveChangesAsync();
        }

        private async Task<IEnumerable<CollectionProperty_Entity>> GetEntitiesAsync(Uri address, string entitySetName)
        {
            var context = ReaderClient(address, ODataProtocolVersion.V4);
            var query = context.CreateQuery<CollectionProperty_Entity>(entitySetName);

            return await query.ExecuteAsync();
        }

        private async Task<DataServiceResponse> UpdateEntityAsync(Uri address, string entitySetName, CollectionProperty_Entity entity, Action<CollectionProperty_Entity> update)
        {
            var context = WriterClient(address, ODataProtocolVersion.V4);
            context.AttachTo(entitySetName, entity);
            update(entity);
            context.UpdateObject(entity);

            return await context.SaveChangesAsync();
        }
    }
}
