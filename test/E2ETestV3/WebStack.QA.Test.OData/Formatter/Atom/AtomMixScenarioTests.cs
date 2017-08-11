using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter.Atom
{
    public class AtomMixScenarioTests : MixScenarioTestsOData
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.EnableODataSupport(GetEdmModel(configuration), "odata");
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebStack.QA.Common.WebHost.WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        //[Fact]
        //[Trait("Category", "LocalOnly")]
        public void ODataCRUDShouldWorkAtom()
        {
            ODataCRUDShouldWork();
        }
    }
}
