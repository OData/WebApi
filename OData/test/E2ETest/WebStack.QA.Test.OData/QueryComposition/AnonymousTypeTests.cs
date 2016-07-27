using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Extensions;
using Nuwa;
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
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection();
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
