using System.Net;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using Microsoft.Data.Edm;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class UriParser_Model1
    {
        public UriParser_Model1 Self { get; set; }
    }

    public class UriParserTests : ODataTestBase
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
            mb.EntitySet<UriParser_Model1>("UriParser_Model1");
            return mb.GetEdmModel();
        }

        [Fact]
        public void TestDeepNestedUri()
        {
            var url = new AttackStringBuilder().Append("/UriParser_Model1(0)/").Repeat("Self/", 150).ToString();
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
        }
    }
}
