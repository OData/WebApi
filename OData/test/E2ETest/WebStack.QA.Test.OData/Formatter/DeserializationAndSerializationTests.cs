using System;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using System.Collections.Generic;

namespace WebStack.QA.Test.OData.Formatter
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

    public class DeserializationAndSerializationTests : ODataFormatterTestBase
    {
        protected static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<UniverseEntity>("UniverseEntity")
                   .EntityType
                   .ComplexProperty(p => p.OptionalComplexProperty)
                   .IsOptional();

            return builder.GetEdmModel();
        }

        public void PostAndGetShouldReturnSameEntity(UniverseEntity entity)
        {
            var uri = new Uri(this.BaseAddress);
            const string entitySetName = "UniverseEntity";
            this.ClearRepository(entitySetName);

            var ctx = WriterClient(uri, ODataProtocolVersion.V4);
            ctx.AddObject(entitySetName, entity);
            ctx.SaveChangesAsync().Wait();

            // get collection of entities from repository
            ctx = ReaderClient(uri, ODataProtocolVersion.V4);
            DataServiceQuery<UniverseEntity> query = ctx.CreateQuery<UniverseEntity>(entitySetName);
            IAsyncResult asyncResult = query.BeginExecute(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();

            var entities = query.EndExecute(asyncResult);

            var beforeUpdate = entities.ToList().First();
            AssertExtension.DeepEqual(entity, beforeUpdate);

            // clear repository
            this.ClearRepository(entitySetName);
        }
    }
}
