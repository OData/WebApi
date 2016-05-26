using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Query;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class ODataQueryOptions_Todo
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class CollectionOfString : List<string>
    {
        public CollectionOfString()
        {
        }

        public CollectionOfString(IEnumerable<string> list)
            : base(list)
        {
        }
    }

    public class CollectionOfStringResult
    {
        public CollectionOfString List { get; set; }
        public string NextPage { get; set; }
    }

    public class ODataQueryOptionsController : ApiController
    {
        private static List<ODataQueryOptions_Todo> todoes = new List<ODataQueryOptions_Todo>();
        static ODataQueryOptionsController()
        {
            for (int i = 0; i < 100; i++)
            {
                todoes.Add(new ODataQueryOptions_Todo
                {
                    ID = i,
                    Name = "Test" + i
                });
            }
        }

        [HttpGet]
        public IQueryable<ODataQueryOptions_Todo> OptionsOnIEnumerableT(ODataQueryOptions options)
        {
            return options.ApplyTo(todoes.AsQueryable()) as IQueryable<ODataQueryOptions_Todo>;
        }

        [HttpGet]
        public IQueryable<string> OptionsOnIEnumerableString(ODataQueryOptions<ODataQueryOptions_Todo> options)
        {
            return (options.ApplyTo(todoes.AsQueryable()) as IQueryable<ODataQueryOptions_Todo>).Select(t => t.Name);
        }

        [HttpGet]
        public string OptionsOnString(ODataQueryOptions<ODataQueryOptions_Todo> options)
        {
            return (options.ApplyTo(todoes.AsQueryable()) as IQueryable<ODataQueryOptions_Todo>).Select(t => t.Name).First();
        }

        [HttpGet]
        public int GetTopValue(ODataQueryOptions<ODataQueryOptions_Todo> options)
        {
            return options.Top.Value;
        }

        [HttpGet]
        public HttpResponseMessage OptionsOnHttpResponseMessage(ODataQueryOptions<ODataQueryOptions_Todo> options)
        {
            var t = options.ApplyTo(todoes.AsQueryable()) as IQueryable<ODataQueryOptions_Todo>;
            return this.Request.CreateResponse(System.Net.HttpStatusCode.OK, new { Todoes = t });
        }

        [HttpGet]
        public CollectionOfStringResult OptionsWithString(ODataQueryOptions<string> options)
        {
            CollectionOfString list = new CollectionOfString();
            list.Add("One");
            list.Add("Two");
            list.Add("Three");
            return new CollectionOfStringResult
            {
                NextPage = "Test",
                List = new CollectionOfString(options.ApplyTo(list.AsQueryable()) as IQueryable<string>)
            };
        }
    }

    public class ODataQueryOptionsTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
        }

        [Fact]
        public void OptionsOnIEnumerableTShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ODataQueryOptions/OptionsOnIEnumerableT?$filter=ID ge 50").Result;
            var actual = response.Content.ReadAsAsync<IEnumerable<ODataQueryOptions_Todo>>().Result;
            Assert.Equal(50, actual.Count());
        }

        [Fact]
        public void OptionsOnStringShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ODataQueryOptions/OptionsOnString?$filter=ID ge 50").Result;
            var actual = response.Content.ReadAsAsync<string>().Result;
            Assert.Equal("Test50", actual);
        }

        [Fact]
        public void GetTopValueShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ODataQueryOptions/GetTopValue?$top=50").Result;
            var actual = response.Content.ReadAsAsync<int>().Result;
            Assert.Equal(50, actual);
        }

        [Fact]
        public void OptionsOnHttpResponseMessageShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/ODataQueryOptions/OptionsOnHttpResponseMessage?$filter=ID ge 50").Result;
            var actual = response.Content.ReadAsStringAsync().Result;
            Console.WriteLine(actual);
        }

        [Fact]
        public void UnitTestOptionsShouldWork()
        {
            ODataQueryOptionsController controller = new ODataQueryOptionsController();

            ODataQueryContext context = new ODataQueryContext(GetEdmModel(), typeof(ODataQueryOptions_Todo), path: null, defaultQuerySettings: null);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$orderby=Name desc");
            ODataQueryOptions<ODataQueryOptions_Todo> options = new ODataQueryOptions<ODataQueryOptions_Todo>(context, request);
            var result = controller.OptionsOnString(options);
            Assert.Equal("Test99", result);
        }

        [Fact]
        public void UnitTestOptionsOfStringShouldWork()
        {
            ODataQueryOptionsController controller = new ODataQueryOptionsController();

            ODataQueryContext context = new ODataQueryContext(new ODataConventionModelBuilder().GetEdmModel(), typeof(string), path: null, defaultQuerySettings: null);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/?$top=1");
            ODataQueryOptions<string> options = new ODataQueryOptions<string>(context, request);
            var result = controller.OptionsWithString(options);
            Assert.Equal("One", result.List.Single());
        }

        private Microsoft.OData.Edm.IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntityType<ODataQueryOptions_Todo>();
            return builder.GetEdmModel();
        }
    }
}
