using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.ODataPathHandler
{
    public class LinkGeneration_Model_v1
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class LinkGeneration_Model_v2
    {
        public int ID { get; set; }
        public string Name { get; set; }
    }

    public class LinkGeneration_Model1Controller : ODataController
    {
        public IQueryable<LinkGeneration_Model_v1> Get()
        {
            return new LinkGeneration_Model_v1[] 
            { 
                new LinkGeneration_Model_v1
                {
                    ID = 1,
                    Name = "One"
                }
            }.AsQueryable();
        }
    }
    public class LinkGeneration_Model2Controller : ODataController
    {
        public IQueryable<LinkGeneration_Model_v2> Get()
        {
            return new LinkGeneration_Model_v2[] 
            { 
                new LinkGeneration_Model_v2
                {
                    ID = 1,
                    Name = "One"
                }
            }.AsQueryable();
        }
    }

    public class LinkGenerationTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            var model1 = GetEdmModel1(configuration);
            var model2 = GetEdmModel2(configuration);
            configuration.MapODataServiceRoute("OData1", "v1", model1, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.MapODataServiceRoute("OData2", "v2", model2, new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
            configuration.Routes.MapHttpRoute("ApiDefault", "api/{controller}/{action}/{id}", new { id = RouteParameter.Optional });
        }

        protected static IEdmModel GetEdmModel1(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<LinkGeneration_Model_v1>("LinkGeneration_Model1");
            return mb.GetEdmModel();
        }

        protected static IEdmModel GetEdmModel2(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
            mb.EntitySet<LinkGeneration_Model_v2>("LinkGeneration_Model2");
            return mb.GetEdmModel();
        }

        [Fact]
        public void GeneratedLinkShouldMatchRequestRouting()
        {
            var content = this.Client.GetStringAsync(this.BaseAddress + "/v1/LinkGeneration_Model1").Result;
            Assert.DoesNotContain(@"/v2/LinkGeneration_Model1", content);

            content = this.Client.GetStringAsync(this.BaseAddress + "/v2/LinkGeneration_Model2").Result;
            Assert.DoesNotContain(@"/v1/LinkGeneration_Model2", content);
        }
    }
}
