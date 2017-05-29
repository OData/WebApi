using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.Edm;
using Newtonsoft.Json.Linq;
using Nuwa;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Routing
{
    [NuwaFramework]
    [NwHost(Nuwa.HostType.KatanaSelf)]
    public class ActionRoutingConventionTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Routes.MapODataServiceRoute("odata", "odata", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<ActionCar> cars = builder.EntitySet<ActionCar>("ActionCars");
            EntitySetConfiguration<ActionFerrari> ferraris = builder.EntitySet<ActionFerrari>("ActionFerraris");
            cars.EntityType.Action("Wash").Returns<string>();
            cars.EntityType.Collection.Action("Wash").Returns<string>();
            ActionConfiguration ferrariWash = ferraris.EntityType.Action("Wash").Returns<string>();
            ActionConfiguration ferrariCollectionWash = ferraris.EntityType.Collection.Action("Wash").Returns<string>();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("/odata/ActionCars(5)/WebStack.QA.Test.OData.Routing.ActionFerrari/Wash", "Ferrari")]
        [InlineData("/odata/ActionCars(5)/Wash", "Car")]
        [InlineData("/odata/ActionFerraris(5)/WebStack.QA.Test.OData.Routing.ActionCar/Wash", "Car")]
        [InlineData("/odata/ActionFerraris(5)/Wash", "Ferrari")]
        [InlineData("/odata/ActionCars/WebStack.QA.Test.OData.Routing.ActionFerrari/Wash", "FerrariCollection")]
        [InlineData("/odata/ActionCars/Wash", "CarCollection")]
        [InlineData("/odata/ActionFerraris/WebStack.QA.Test.OData.Routing.ActionCar/Wash", "CarCollection")]
        [InlineData("/odata/ActionFerraris/Wash", "FerrariCollection")]
        public void CanSupportOverloadOnDerivedBindableTypes(string url, string expectedResult)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + url);
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            Assert.Equal(expectedResult, (string)response.Content.ReadAsAsync<JObject>().Result["value"]);
        }
    }
    
    public class ActionFerrarisController : ODataController
    {
        public IHttpActionResult WashOnActionCar(int key)
        {
            return Ok("Car");
        }

        public IHttpActionResult Wash(int key)
        {
            return Ok("Ferrari");
        }

        public IHttpActionResult WashOnCollectionOfActionCar()
        {
            return Ok("CarCollection");
        }

        public IHttpActionResult Wash()
        {
            return Ok("FerrariCollection");
        }
    }

    public class ActionCarsController : ODataController
    {
        public IHttpActionResult WashOnActionFerrari(int key)
        {
            return Ok("Ferrari");
        }

        public IHttpActionResult Wash(int key)
        {
            return Ok("Car");
        }

        public IHttpActionResult WashOnCollectionOfActionFerrari()
        {
            return Ok("FerrariCollection");
        }

        public IHttpActionResult Wash()
        {
            return Ok("CarCollection");
        }
    }
    
    public class ActionCar
    {
        public int Id { get; set; }
    }

    public class ActionFerrari : ActionCar
    {
    }
}
