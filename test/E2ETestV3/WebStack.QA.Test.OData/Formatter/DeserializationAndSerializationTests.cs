using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter
{
    [EntitySet("UniverseEntity")]
    [DataServiceKey("ID")]
    public class UniverseEntity
    {
        public string ID { get; set; }
        public int IntProperty { get; set; }
        public int? NullableIntProperty { get; set; }
        public bool BoolProperty { get; set; }
        public string StringProperty { get; set; }
        public ComplexType OptionalComplexProperty { get; set; }
    }

    public class ComplexType
    {
        public string Name { get; set; }
    }

    public class UniverseEntityController : InMemoryEntitySetController<UniverseEntity, string>
    {
        public UniverseEntityController()
            : base("ID")
        {
        }
    }

    public class DeserializationAndSerializationTests : ODataFormatterTestBase
    {
        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            var entity = mb.EntitySet<UniverseEntity>("UniverseEntity").EntityType;
            entity.ComplexProperty(p => p.OptionalComplexProperty).IsOptional();
            return mb.GetEdmModel();
        }

        public void PostAndGetShouldReturnSameEntity(UniverseEntity entity)
        {
            var entitySetName = "UniverseEntity";
            // clear respository
            this.ClearRepository(entitySetName);

            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject(entitySetName, entity);
            ctx.SaveChanges();

            // get collection of entities from repository
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            IEnumerable<UniverseEntity> entities = ctx.CreateQuery<UniverseEntity>(entitySetName);
            var beforeUpdate = entities.ToList().First();
            AssertExtension.DeepEqual(entity, beforeUpdate);

            // clear repository
            this.ClearRepository(entitySetName);
        }
    }
}
