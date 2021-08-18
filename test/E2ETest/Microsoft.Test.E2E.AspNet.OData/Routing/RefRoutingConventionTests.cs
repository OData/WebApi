//-----------------------------------------------------------------------------
// <copyright file="RefRoutingConventionTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.UriParserExtension;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Routing
{
    public class RefRoutingConventionTests : WebHostTestBase
    {
        public RefRoutingConventionTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            var controllers = new[] { typeof(CustomersController), typeof(OrdersController), typeof(AddressesController) };
            config.AddControllers(controllers);

            config.Routes.Clear();

            config.MapODataServiceRoute("odata", "",
                builder =>
                    builder.AddService(ServiceLifetime.Singleton, sp => GetModel(config))
                        .AddService<IEnumerable<IODataRoutingConvention>>(ServiceLifetime.Singleton, sp =>
                            ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config))
                        .AddService(ServiceLifetime.Singleton, sp => new CaseInsensitiveResolver()));
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntityType<VipCustomer>().DerivesFrom<Customer>();
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Address>("Addresses");
            builder.EntitySet<PersonalInformation>("Informations");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("POST", "/Customers(5)/Orders/$ref")]
        [InlineData("POST", "/Customers(5)/Microsoft.Test.E2E.AspNet.OData.Routing.VipCustomer/Orders/$ref")]
        [InlineData("PUT", "/Addresses(5)/VipCustomer/$ref")]
        [InlineData("PUT", "/Customers(5)/Information/$ref")]
        [InlineData("POST", "/Customers(5)/Microsoft.Test.E2E.AspNet.OData.Routing.VipCustomer/Addresses/$ref")]
        public async Task CreateRefRoutingConventionWorks(string method, string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), BaseAddress + url);
            request.Content = new StringContent("{ \"@odata.id\" : \"http://localhost:12345/Orders(25)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/Customers(5)/Orders(25)/$ref")]
        [InlineData("/Orders(25)/Customer/$ref")]
        [InlineData("/Customers(5)/Microsoft.Test.E2E.AspNet.OData.Routing.VipCustomer/Addresses(25)/$ref")]
        [InlineData("/Addresses(25)/VipCustomer/$ref")]
        [InlineData("/Customers(5)/Information/$ref")]
        public async Task DeleteRefRoutingConventionWorks(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("DELETE"), BaseAddress + url);
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }

    public class PersonalInformation
    {
        public int Id { get; set; }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Order> Orders { get; set; }
        public PersonalInformation Information { get; set; }
    }

    public class Order
    {
        public int Id { get; set; }
        public Customer Customer { get; set; }
    }

    public class VipCustomer : Customer
    {
        public IList<Address> Addresses { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public VipCustomer VipCustomer { get; set; }
    }

    public class AddressesController : TestODataController
    {
        [AcceptVerbs("PUT")]
        public ITestActionResult CreateRefToVipCustomer([FromODataUri] int key, [FromBody] Uri reference)
        {
            if (key == 5 && reference.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        public ITestActionResult DeleteRefToVipCustomer([FromODataUri] int key)
        {
            if (key == 25)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }
    }

    public class CustomersController : TestODataController
    {
        [AcceptVerbs("POST")]
        public ITestActionResult CreateRefToOrders([FromODataUri] int key, [FromBody] Uri reference)
        {
            if (key == 5 && reference.ToString().Equals("http://localhost:12345/Orders(25)"))
            {
                return StatusCode(HttpStatusCode.OK);
            }
            return BadRequest();
        }

        public ITestActionResult DeleteRefToOrders([FromODataUri] int key, string relatedKey)
        {
            if (key == 5 && relatedKey.Equals("25"))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }

        public ITestActionResult DeleteRefToAddressesFromVipCustomer([FromODataUri] int key, string relatedKey)
        {
            if (key == 5 && relatedKey.Equals("25"))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }

        [AcceptVerbs("POST", "PUT")]
        public ITestActionResult CreateRefToAddressesFromVipCustomer([FromODataUri] int key, [FromBody] Uri reference)
        {
            if (key == 5 && reference.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        [AcceptVerbs("POST", "PUT")]
        public ITestActionResult CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri reference)
        {
            if (key == 5 && navigationProperty == "Information" && reference.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        public ITestActionResult DeleteRef([FromODataUri] int key, string navigationProperty)
        {
            if (key == 5 && navigationProperty == "Information")
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }
    }

    public class OrdersController : TestODataController
    {
        public ITestActionResult DeleteRefToCustomer([FromODataUri] int key)
        {
            if (key == 25)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }
    }
}
