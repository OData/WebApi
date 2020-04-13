// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
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

    public class NestedClassController : TestNonODataController
    {
        [HttpGet]
        public IQueryable<Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest> QueryOnNestClass()
        {
            return new Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest[]
            {
                new Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest()
                {
                    Name = "aaa",
                    NestProperty = new NestedClass_Parent.NestPropertyType()
                    {
                        Name = "aaa"
                    }
                },
                new Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest()
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

    public class NestedClassTests : WebHostTestBase<NestedClassTests>
    {
        public NestedClassTests(WebHostTestFixture<NestedClassTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection();
        }

        [Fact]
        public async Task QueryOnNestClassShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/NestedClass/QueryOnNestClass?$filter=Name eq 'aaa'");
            response.EnsureSuccessStatusCode();

            var actual = await response.Content.ReadAsObject<Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest[]>();

            Assert.Equal("aaa", actual.Single().Name);
        }

        [Fact]
        public async Task QueryOnPropertyWithNestedTypeShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/NestedClass/QueryOnNestClass?$filter=NestProperty/Name eq 'aaa'");
            response.EnsureSuccessStatusCode();

            var actual = await response.Content.ReadAsObject<Microsoft.Test.E2E.AspNet.OData.QueryComposition.NestedClass_Parent.Nest[]>();

            Assert.Equal("aaa", actual.Single().NestProperty.Name);
        }
    }
}
