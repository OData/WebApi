using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData.Extensions;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class AnonymousType_Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class AnonymousTypeController : ApiController
    {
        public IQueryable Get()
        {
            var personList = new List<AnonymousType_Person> 
            {
                new AnonymousType_Person { Id = 1, FirstName = "John", LastName = "Smith" },
                new AnonymousType_Person { Id = 2, FirstName = "Eugene", LastName = "Agafonov" }
            };
            var query = from p in personList select new { p.FirstName, p.LastName };
            return query.AsQueryable();
        }
    }

    public class AnonymousTypeTests : ODataTestBase
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.AddODataQueryFilter();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [Fact]
        public void ReturnIQueryableOfAnonymousTypeShouldWork()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/AnonymousType/Get?$filter=FirstName eq 'John'").Result;
            var actual = response.Content.ReadAsAsync<IEnumerable<AnonymousType_Person>>().Result;
            Assert.Equal(1, actual.Count());
            Assert.Equal("John", actual.First().FirstName);
        }
    }
}
