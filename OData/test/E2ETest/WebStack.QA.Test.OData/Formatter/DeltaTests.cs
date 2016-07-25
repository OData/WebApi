using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using System.Xml.Linq;
using Microsoft.OData.Client;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Instancing;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
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

    public class DeltaTests : ODataFormatterTestBase
    {
        protected static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<DeltaTests_Todo>("DeltaTests_Todoes");
            return mb.GetEdmModel();
        }

        public void TestApplyPatchOnIndividualProperty()
        {
            // clear respository
            this.ClearRepository("DeltaTests_Todoes");

            this.Client.GetStringAsync(this.BaseAddress + "/$metadata").Wait();

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
            ctx.SaveChangesAsync().Wait();

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
            ctx.SaveChangesAsync().Wait();

            ctx = ReaderClient(new Uri(this.BaseAddress), ODataProtocolVersion.V4);
            ctx.ResolveName = ResolveName;
            ctx.ResolveType = ResolveType;

            var query = ctx.CreateQuery<DeltaTests_TodoClient>("DeltaTests_Todoes");
            var results = query.ExecuteAsync().Result;
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

            // clear respository
            this.ClearRepository("DeltaTests_Todoes");
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

    [NuwaFramework]
    [NwHost(Nuwa.HostType.KatanaSelf)]
    public class PutDeltaOfTTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DeltaCustomer>("DeltaCustomers");
            builder.EntitySet<DeltaOrder>("DeltaOrders");
            return builder.GetEdmModel();
        }

        [Fact]
        public void PutShouldntOverrideNavigationProperties()
        {
            HttpRequestMessage put = new HttpRequestMessage(HttpMethod.Put, BaseAddress + "/odata/DeltaCustomers(5)");
            dynamic data = new ExpandoObject();
            put.Content = new ObjectContent<dynamic>(data, new JsonMediaTypeFormatter());
            HttpResponseMessage response = Client.SendAsync(put).Result;
            Assert.True(response.IsSuccessStatusCode);

            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/DeltaCustomers(0)?$expand=Orders");
            response = Client.SendAsync(get).Result;
            Assert.True(response.IsSuccessStatusCode);
            dynamic query = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(3, query.Orders.Count);
        }
    }

    [NuwaFramework]
    [NwHost(Nuwa.HostType.KatanaSelf)]
    public class PatchtDeltaOfTTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DeltaCustomer>("DeltaCustomers");
            builder.EntitySet<DeltaOrder>("DeltaOrders");
            return builder.GetEdmModel();
        }

        [Fact]
        public void PatchShouldSupportNonSettableCollectionProperties()
        {
            HttpRequestMessage patch = new HttpRequestMessage(new HttpMethod("MERGE"), BaseAddress + "/odata/DeltaCustomers(5)");
            dynamic data = new ExpandoObject();
            data.Addresses = Enumerable.Range(10, 3).Select(i => new DeltaAddress { ZipCode = i });
            patch.Content = new ObjectContent<dynamic>(data, new JsonMediaTypeFormatter());
            HttpResponseMessage response = Client.SendAsync(patch).Result;
            Assert.True(response.IsSuccessStatusCode);

            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/DeltaCustomers(5)?$expand=Orders");
            response = Client.SendAsync(get).Result;
            Assert.True(response.IsSuccessStatusCode);
            dynamic query = response.Content.ReadAsAsync<JObject>().Result;
            Assert.Equal(3, query.Addresses.Count);
            Assert.Equal(3, query.Orders.Count);
        }
    }

    public class DeltaCustomersController : ODataController
    {
        private static DeltaCustomer customer;

        static DeltaCustomersController()
        {
            customer = new DeltaCustomer("Original name",
                Enumerable.Range(0, 2).Select(i => new DeltaAddress { ZipCode = i }),
                Enumerable.Range(0, 3).Select(i => new DeltaOrder { Details = i.ToString() }));
            customer.Id = 5;
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            if (key == customer.Id)
            {
                return Ok(customer);
            }
            else
            {
                return BadRequest();
            }
        }
        public IHttpActionResult Put([FromODataUri] int key, [FromBody] Delta<DeltaCustomer> entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            entity.Put(customer);
            return Ok(customer);
        }

        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<DeltaCustomer> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
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
