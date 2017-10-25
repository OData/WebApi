﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
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
            config.MapODataServiceRoute("odata", "odata", builder =>
                builder.AddService(ServiceLifetime.Singleton, sp => GetModel())
                    .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                        ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config))
                    .AddService<ODataUriResolver>(ServiceLifetime.Singleton, sp => new UnqualifiedODataUriResolver()));
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
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
        public void CanCallBoundActionWithUnqualifiedRouteName(string url, string expectedResult)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseAddress + url);
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            Assert.NotNull(response.Content);
            Assert.Equal(expectedResult, (string)response.Content.ReadAsAsync<JObject>().Result["value"]);
        }

        [Theory]
        [InlineData("/odata/UnqualifiedCars(5)/Check", "CheckSingle5")]
        [InlineData("/odata/UnqualifiedCars(5)/Default.Check", "CheckSingle5")]
        [InlineData("/odata/UnqualifiedCars/Check", "CheckCollection")]
        [InlineData("/odata/UnqualifiedCars/Default.Check", "CheckCollection")]
        public void CanCallBoundFunctionWithUnqualifiedRouteName(string url, string expectedResult)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + url);
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

        [HttpGet]
        [ODataRoute("UnqualifiedCars({key})/Check")]
        public IHttpActionResult CheckSingle([FromODataUri]int key)
        {
            return Ok("CheckSingle" + key);
        }

        [HttpGet]
        [ODataRoute("UnqualifiedCars/Check")]
        public IHttpActionResult CheckOnCollection()
        {
            return Ok("CheckCollection");
        }
    }

    public class UnqualifiedCar
    {
        public int Id { get; set; }
    }
}
