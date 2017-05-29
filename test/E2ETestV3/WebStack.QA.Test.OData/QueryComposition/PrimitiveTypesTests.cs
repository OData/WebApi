using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Extensions;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class PrimitiveTypesController : ApiController
    {
        private List<string> stringList = new List<string>();

        public PrimitiveTypesController()
        {
            stringList.Add("One");
            stringList.Add("Two");
            stringList.Add("Three");
        }

        public IQueryable<string> GetIQueryableOfString()
        { 
            return stringList.AsQueryable();
        }

        [EnableQuery]
        public IEnumerable<string> GetIEnumerableOfString()
        {
            return stringList.AsEnumerable();
        }
    }

    public class PrimitiveTypesTests : ODataTestBase
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

        [Theory]
        [InlineData("/api/PrimitiveTypes/GetIQueryableOfString?$skip=1&$top=1")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$skip=1&$top=1")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$filter=$it eq 'Two'")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$filter=indexof($it, 'Two') ne -1")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$orderby=$it desc&$top=1")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$filter=$it gt 'Three'")]
        public void TestSkipAndTopOnString(string url)
        {
            var response = this.Client.GetAsync(this.BaseAddress + url).Result;
            response.EnsureSuccessStatusCode();
            var actual = response.Content.ReadAsAsync<IEnumerable<string>>().Result;

            Assert.Equal("Two", actual.First());
        }
    }
}
