using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData.Extensions;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;

namespace WebStack.QA.Test.OData.ODataOrderByTest
{
    [NuwaFramework]
    [NuwaTrace(NuwaTraceAttribute.Tag.Off)]
    public class ODataOrderByTest
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(ItemsController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();
            HttpServer httpServer = configuration.GetHttpServer();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            configuration.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: OrderByEdmModel.GetModel());

            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task TestOrderByResultItem()
        {   // Arrange
            var requestUri = string.Format("{0}/odata/Items", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResult = response.Content.ReadAsStringAsync().Result;
            var jsonResult = JObject.Parse(rawResult);
            var jsonValue = jsonResult.SelectToken("value");
            Assert.NotNull(jsonValue);
            var concreteResult = jsonValue.ToObject<List<Item>>();
            Assert.NotEmpty(concreteResult);
            for (var i = 0; i < concreteResult.Count - 1; i++)
            {
                var value = string.Format("#{0}", i + 1);
                Assert.True(concreteResult[i].Name.StartsWith(value), "Incorrect order.");
            }
        }

        [Fact]
        public async Task TestOrderByResultItem2()
        {   // Arrange
            var requestUri = string.Format("{0}/odata/Items2", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResult = response.Content.ReadAsStringAsync().Result;
            var jsonResult = JObject.Parse(rawResult);
            var jsonValue = jsonResult.SelectToken("value");
            Assert.NotNull(jsonValue);
            var concreteResult = jsonValue.ToObject<List<Item2>>();
            Assert.NotEmpty(concreteResult);
            for (var i = 0; i < concreteResult.Count - 1; i++)
            {
                var value = string.Format("#{0}", i + 1);
                Assert.True(concreteResult[i].Name.StartsWith(value), "Incorrect order.");
            }
        }
    }
}