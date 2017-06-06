using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Linq;
using System.Net.Http;
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
    [EntitySetAttribute("SpecialCharactersLinkGenerationTests")]
    [DataServiceKeyAttribute("Name")]
    public class SpecialCharactersLinkGenerationTestsModel
    {
        [Key]
        public string Name { get; set; }
    }

    public class SpecialCharactersLinkGenerationTestsController : EntitySetController<SpecialCharactersLinkGenerationTestsModel, string>
    {
        static SpecialCharactersLinkGenerationTestsController()
        {
            var todoes = new List<SpecialCharactersLinkGenerationTestsModel>();
            foreach (var c in "$&+,/:;=?@ <>#%{}|\\^~[]` ")
            {
                // Skip: 498
                if ("/ \\+?#".Contains(c))
                {
                    continue;
                }

                todoes.Add(new SpecialCharactersLinkGenerationTestsModel()
                {
                    Name = c.ToString()
                });
            }
            Todoes = todoes;
        }

        public static IEnumerable<SpecialCharactersLinkGenerationTestsModel> Todoes { get; set; }

        public override IQueryable<SpecialCharactersLinkGenerationTestsModel> Get()
        {
            return Todoes.AsQueryable();
        }

        protected override SpecialCharactersLinkGenerationTestsModel GetEntityByKey(string key)
        {
            return Todoes.FirstOrDefault(t => t.Name == key);
        }

        protected override SpecialCharactersLinkGenerationTestsModel PatchEntity(string key, Delta<SpecialCharactersLinkGenerationTestsModel> patch)
        {
            var todo = Todoes.FirstOrDefault(t => t.Name == key);
            return todo;
        }
    }

    public class SpecialCharactersLinkGenerationWebTestsController : EntitySetController<SpecialCharactersLinkGenerationTestsModel, string>
    {
        static SpecialCharactersLinkGenerationWebTestsController()
        {
            var todoes = new List<SpecialCharactersLinkGenerationTestsModel>();
            foreach (var c in "$&+,/:;=?@ <>#%{}|\\^~[]` ")
            {
                // Skip: it blocked by IIS settings
                if ("<>*%:+/\\&:?#".Contains(c))
                {
                    continue;
                }

                todoes.Add(new SpecialCharactersLinkGenerationTestsModel()
                {
                    Name = c.ToString()
                });
            }
            Todoes = todoes;
        }

        public static IEnumerable<SpecialCharactersLinkGenerationTestsModel> Todoes { get; set; }

        public override IQueryable<SpecialCharactersLinkGenerationTestsModel> Get()
        {
            return Todoes.AsQueryable();
        }

        protected override SpecialCharactersLinkGenerationTestsModel GetEntityByKey(string key)
        {
            return Todoes.FirstOrDefault(t => t.Name == key);
        }

        protected override SpecialCharactersLinkGenerationTestsModel PatchEntity(string key, Delta<SpecialCharactersLinkGenerationTestsModel> patch)
        {
            var todo = Todoes.FirstOrDefault(t => t.Name == key);
            return todo;
        }
    }

    // Skip webhost as it denies most of the special charactors
    [NuwaFramework]
    [NwHost(HostType.KatanaSelf)]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    //[NuwaTrace(typeof(PlaceholderTraceWriter))]
    public class SpecialCharactersLinkGenerationTests
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

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel());
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationTests");
            return builder.GetEdmModel();
        }

        [Fact]
        public void TestSpecialCharactersInPrimaryKey()
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress));
            var todoes = ctx.CreateQuery<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationTests").ToList();
            bool success = true;
            foreach (var todo in todoes)
            {
                try
                {
                    Uri selfLink;
                    Assert.True(ctx.TryGetUri(todo, out selfLink));
                    Console.WriteLine(selfLink);

                    //ctx.UpdateObject(todo);

                    //var response = ctx.SaveChanges().Single();

                    //Assert.Equal(204, response.StatusCode);

                    ctx.Execute<SpecialCharactersLinkGenerationTestsModel>(selfLink, "GET", true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    success = false;
                }
            }

            Assert.True(success);
        }
    }

    public class SpecialCharactersLinkGenerationWebTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel());
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        public static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationWebTests");
            return builder.GetEdmModel();
        }

        [Fact]
        public void TestSpecialCharactersInPrimaryKey()
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress));
            var todoes = ctx.CreateQuery<SpecialCharactersLinkGenerationTestsModel>("SpecialCharactersLinkGenerationWebTests").ToList();
            bool success = true;
            foreach (var todo in todoes)
            {
                try
                {
                    Uri selfLink;
                    Assert.True(ctx.TryGetUri(todo, out selfLink));
                    Console.WriteLine(selfLink);

                    //ctx.UpdateObject(todo);

                    //var response = ctx.SaveChanges().Single();

                    //Assert.Equal(204, response.StatusCode);

                    ctx.Execute<SpecialCharactersLinkGenerationTestsModel>(selfLink, "GET", true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    success = false;
                }
            }

            Assert.True(success);
        }
    }
}
