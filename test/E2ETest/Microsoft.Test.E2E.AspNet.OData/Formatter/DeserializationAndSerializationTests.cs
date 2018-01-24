// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    [EntitySet("UniverseEntity")]
    [Key("ID")]
    public class UniverseEntity
    {
        public UniverseEntity()
        {
            DynamicProperties = new Dictionary<string, object>();
        }
        public string ID { get; set; }
        public int IntProperty { get; set; }
        public int? NullableIntProperty { get; set; }
        public bool BoolProperty { get; set; }
        public string StringProperty { get; set; }
        public ComplexType OptionalComplexProperty { get; set; }
        public IDictionary<string, object> DynamicProperties { get; set; }
    }

    public class UniverseEntityClient : UniverseEntity
    {
        public string string1 { get; set;}
        public string string2 { get; set; }
        public string string3 { get; set; }
        public string string4 { get; set; }
        public decimal number10 { get; set; }
        public decimal number10point5 { get; set; }
        public decimal number10e25 { get; set; }
        public bool boolean_true { get; set; }
        public bool boolean_false { get; set; }
    }

    public class ComplexType
    {
        public string Name { get; set; }
    }

    public class UniverseEntityController : InMemoryODataController<UniverseEntity, string>
    {
        public UniverseEntityController()
            : base("ID")
        {
        }
    }

    public abstract class DeserializationAndSerializationTests : ODataFormatterTestBase
    {
        public DeserializationAndSerializationTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<UniverseEntity>("UniverseEntity")
                   .EntityType
                   .ComplexProperty(p => p.OptionalComplexProperty)
                   .IsOptional();

            return builder.GetEdmModel();
        }

        public async Task PostAndGetShouldReturnSameEntity(UniverseEntity entity)
        {
            var uri = new Uri(this.BaseAddress);
            const string entitySetName = "UniverseEntity";
            await this.ClearRepositoryAsync(entitySetName);

            var ctx = WriterClient(uri, ODataProtocolVersion.V4);
            ctx.AddObject(entitySetName, entity);
            await ctx.SaveChangesAsync();

            // get collection of entities from repository
            ctx = ReaderClient(uri, ODataProtocolVersion.V4);
            DataServiceQuery<UniverseEntity> query = ctx.CreateQuery<UniverseEntity>(entitySetName);
            var entities = await Task.Factory.FromAsync(query.BeginExecute(null, null), (asyncResult) =>
            {
                return query.EndExecute(asyncResult);
            });

            var beforeUpdate = entities.ToList().First();
            AssertExtension.DeepEqual(entity, beforeUpdate);

            // clear repository
            await this.ClearRepositoryAsync(entitySetName);
        }
    }
}
