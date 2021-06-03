//-----------------------------------------------------------------------------
// <copyright file="DeltaTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
#else
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNet.OData.Test.Builder.TestModels.Recursive;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public enum DeltaTests_EnumType
    {
        One,
        Two,
        Three
    }

    [TypeDescriptionProvider(typeof(ExcludeXElementPropertyDescriptionProvider<DeltaTests_Todo>))]
    public class DeltaTests_Todo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DeltaTests_TodoItems Items { get; set; }
        public List<DeltaTests_TodoTag> Tags { get; set; }
        public DeltaTests_Estimation Estimation { get; set; }

        public bool? NullableBool { get; set; }
        public int? NullableInt { get; set; }
        public DeltaTests_EnumType Enum { get; set; }

        public Guid Guid { get; set; }
        public int Integer { get; set; }
        public string String { get; set; }
        public bool Bool { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public float Float { get; set; }
        public short Short { get; set; }
        public long Long { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public byte Byte { get; set; }
        public byte[] ByteArray { get; set; }
        public XElement XElement { get; set; }
    }

    [EntitySet("DeltaTests_Todoes")]
    [Key("ID")]
    public class DeltaTests_TodoClient
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DeltaTests_TodoItems Items { get; set; }
        public List<DeltaTests_TodoTag> Tags { get; set; }
        public DeltaTests_Estimation Estimation { get; set; }

        public bool? NullableBool { get; set; }
        public int? NullableInt { get; set; }
        public string Enum { get; set; }

        public Guid Guid { get; set; }
        public int Integer { get; set; }
        public string String { get; set; }
        public bool Bool { get; set; }
        public double Double { get; set; }
        public decimal Decimal { get; set; }
        public DateTimeOffset DateTime { get; set; }
        public float Float { get; set; }
        public short Short { get; set; }
        public long Long { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public DateTimeOffset DateTimeOffset { get; set; }
        public byte Byte { get; set; }
        public byte[] ByteArray { get; set; }
        public string XElement { get; set; }
    }

    public class DeltaTests_TodoItems
    {
        public List<string> Items { get; set; }
    }

    public class DeltaTests_TodoTag
    {
        public string Name { get; set; }
    }

    public class DeltaTests_Estimation
    {
        public DateTimeOffset? CompletedBy { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
    }

    public class DeltaTests_TodoesController : InMemoryODataController<DeltaTests_Todo, int>
    {
        public DeltaTests_TodoesController()
            : base("ID")
        {
        }
    }

    public class EmptyTypeDescriptionProvider : TypeDescriptionProvider
    {
        public EmptyTypeDescriptionProvider()
            : base()
        {
        }
    }

    public class ExcludeXElementPropertyDescriptionProvider<T> : TypeDescriptionProvider
    {
        public ExcludeXElementPropertyDescriptionProvider()
            : base(TypeDescriptor.GetProvider(typeof(T)))
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new ExcludeXElementPropertyDescriptor(base.GetTypeDescriptor(objectType, instance));
        }
    }

    public class ExcludeXElementPropertyDescriptor : CustomTypeDescriptor
    {
        public ExcludeXElementPropertyDescriptor(ICustomTypeDescriptor parent)
            : base(parent)
        {
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);
            foreach (PropertyDescriptor descriptor in base.GetProperties(attributes))
            {
                if (descriptor.PropertyType == typeof(XElement))
                {
                    continue;
                }

                descriptors.Add(descriptor);
            }

            return descriptors;
        }
    }

    public abstract class DeltaTests : ODataFormatterTestBase
    {
        public DeltaTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<DeltaTests_Todo>("DeltaTests_Todoes");
            return mb.GetEdmModel();
        }

        public async Task TestApplyPatchOnIndividualProperty()
        {
            // clear respository
            await this.ClearRepositoryAsync("DeltaTests_Todoes");

            await this.Client.GetStringAsync(this.BaseAddress + "/$metadata");

            Random r = new Random(RandomSeedGenerator.GetRandomSeed());

            var s = new CreatorSettings()
            {
                NullValueProbability = 0.0,
                MaxArrayLength = 100
            };

            // post new entity to repository
            var todo = InstanceCreator.CreateInstanceOf<DeltaTests_TodoClient>(r, s);
            todo.NullableBool = true;
            todo.NullableInt = 100000;
            todo.Enum = "One";
            todo.Estimation = new DeltaTests_Estimation()
            {
                CompletedBy = new DateTime(2012, 10, 18),
                EstimatedTime = TimeSpan.FromDays(1)
            };
            todo.XElement = @"<a><b/></a>";
            DataServiceContext ctx = WriterClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.ResolveName = ResolveName;
            ctx.ResolveType = ResolveType;
            ctx.AddObject("DeltaTests_Todoes", todo);
            await ctx.SaveChangesAsync();

            int id = todo.ID;
            //todo.ID = InstanceCreator.CreateInstanceOf<int>(r, s);
            todo.Name = InstanceCreator.CreateInstanceOf<string>(r, s);
            todo.Enum = "Two";
            todo.NullableBool = null;
            todo.Items = InstanceCreator.CreateInstanceOf<DeltaTests_TodoItems>(r, s);
            todo.Tags = InstanceCreator.CreateInstanceOf<List<DeltaTests_TodoTag>>(r, s);
            todo.Estimation.CompletedBy = new DateTime(2012, 11, 18);
            todo.NullableInt = 999999;

            todo.Bool = InstanceCreator.CreateInstanceOf<bool>(r, s);
            todo.Byte = InstanceCreator.CreateInstanceOf<Byte>(r, s);
            todo.ByteArray = InstanceCreator.CreateInstanceOf<byte[]>(r, s);
            todo.DateTime = InstanceCreator.CreateInstanceOf<DateTime>(r, s);
            todo.DateTimeOffset = InstanceCreator.CreateInstanceOf<DateTimeOffset>(r, s);
            todo.Decimal = InstanceCreator.CreateInstanceOf<Decimal>(r, s);
            todo.Double = InstanceCreator.CreateInstanceOf<Double>(r, s);
            todo.Float = InstanceCreator.CreateInstanceOf<float>(r, s);
            todo.Guid = InstanceCreator.CreateInstanceOf<Guid>(r, s);
            todo.Integer = InstanceCreator.CreateInstanceOf<Int32>(r, s);
            todo.Long = InstanceCreator.CreateInstanceOf<long>(r, s);
            todo.Short = InstanceCreator.CreateInstanceOf<short>(r, s);
            todo.String = InstanceCreator.CreateInstanceOf<string>(r, s);
            todo.TimeSpan = InstanceCreator.CreateInstanceOf<TimeSpan>(r, s);
            todo.XElement = @"<b><a/></b>";

            ctx.UpdateObject(todo);
            await ctx.SaveChangesAsync();

            ctx = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.ResolveName = ResolveName;
            ctx.ResolveType = ResolveType;

            var query = ctx.CreateQuery<DeltaTests_TodoClient>("DeltaTests_Todoes");
            var results = await query.ExecuteAsync();
            var actual = results.Where(t => t.ID == id).First();
            //Assert.Equal(id, actual.ID);
            Assert.Equal(todo.Name, actual.Name);
            Assert.Equal(todo.Estimation.CompletedBy, actual.Estimation.CompletedBy);
            Assert.Equal(todo.Estimation.EstimatedTime, actual.Estimation.EstimatedTime);
            Assert.Equal(todo.NullableBool, actual.NullableBool);
            Assert.Equal(todo.NullableInt, actual.NullableInt);

            Assert.Equal(todo.Bool, actual.Bool);
            Assert.Equal(todo.Byte, actual.Byte);
            Assert.Equal(todo.ByteArray, actual.ByteArray);
            Assert.Equal(todo.DateTime, actual.DateTime);
            Assert.Equal(todo.DateTimeOffset, actual.DateTimeOffset);
            Assert.Equal(todo.Decimal, actual.Decimal);
            AssertExtension.DoubleEqual(todo.Double, actual.Double);
            AssertExtension.SingleEqual(todo.Float, actual.Float);
            Assert.Equal(todo.Guid, actual.Guid);
            Assert.Equal(todo.Integer, actual.Integer);
            Assert.Equal(todo.Long, actual.Long);
            Assert.Equal(todo.Short, actual.Short);
            Assert.Equal(todo.String, actual.String);
            Assert.Equal(todo.TimeSpan, actual.TimeSpan);
            Assert.Equal(todo.XElement.Replace(" ", string.Empty).Replace(Environment.NewLine, string.Empty),
                actual.XElement.Replace(" ", string.Empty).Replace(Environment.NewLine, string.Empty));

            // clear repository
            await this.ClearRepositoryAsync("DeltaTests_Todoes");
        }

        public static string ResolveName(Type type)
        {
            if (type == typeof(DeltaTests_TodoClient))
            {
                return typeof(DeltaTests_Todo).FullName;
            }

            return type.FullName;
        }

        public static Type ResolveType(string name)
        {
            if (name == typeof(DeltaTests_Todo).FullName)
            {
                return typeof(DeltaTests_TodoClient);
            }

            return Type.GetType(name);
        }
    }

    public class PutDeltaOfTTests : WebHostTestBase
    {
        public PutDeltaOfTTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<DeltaCustomer>("DeltaCustomers");
            builder.EntitySet<DeltaOrder>("DeltaOrders");
            return builder.GetEdmModel();
        }

        [Fact]
        //Changing the test from shouldnt to should as it override navigation properties with bulk operations
        public async Task PutShouldOverrideNavigationProperties()
        {
            string putUri = BaseAddress + "/odata/DeltaCustomers(5)";
            ExpandoObject data = new ExpandoObject();
            HttpResponseMessage response = await Client.PutAsJsonAsync(putUri, data);
            Assert.True(response.IsSuccessStatusCode);

            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/DeltaCustomers(0)?$expand=Orders");
            response = await Client.SendAsync(get);
            Assert.True(response.IsSuccessStatusCode);
            dynamic query = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(0, query.Orders.Count);
        }
    }

    public class PatchtDeltaOfTTests : WebHostTestBase
    {
        static IEdmModel model;
        public PatchtDeltaOfTTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<DeltaCustomer>("DeltaCustomers");
            builder.EntitySet<DeltaOrder>("DeltaOrders");
            model = builder.GetEdmModel();
            return model;
        }

 
        [Fact]
        public async Task PatchShouldSupportNonSettableCollectionProperties()
        {
            var changedEntity = new EdmDeltaEntityObject(model.FindDeclaredType("Microsoft.Test.E2E.AspNet.OData.Formatter.DeltaCustomer") as IEdmEntityType);
            changedEntity.TrySetPropertyValue("Id", 1);
            changedEntity.TrySetPropertyValue("FathersAge", 3);

            HttpRequestMessage patch = new HttpRequestMessage(new HttpMethod("PATCH"), BaseAddress + "/odata/DeltaCustomers(6)");
            var data = new ExpandoObject() as IDictionary<string, object>; 

            foreach(var prop in changedEntity.GetChangedPropertyNames())
            {
                object val;
                if(changedEntity.TryGetPropertyValue(prop, out val))
                {
                    data.Add(prop, val);
                }
                
            }

            string content = JsonConvert.SerializeObject(data);
            patch.Content = new StringContent(content);
            patch.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(patch);

            Assert.True(response.IsSuccessStatusCode);

            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/DeltaCustomers(1)?$expand=Orders");
            response = await Client.SendAsync(get);
            Assert.True(response.IsSuccessStatusCode);
            dynamic query = await response.Content.ReadAsObject<JObject>();
            Assert.Equal(2, query.Addresses.Count);
            Assert.Equal(3, query.Orders.Count);
        }

        [Fact]
        public async Task PatchShouldSupportComplexDerivedTypeTransform()
        {
            HttpRequestMessage patch = new HttpRequestMessage(new HttpMethod("MERGE"), BaseAddress + "/odata/DeltaCustomers(6)");
            dynamic data = new ExpandoObject();
            data.Addresses = Enumerable.Range(10, 3).Select(i => new DeltaAddress { ZipCode = i });
             
            string content = JsonConvert.SerializeObject(data);
            content = @"{'MyAddress':{'@odata.type': 'Microsoft.Test.E2E.AspNet.OData.Formatter.PersonalAddress','Street': 'abc'}}";
            patch.Content = new StringContent(content);
            patch.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(patch);

            Assert.True(response.IsSuccessStatusCode);

            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/DeltaCustomers(6)?$expand=Orders");
            response = await Client.SendAsync(get);
            Assert.True(response.IsSuccessStatusCode);
            dynamic query = await response.Content.ReadAsObject<JObject>();
            Assert.Equal("abc", query.MyAddress.Street.ToString());            
        }

    }

    public class DeltaCustomersController : TestODataController
    {
        private static List<DeltaCustomer> customers;

        static DeltaCustomersController()
        {
            customers = new List<DeltaCustomer>();

            var customer = new DeltaCustomer("Original name",
                Enumerable.Range(0, 2).Select(i => new DeltaAddress { ZipCode = i }),
                Enumerable.Range(0, 3).Select(i => new DeltaOrder { Details = i.ToString() }));
            customer.Id = 5;
            customers.Add(customer);

            customer = new DeltaCustomer("Original name",
                Enumerable.Range(0, 2).Select(i => new DeltaAddress { ZipCode = i }),
                Enumerable.Range(0, 3).Select(i => new DeltaOrder { Details = i.ToString() }));
            customer.Id = 6;
            customer.MyAddress = new OfficeAddress { Street = "Microsot" };
            customers.Add(customer);
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public ITestActionResult Get([FromODataUri] int key)
        {
            var customer = customers.Where(c => c.Id == key).FirstOrDefault();
            if (customer != null)
            {
                return Ok(customer);
            }
            else
            {
                return BadRequest();
            }
        }
        public ITestActionResult Put([FromODataUri] int key, [FromBody] Delta<DeltaCustomer> entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var customer = customers.Where(c => c.Id == key).FirstOrDefault();
            entity.Put(customer);
            return Ok(customer);

        }

        [AcceptVerbs("PATCH", "MERGE")]
        public ITestActionResult Patch([FromODataUri] int key, Delta<DeltaCustomer> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var customer = customers.Where(c => c.Id == key).FirstOrDefault();
            patch.Patch(customer);
            return Ok(customer);
        }
    }

    public class DeltaCustomer
    {
        public DeltaCustomer()
        {
            Orders = new List<DeltaOrder>();
            Addresses = new List<DeltaAddress>();
        }

        public DeltaCustomer(string name, IEnumerable<DeltaAddress> addresses, IEnumerable<DeltaOrder> orders)
        {
            _name = name;
            _addresses = addresses.ToList();
            Orders = orders.ToList();
        }

        public int Id { get; set; }

        private string _name = null;

        public string Name
        {
            get { return _name; }
        }

        public int Age { get; private set; }

        public int FathersAge { get; set; }
        public ICollection<DeltaOrder> Orders { get; private set; }

        private ICollection<DeltaAddress> _addresses;

        public ICollection<DeltaAddress> Addresses
        {
            get { return _addresses; }
            set { _addresses = value; }
        }

        public Address MyAddress { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
    }

    public class OfficeAddress: Address
    {

    }

    public class PersonalAddress : Address
    {

    }

    public class DeltaOrder
    {
        public int Id { get; set; }
        public string Details { get; set; }
    }

    public class DeltaAddress
    {
        public int ZipCode { get; set; }
    }

}
