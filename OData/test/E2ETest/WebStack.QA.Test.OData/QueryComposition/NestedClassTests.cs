using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class NestedClass_Parent
    {
        public class Nest
        {
            public string Name { get; set; }
            public NestPropertyType NestProperty { get; set; }
        }

        public class NestPropertyType
        {
            public string Name { get; set; }
        }
    }

    public class NestedClassController : ApiController
    {
        [HttpGet]
        public IQueryable<WebStack.QA.Test.OData.QueryComposition.NestedClass_Parent.Nest> QueryOnNestClass()
        {
            return new WebStack.QA.Test.OData.QueryComposition.NestedClass_Parent.Nest[]
            {
                new WebStack.QA.Test.OData.QueryComposition.NestedClass_Parent.Nest()
                {
                    Name = "aaa",
                    NestProperty = new NestedClass_Parent.NestPropertyType()
                    {
                        Name = "aaa"
                    }
                },
                new WebStack.QA.Test.OData.QueryComposition.NestedClass_Parent.Nest()
                {
                    Name = "bbb",
                    NestProperty = new NestedClass_Parent.NestPropertyType()
                    {
                        Name = "bbb"
                    }
                }
            }.AsQueryable();
        }
    }

    public class NestedClassTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.AddODataQueryFilter();
        }

        [Fact]
        public void QueryOnNestClassShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/NestedClass/QueryOnNestClass?$filter=Name eq 'aaa'").Result;
            response.EnsureSuccessStatusCode();

            var actual = response.Content.ReadAsAsync<WebStack.QA.Test.OData.QueryComposition.NestedClass_Parent.Nest[]>().Result;

            Assert.Equal("aaa", actual.Single().Name);
        }

        [Fact]
        public void QueryOnPropertyWithNestedTypeShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/NestedClass/QueryOnNestClass?$filter=NestProperty/Name eq 'aaa'").Result;
            response.EnsureSuccessStatusCode();

            var actual = response.Content.ReadAsAsync<WebStack.QA.Test.OData.QueryComposition.NestedClass_Parent.Nest[]>().Result;

            Assert.Equal("aaa", actual.Single().NestProperty.Name);
        }
    }
}
