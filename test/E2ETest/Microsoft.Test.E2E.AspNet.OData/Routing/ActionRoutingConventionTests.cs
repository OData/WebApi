// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Routing
{
    public class ActionRoutingConventionTests : WebHostTestBase<ActionRoutingConventionTests>
    {
        public ActionRoutingConventionTests(WebHostTestFixture<ActionRoutingConventionTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            EntitySetConfiguration<ActionCar> cars = builder.EntitySet<ActionCar>("ActionCars");
            EntitySetConfiguration<ActionFerrari> ferraris = builder.EntitySet<ActionFerrari>("ActionFerraris");
            cars.EntityType.Action("Wash").Returns<string>();
            cars.EntityType.Collection.Action("Wash").Returns<string>();
            ActionConfiguration ferrariWash = ferraris.EntityType.Action("Wash").Returns<string>();
            ActionConfiguration ferrariCollectionWash = ferraris.EntityType.Collection.Action("Wash").Returns<string>();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("/odata/ActionCars(5)/Microsoft.Test.E2E.AspNet.OData.Routing.ActionFerrari/Default.Wash", "Ferrari")]
        [InlineData("/odata/ActionCars(5)/Default.Wash", "Car")]
        [InlineData("/odata/ActionFerraris(5)/Microsoft.Test.E2E.AspNet.OData.Routing.ActionCar/Default.Wash", "Car")]
        [InlineData("/odata/ActionFerraris(5)/Default.Wash", "Ferrari")]
        [InlineData("/odata/ActionCars/Microsoft.Test.E2E.AspNet.OData.Routing.ActionFerrari/Default.Wash", "FerrariCollection")]
        [InlineData("/odata/ActionCars/Default.Wash", "CarCollection")]
        [InlineData("/odata/ActionFerraris/Microsoft.Test.E2E.AspNet.OData.Routing.ActionCar/Default.Wash", "CarCollection")]
        [InlineData("/odata/ActionFerraris/Default.Wash", "FerrariCollection")]
        public async Task CanSupportOverloadOnDerivedBindableTypes(string url, string expectedResult)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + url);
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            Assert.Equal(expectedResult, (string)(await response.Content.ReadAsObject<JObject>())["value"]);
        }
    }

    public class ActionFerrarisController : TestODataController
    {
        public ITestActionResult WashOnActionCar(int key)
        {
            return Ok("Car");
        }

        public ITestActionResult Wash(int key)
        {
            return Ok("Ferrari");
        }

        public ITestActionResult WashOnCollectionOfActionCar()
        {
            return Ok("CarCollection");
        }

        public ITestActionResult Wash()
        {
            return Ok("FerrariCollection");
        }
    }

    public class ActionCarsController : TestODataController
    {
        public ITestActionResult WashOnActionFerrari(int key)
        {
            return Ok("Ferrari");
        }

        public ITestActionResult Wash(int key)
        {
            return Ok("Car");
        }

        public ITestActionResult WashOnCollectionOfActionFerrari()
        {
            return Ok("FerrariCollection");
        }

        public ITestActionResult Wash()
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
