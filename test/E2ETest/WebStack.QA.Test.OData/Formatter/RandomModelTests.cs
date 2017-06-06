using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Builder;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.ModelBuilder;
using Xunit;

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

        public virtual DataServiceContext ReaderClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            return new Container(serviceRoot, protocolVersion);
        }

        public virtual DataServiceContext WriterClient(Uri serviceRoot, ODataProtocolVersion protocolVersion)
        {
            var ctx = new Container(serviceRoot, protocolVersion);
            return ctx;
        }

        public static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var builder = new ODataConventionModelBuilder(configuration);

            foreach (var type in Creator.EntityTypes)
            {
                var entity = builder.AddEntityType(type);
                builder.AddEntitySet(type.Name, entity);
            }

            foreach (var type in Creator.ComplexTypes.Where(type => type.IsEnum))
            {
                var entity = builder.AddEnumType(type);
            }

            return builder.GetEdmModel();
        }

        public async Task TestRandomEntityTypes<T>(string entitySetName)
        {
            // clear respository
            this.ClearRepository(entitySetName);

            // TODO: Get ride of random generator in test codes. It's bad idea to introduce random factors in functional test
            var rand = new Random(RandomSeedGenerator.GetRandomSeed());

            T entityBaseline = await PostNewEntityAsync<T>(entitySetName, rand);

            T entityBeforeUpdate = await ReadFirstEntityAsync<T>(entitySetName);
            Assert.NotNull(entityBeforeUpdate);
            AssertExtension.PrimitiveEqual(entityBaseline, entityBeforeUpdate);

            DataServiceResponse responseUpdate = await UpdateEntityAsync<T>(entitySetName, entityBeforeUpdate, rand);

            T entityAfterUpdate = await ReadFirstEntityAsync<T>(entitySetName);
            Assert.NotNull(entityAfterUpdate);
            AssertExtension.PrimitiveEqual(entityBeforeUpdate, entityAfterUpdate);

            DataServiceResponse responseDelete = await DeleteEntityAsync<T>(entitySetName, entityAfterUpdate);

            T[] entities = await ReadAllEntitiesAsync<T>(entitySetName);
            Assert.Equal(0, entities.Length);
        }

        // post new entity to repository
        private async Task<T> PostNewEntityAsync<T>(string entitySetName, Random rand)
        {
            var newEntity = (T)Creator.GenerateClientRandomData(typeof(T), rand);
            var client = WriterClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            client.AddObject(entitySetName, newEntity);

            var saveResponse = await client.SaveChangesAsync();

            return newEntity;
        }

        // get the first entity from an entity set
        private async Task<T> ReadFirstEntityAsync<T>(string entitySetName)
        {
            var client = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);

            DataServiceQuery<T> query = client.CreateQuery<T>(entitySetName);
            IEnumerable<T> results = await query.ExecuteAsync();

            return results.FirstOrDefault();
        }

        // update entity
        private async Task<DataServiceResponse> UpdateEntityAsync<T>(string entitySetName, T entity, Random rand)
        {
            var ctx = WriterClient(new Uri(BaseAddress), ODataProtocolVersion.V4);
            ctx.AttachTo(entitySetName, entity);
            UpdateNonIDProperty(entity, rand);
            ctx.UpdateObject(entity);

            return await ctx.SaveChangesAsync();
        }

        // delete entity
        private async Task<DataServiceResponse> DeleteEntityAsync<T>(string entitySetName, T entity)
        {
            var client = WriterClient(new Uri(baseAddress), ODataProtocolVersion.V4);
            client.AttachTo(entitySetName, entity);
            client.DeleteObject(entity);

            return await client.SaveChangesAsync();
        }

        // get all the entities from an entity set
        private async Task<T[]> ReadAllEntitiesAsync<T>(string entitySetName)
        {
            var client = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);

            DataServiceQuery<T> query = client.CreateQuery<T>(entitySetName);
            IEnumerable<T> results = await query.ExecuteAsync();

            return results.ToArray();
        }

        public async Task TestRandomEntityTypes(Type entityType, string entitySetName)
        {
            var genericMethod = this.GetType()
                .GetMethods()
                .Where(method => method.Name == "TestRandomEntityTypes")
                .Where(method => method.IsGenericMethod)
                .Where(method => method.GetParameters().Length == 1 && method.GetParameters()[0].Name == "entitySetName")
                .SingleOrDefault();

            var concreteMethod = genericMethod.MakeGenericMethod(entityType);

            await (Task)concreteMethod.Invoke(this, new object[] { entitySetName });
        }

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
}
