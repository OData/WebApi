using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.ETags
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class ETagCurrencyTokenEfContextTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(DominiosController)};
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel());
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder();
            builder.EntitySet<Dominio>("Dominios");
            builder.EntitySet<Server>("Servers");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task NestedDollarSelectWorksOnCurrencyTokenProperty()
        {
            string expect = "{\r\n" +
"  \"@odata.context\":\"{XXXX}\",\"value\":[\r\n" +
"    {\r\n" +
"      \"@odata.etag\":\"W/\\\"bnVsbA==\\\"\",\"Id\":\"1\",\"Descrizione\":\"Test1\",\"ServerAutenticazioneId\":\"1\",\"RECVER\":null,\"ServerAutenticazione\":{\r\n" +
"        \"@odata.etag\":\"W/\\\"bnVsbA==\\\"\",\"Id\":\"1\",\"RECVER\":null\r\n" +
"      }\r\n" +
"    },{\r\n" +
"      \"@odata.etag\":\"W/\\\"MTA=\\\"\",\"Id\":\"2\",\"Descrizione\":\"Test2\",\"ServerAutenticazioneId\":\"2\",\"RECVER\":10,\"ServerAutenticazione\":{\r\n" +
"        \"@odata.etag\":\"W/\\\"NQ==\\\"\",\"Id\":\"2\",\"RECVER\":5\r\n" +
"      }\r\n" +
"    }\r\n" +
"  ]\r\n" +
"}";
            // Remove indentation
            expect = Regex.Replace(expect, @"\r\n\s*([""{}\]])", "$1");

            expect = expect.Replace("{XXXX}", string.Format("{0}/odata/$metadata#Dominios(ServerAutenticazione(Id,RECVER))", BaseAddress.ToLowerInvariant()));

            var getUri = this.BaseAddress + "/odata/Dominios?$expand=ServerAutenticazione($select=Id,RECVER)";

            var response = await Client.GetAsync(getUri);

            response.EnsureSuccessStatusCode();

            Assert.NotNull(response.Content);

            var payload = await response.Content.ReadAsStringAsync();

            Assert.Equal(expect, payload);
        }
    }
}
