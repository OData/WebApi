using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    #region Guid
    [EntitySetAttribute("GuidPrimaryKeyType")]
    [DataServiceKeyAttribute("ID")]
    public class GuidPrimaryKeyType
    {
        public Guid ID { get; set; }
    }

    public class GuidPrimaryKeyTypeController : ODataController
    {
        static GuidPrimaryKeyTypeController()
        {
            CachedData = new GuidPrimaryKeyType[]
            {
                new GuidPrimaryKeyType()
                {
                    ID = Guid.NewGuid()
                },
                new GuidPrimaryKeyType()
                {
                    ID = Guid.NewGuid()
                },
                new GuidPrimaryKeyType()
                {
                    ID = Guid.NewGuid()
                }
            };
        }

        public static IEnumerable<GuidPrimaryKeyType> CachedData { get; set; }

        public IEnumerable<GuidPrimaryKeyType> Get()
        {
            return CachedData;
        }

        [AcceptVerbs("MERGE", "PATCH")]
        public GuidPrimaryKeyType Patch([FromODataUri]Guid key, GuidPrimaryKeyType model)
        {
            model = CachedData.Single(d => d.ID == key);
            return model;
        }
    }
    #endregion

    #region string
    [EntitySetAttribute("StringPrimaryKeyType")]
    [DataServiceKeyAttribute("ID")]
    public class StringPrimaryKeyType
    {
        public string ID { get; set; }
    }

    public class StringPrimaryKeyTypeController : ODataController
    {
        static StringPrimaryKeyTypeController()
        {
            CachedData = new StringPrimaryKeyType[]
            {
                new StringPrimaryKeyType()
                {
                    ID = "Test 1".ToString()
                },
                new StringPrimaryKeyType()
                {
                    ID = "Test 2".ToString()
                },
                new StringPrimaryKeyType()
                {
                    ID = "Test 3".ToString()
                }
            };
        }

        public static IEnumerable<StringPrimaryKeyType> CachedData { get; set; }

        public IEnumerable<StringPrimaryKeyType> Get()
        {
            return CachedData;
        }

        [AcceptVerbs("MERGE", "PATCH")]
        public StringPrimaryKeyType Patch([FromODataUri]string key, StringPrimaryKeyType model)
        {
            model = CachedData.Single(d => d.ID == key);
            return model;
        }
    }
    #endregion

    #region UInt
    public class UIntPrimaryKeyType
    {
        public uint ID { get; set; }
    }

    [EntitySetAttribute("UIntPrimaryKeyType")]
    [DataServiceKeyAttribute("ID")]
    public class UIntPrimaryKeyType_Client
    {
        public long ID { get; set; }
    }

    public class UIntPrimaryKeyTypeController : ODataController
    {
        static UIntPrimaryKeyTypeController()
        {
            CachedData = new UIntPrimaryKeyType[]
            {
                new UIntPrimaryKeyType()
                {
                    ID = 1
                },
                new UIntPrimaryKeyType()
                {
                    ID = 2
                },
                new UIntPrimaryKeyType()
                {
                    ID = 3
                }
            };
        }

        public static IEnumerable<UIntPrimaryKeyType> CachedData { get; set; }

        public IEnumerable<UIntPrimaryKeyType> Get()
        {
            return CachedData;
        }

        [AcceptVerbs("MERGE", "PATCH")]
        public UIntPrimaryKeyType Patch([FromODataUri]uint key, UIntPrimaryKeyType model)
        {
            model = CachedData.Single(d => d.ID == key);
            return model;
        }
    }
    #endregion

    #region Long
    [EntitySetAttribute("LongPrimaryKeyType")]
    [DataServiceKeyAttribute("ID")]
    public class LongPrimaryKeyType
    {
        public long ID { get; set; }
    }

    public class LongPrimaryKeyTypeController : ODataController
    {
        static LongPrimaryKeyTypeController()
        {
            CachedData = new LongPrimaryKeyType[]
            {
                new LongPrimaryKeyType()
                {
                    ID = 1
                },
                new LongPrimaryKeyType()
                {
                    ID = 2
                },
                new LongPrimaryKeyType()
                {
                    ID = 3
                }
            };
        }

        public static IEnumerable<LongPrimaryKeyType> CachedData { get; set; }

        public IEnumerable<LongPrimaryKeyType> Get()
        {
            return CachedData;
        }

        [AcceptVerbs("MERGE", "PATCH")]
        public LongPrimaryKeyType Patch([FromODataUri]long key, LongPrimaryKeyType model)
        {
            model = CachedData.Single(d => d.ID == key);
            return model;
        }
    }
    #endregion

    public class SpecialPrimaryKeyTypesInLinkGenerationTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel());
            //configuration.Services.Replace(typeof(ModelBinderProvider), new ODataModelBinderProvider());
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<GuidPrimaryKeyType>("GuidPrimaryKeyType");
            builder.EntitySet<StringPrimaryKeyType>("StringPrimaryKeyType");
            builder.EntitySet<UIntPrimaryKeyType>("UIntPrimaryKeyType");
            builder.EntitySet<LongPrimaryKeyType>("LongPrimaryKeyType");
            return builder.GetEdmModel();
        }

        //[Fact]
        //[Trait("Category", "LocalOnly")]
        public void TestGuidTypeAsPrimaryKey()
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress));
            var models = ctx.CreateQuery<GuidPrimaryKeyType>("GuidPrimaryKeyType").ToList();

            foreach (var model in models)
            {
                Uri selfLink;
                Assert.True(ctx.TryGetUri(model, out selfLink));
                Console.WriteLine(selfLink);

                ctx.UpdateObject(model);

                var response = ctx.SaveChanges().Single();

                Assert.Equal(200, response.StatusCode);
            }
        }

        //[Fact]
        //[Trait("Category", "LocalOnly")]
        public void TestStringTypeAsPrimaryKey()
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress));
            var models = ctx.CreateQuery<StringPrimaryKeyType>("StringPrimaryKeyType").ToList();

            foreach (var model in models)
            {
                Uri selfLink;
                Assert.True(ctx.TryGetUri(model, out selfLink));
                Console.WriteLine(selfLink);

                ctx.UpdateObject(model);

                var response = ctx.SaveChanges().Single();

                Assert.Equal(200, response.StatusCode);
            }
        }

        //[Fact]
        //[Trait("Category", "LocalOnly")]
        public void TestUIntTypeAsPrimaryKey()
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress));
            var models = ctx.CreateQuery<UIntPrimaryKeyType_Client>("UIntPrimaryKeyType").ToList();

            foreach (var model in models)
            {
                Uri selfLink;
                Assert.True(ctx.TryGetUri(model, out selfLink));
                Console.WriteLine(selfLink);

                ctx.UpdateObject(model);

                var response = ctx.SaveChanges().Single();

                Assert.Equal(200, response.StatusCode);
            }
        }

        //[Fact]
        //[Trait("Category", "LocalOnly")]
        public void TestLongTypeAsPrimaryKey()
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress));
            var models = ctx.CreateQuery<LongPrimaryKeyType>("LongPrimaryKeyType").ToList();

            foreach (var model in models)
            {
                Uri selfLink;
                Assert.True(ctx.TryGetUri(model, out selfLink));
                Console.WriteLine(selfLink);

                ctx.UpdateObject(model);

                var response = ctx.SaveChanges().Single();

                Assert.Equal(200, response.StatusCode);
            }
        }
    }
}
