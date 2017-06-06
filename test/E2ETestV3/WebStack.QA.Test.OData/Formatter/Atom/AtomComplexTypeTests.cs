using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.Formatter.Atom
{
    public class AtomComplexTypeTests : ComplexTypeTests
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Formatters.Clear();
            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [Fact]
        public void ShouldSupportDerivedComplexTypePayloadAtom()
        {
            ShouldSupportDerivedComplexType();
        }
    }
}
