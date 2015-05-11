using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Routing
{
    [NuwaFramework]
    [NwHost(HostType.KatanaSelf)]
    public class UnqualifiedNameCallRoutingTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            var controllers = new[] { typeof(UnqualifiedCarsController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            config.Services.Replace(typeof(IAssembliesResolver), resolver);
            config.Routes.Clear();
            config.EnableUnqualifiedNameCall(true);
            config.MapODataServiceRoute("odata", "odata", GetModel());
             config.EnsureInitialized();
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<UnqualifiedCar> cars = builder.EntitySet<UnqualifiedCar>("UnqualifiedCars");
            cars.EntityType.Action("Wash").Returns<string>();
            cars.EntityType.Collection.Action("Wash").Returns<string>();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("/odata/UnqualifiedCars(5)/Wash", "WashSingle5")]
        [InlineData("/odata/UnqualifiedCars/Wash", "WashCollection")]
        public void CanCallActionWithUnqualifiedRouteName(string url, string expectedResult)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + url);
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            Assert.Equal(expectedResult, (string)response.Content.ReadAsAsync<JObject>().Result["value"]);
        }
    }

    public class UnqualifiedCarsController : ODataController
    {
        [ODataRoute("UnqualifiedCars({key})/Wash")]
        public IHttpActionResult WashSingle([FromODataUri]int key)
        {
            return Ok("WashSingle" + key);
        }

        [ODataRoute("UnqualifiedCars/Wash")]
        public IHttpActionResult WashOnCollection()
        {
            return Ok("WashCollection");
        }
    }

    public class UnqualifiedCar
    {
        public int Id { get; set; }
    }
}
