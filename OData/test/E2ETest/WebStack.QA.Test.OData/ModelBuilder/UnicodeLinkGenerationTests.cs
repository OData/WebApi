using System.Web.Http;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Common.Models.ProductFamilies;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    public class UnicodeLinkGeneration_Products : InMemoryODataController<Product, int>
    {
        public UnicodeLinkGeneration_Products()
            : base("ID")
        {
        }
    }

    public class UnicodeLinkGenerationTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetImplicitEdmModel());
        }

        private static IEdmModel GetImplicitEdmModel()
        {
            ODataConventionModelBuilder modelBuilder = new ODataConventionModelBuilder();
            modelBuilder.EntitySet<Product>("UnicodeLinkGeneration_Products");

            return modelBuilder.GetEdmModel();
        }
    }
}
