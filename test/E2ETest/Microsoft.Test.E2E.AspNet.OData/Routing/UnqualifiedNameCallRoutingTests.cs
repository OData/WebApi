// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Routing
{
    public class UnqualifiedNameCallRoutingTests : WebHostTestBase<UnqualifiedNameCallRoutingTests>
    {
        public UnqualifiedNameCallRoutingTests(WebHostTestFixture<UnqualifiedNameCallRoutingTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
        {
            var controllers = new[] { typeof(UnqualifiedCarsController) };
            config.AddControllers(controllers);

            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => GetModel(config))
                    .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                        ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config))
                    .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new UnqualifiedODataUriResolver()));
        }

        private static IEdmModel GetModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<UnqualifiedCar> cars = builder.EntitySet<UnqualifiedCar>("UnqualifiedCars");
            cars.EntityType.Action("Wash").Returns<string>();
            cars.EntityType.Collection.Action("Wash").Returns<string>();
            cars.EntityType.Function("Check").Returns<string>();
            cars.EntityType.Collection.Function("Check").Returns<string>();
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("/odata/UnqualifiedCars(5)/Wash", "WashSingle5")]
        [InlineData("/odata/UnqualifiedCars(5)/Default.Wash", "WashSingle5")]
        [InlineData("/odata/UnqualifiedCars/Wash", "WashCollection")]
        [InlineData("/odata/UnqualifiedCars/Default.Wash", "WashCollection")]
        public async Task CanCallBoundActionWithUnqualifiedRouteName(string url, string expectedResult)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + url);
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            Assert.Equal(expectedResult, (string)(await response.Content.ReadAsObject<JObject>())["value"]);
        }

        [Theory]
        [InlineData("/odata/UnqualifiedCars(5)/Check", "CheckSingle5")]
        [InlineData("/odata/UnqualifiedCars(5)/Default.Check", "CheckSingle5")]
        [InlineData("/odata/UnqualifiedCars/Check", "CheckCollection")]
        [InlineData("/odata/UnqualifiedCars/Default.Check", "CheckCollection")]
        public async Task CanCallBoundFunctionWithUnqualifiedRouteName(string url, string expectedResult)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + url);
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            Assert.Equal(expectedResult, (string)(await response.Content.ReadAsObject<JObject>())["value"]);
        }
    }

    public class UnqualifiedCarsController : TestODataController
    {
        [ODataRoute("UnqualifiedCars({key})/Wash")]
        public ITestActionResult WashSingle([FromODataUri]int key)
        {
            return Ok("WashSingle" + key);
        }

        [ODataRoute("UnqualifiedCars/Wash")]
        public ITestActionResult WashOnCollection()
        {
            return Ok("WashCollection");
        }

        [HttpGet]
        [ODataRoute("UnqualifiedCars({key})/Check")]
        public ITestActionResult CheckSingle([FromODataUri]int key)
        {
            return Ok("CheckSingle" + key);
        }

        [HttpGet]
        [ODataRoute("UnqualifiedCars/Check")]
        public ITestActionResult CheckOnCollection()
        {
            return Ok("CheckCollection");
        }
    }

    public class UnqualifiedCar
    {
        public int Id { get; set; }
    }
}
