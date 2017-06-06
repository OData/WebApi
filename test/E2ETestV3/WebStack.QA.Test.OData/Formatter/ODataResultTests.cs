using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Data.Services.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Query;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter
{
    [EntitySet("ODataResult_Model1")]
    [DataServiceKey("ID")]
    public class ODataResult_Model1
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public ICollection<ODataResult_Model2> Model2 { get; set; }
    }

    [EntitySet("ODataResult_Model2")]
    [DataServiceKey("ID")]
    public class ODataResult_Model2
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class ODataResult_Model1Controller : InMemoryEntitySetController<ODataResult_Model1, int>
    {
        public ODataResult_Model1Controller()
            : base("ID")
        {
        }

        [HttpGet]
        public PageResult<ODataResult_Model2> GetModel2(ODataQueryOptions options, int key, int count)
        {
            var models = new List<ODataResult_Model2>();
            for (int i = 0; i < count; i++)
            {
                models.Add(new ODataResult_Model2
                {
                    ID = i,
                    Name = "Test " + i
                });
            }
            var baseUri = new Uri(this.Url.CreateODataLink());
            var uri = new Uri(this.Url.CreateODataLink(new EntitySetPathSegment("ODataResult_Model2")));
            return new PageResult<ODataResult_Model2>(models, baseUri.MakeRelativeUri(uri), count);
        }
    }

    public class ODataResultTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<ODataResult_Model1>("ODataResult_Model1");
            mb.EntitySet<ODataResult_Model2>("ODataResult_Model2");
            return mb.GetEdmModel();
        }

        [Fact]
        public void ODataResultWithZeroResultShouldWork()
        {
            DataServiceContext ctx = new DataServiceContext(new Uri(this.BaseAddress), DataServiceProtocolVersion.V3);
            ctx.AddObject("ODataResult_Model1", new ODataResult_Model1() { ID = 1, Name = "ABC" });
            ctx.SaveChanges();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/ODataResult_Model1(1)/Model2?count=0");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata=verbose"));
            var response = this.Client.SendAsync(request).Result;
            Console.WriteLine(response);
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            response.EnsureSuccessStatusCode();
        }
    }
}
