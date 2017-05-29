using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.SelfHost;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.TypeCreator;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter
{
    [NuwaFramework]
    [NwHost(HostType.WcfSelf)]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    [NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class RandomModelTests : IODataTestBase, IODataFormatterTestBase
    {
        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        private static ODataModelTypeCreator creator = null;
        public static ODataModelTypeCreator Creator
        {
            get
            {
                if (creator == null)
                {
                    creator = new ODataModelTypeCreator();
                    creator.CreateTypes(50, new Random(RandomSeedGenerator.GetRandomSeed()));
                }
                return creator;
            }
        }

        public virtual DataServiceContext ReaderClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            return new Container(serviceRoot, protocolVersion);
        }

        public virtual DataServiceContext WriterClient(Uri serviceRoot, DataServiceProtocolVersion protocolVersion)
        {
            var ctx = new Container(serviceRoot, protocolVersion);
            return ctx;
        }

        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder(configuration);

            foreach (var type in Creator.EntityTypes)
            {
                var entity = builder.AddEntity(type);
                builder.AddEntitySet(type.Name, entity);
            }

            return builder.GetEdmModel();
        }

        public void TestRandomEntityTypes(Type entityType, string entitySetName)
        {
            this.BaseAddress = this.BaseAddress.Replace("localhost", Environment.MachineName);
            //var entitySetName = entityType.Name;
            // clear respository
            this.ClearRepository(entitySetName);

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            // post new entity to repository
            var value = Creator.GenerateClientRandomData(entityType, r);

            var ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject(entitySetName, value);
            ctx.SaveChanges();

            // get collection of entities from repository
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            IEnumerable<object> entities = ctx.CreateQuery(entityType, entitySetName);
            var beforeUpdate = entities.ToList().First();
            AssertExtension.PrimitiveEqual(value, beforeUpdate);

            // update entity and verify if it's saved
            ctx = WriterClient(new Uri(BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AttachTo(entitySetName, beforeUpdate);
            var updatedProperty = UpdateNonIDProperty(beforeUpdate, r);
            ctx.UpdateObject(beforeUpdate);
            ctx.SaveChanges();
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            entities = ctx.CreateQuery(entityType, entitySetName);
            var afterUpdate = entities.ToList().First();
            AssertExtension.PrimitiveEqual(beforeUpdate, afterUpdate);            
            //var afterUpdate = entities.Where(FilterByPk(entityType, GetIDValue(beforeUpdate))).First();

            // delete entity
            ctx = WriterClient(new Uri(BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AttachTo(entitySetName, afterUpdate);
            ctx.DeleteObject(afterUpdate);
            ctx.SaveChanges();
            ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            entities = ctx.CreateQuery(entityType, entitySetName);
            Assert.Equal(0, entities.ToList().Count());

            // clear repository
            this.ClearRepository(entitySetName);
        }

        //public void TestRandomEntityEntry(Type entityType, string entitySetName)
        //{
        //    this.BaseAddress = this.BaseAddress.Replace("localhost", Environment.MachineName);
        //    //var entitySetName = entityType.Name;
        //    // clear respository
        //    this.ClearRepository(entitySetName);

        //    Random r = new Random(RandomSeedGenerator.GetRandomSeed());

        //    // post new entity to repository
        //    var value = Creator.GenerateClientRandomData(entityType, r);

        //    var ctx = WriterClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
        //    ctx.AddObject(entitySetName, value);
        //    ctx.SaveChanges();

        //    // get collection of entities from repository
        //    ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
        //    IEnumerable<object> entities = ctx.CreateQuery(entityType, entitySetName);
        //    var beforeUpdate = entities.ToList().First();
        //    AssertExtension.PrimitiveEqual(value, beforeUpdate);

        //    // update entity and verify if it's saved
        //    ctx = WriterClient(new Uri(BaseAddress), DataServiceProtocolVersion.V3);
        //    ctx.AttachTo(entitySetName, beforeUpdate);
        //    var updatedProperty = UpdateNonIDProperty(beforeUpdate, r);
        //    ctx.UpdateObject(beforeUpdate);
        //    ctx.SaveChanges();
        //    ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
        //    entities = ctx.CreateQuery(entityType, entitySetName);
        //    var afterUpdate = entities.ToList().First();
        //    AssertExtension.PrimitiveEqual(beforeUpdate, afterUpdate);
        //    //var afterUpdate = entities.Where(FilterByPk(entityType, GetIDValue(beforeUpdate))).First();

        //    // delete entity
        //    ctx = WriterClient(new Uri(BaseAddress), DataServiceProtocolVersion.V3);
        //    ctx.AttachTo(entitySetName, afterUpdate);
        //    ctx.DeleteObject(afterUpdate);
        //    ctx.SaveChanges();
        //    ctx = ReaderClient(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
        //    entities = ctx.CreateQuery(entityType, entitySetName);
        //    Assert.Equal(0, entities.ToList().Count());
        //    Expression<Func<int,int>> exp = e => e+1;
        //    // clear repository
        //    this.ClearRepository(entitySetName);
        //}

        //[Theory(Skip = "Adhoc Test")]
        //[PropertyData("EntityTypes")]
        //public void TestRandomEntityTypesByMulitpleThreads(Type entityType, string entitySetName)
        //{
        //    Parallel.For(0, 2, (i) =>
        //        {
        //            TestRandomEntityTypes(entityType, entitySetName);
        //        });
        //}

        private object GetIDValue(object o)
        {
            var idProperty = o.GetType().GetProperty("ID");
            return idProperty.GetValue(o, null);
        }

        private PropertyInfo UpdateNonIDProperty(object o, Random rndGen)
        {
            var properties = 
                o.GetType().GetProperties().Where(p => 
                    !p.Name.Equals("ID", StringComparison.OrdinalIgnoreCase)
                    && p.PropertyType.IsPrimitive);
            if (!properties.Any())
            {
                return null;
            }

            var newObj = Creator.GenerateClientRandomData(o.GetType(), rndGen);
            var property = properties.Skip(rndGen.Next(properties.Count())).First();
            property.SetValue(o, property.GetValue(newObj, null), null);
            return property;
        }
    }

    public static class ContainerExtension
    {
        public static IEnumerable<object> CreateQuery(this DataServiceContext ctx, Type type, string entitySetName)
        {
            var createQueryGenericMethod = typeof(Container).GetMethod(
                "CreateQuery",
                new Type[] 
                {
                    typeof(string)
                });
            createQueryGenericMethod = createQueryGenericMethod.MakeGenericMethod(type);
            return createQueryGenericMethod.Invoke(ctx, new object[] { entitySetName }) as IEnumerable<object>;
        }
    }
}
