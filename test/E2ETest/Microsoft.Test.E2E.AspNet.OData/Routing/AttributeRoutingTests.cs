// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AttributeRoutingTests : WebHostTestBase<AttributeRoutingTests>
    {
        public AttributeRoutingTests(WebHostTestFixture<AttributeRoutingTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.AddControllers(new Type[] { typeof(DogsController), typeof(CatsController), typeof(OwnersController) });

            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<Dog>("Dogs").EntityType.Collection.Function("BestDog").Returns<string>();
            builder.EntitySet<Owner>("Owners").EntityType.Collection.Function("BestOwner").Returns<string>();
            config.MapODataServiceRoute("Dog", "dog", builder.GetEdmModel());

            builder = config.CreateConventionModelBuilder();
            builder.EntitySet<Cat>("Cats").EntityType.Collection.Function("BestCat").Returns<string>();
            builder.EntitySet<Owner>("Owners").EntityType.Collection.Function("BestOwner").Returns<string>();
            config.MapODataServiceRoute("Cat", "cat", builder.GetEdmModel());
        }

        [Theory]
        [InlineData("/dog/Dogs")]
        [InlineData("/dog/Dogs/Default.BestDog")]
        [InlineData("/dog/Owners")]
        [InlineData("/dog/Owners/Default.BestOwner")]
        [InlineData("/cat/Cats")]
        [InlineData("/cat/Cats/Default.BestCat")]
        [InlineData("/cat/Owners")]
        [InlineData("/cat/Owners/Default.BestOwner")]
        public async Task CanSupportMultipleAttributesRoutes(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + url);
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
        }
    }

    public class DogsController : TestODataController
    {
        private static IList<Dog> dogs = Enumerable.Range(1, 5)
            .Select(e => { return new Dog() { Id = e, Name = "Dog " + e.ToString() }; })
            .ToList();

        public ITestActionResult Get()
        {
            return Ok(dogs);
        }

        [HttpGet]
        [ODataRoute("Dogs/Default.BestDog", RouteName = "Dog")]
        public ITestActionResult WhoIsTheBestDog()
        {
            return Ok(dogs.First().Name);
        }
    }

    public class CatsController : TestODataController
    {
        private static IList<Cat> cats = Enumerable.Range(1, 5)
            .Select(e => { return new Cat() { Id = e, Name = "Cat " + e.ToString() }; })
            .ToList();

        public ITestActionResult Get()
        {
            return Ok(cats);
        }

        [HttpGet]
        [ODataRoute("Cats/Default.BestCat", RouteName = "Cat")]
        public ITestActionResult WhoIsTheBestCat()
        {
            return Ok(cats.First().Name);
        }
    }

    public class OwnersController : TestODataController
    {
        private static IList<Owner> owners = Enumerable.Range(1, 5)
            .Select(e => { return new Owner() { Id = e, Name = "Owner " + e.ToString() }; })
            .ToList();

        public ITestActionResult Get()
        {
            return Ok(owners);
        }

        [HttpGet]
        [ODataRoute("Owners/Default.BestOwner")]
        public ITestActionResult WhoIsTheBestOwner()
        {
            return Ok(owners.First().Name);
        }
    }

    public class Dog
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Cat
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
