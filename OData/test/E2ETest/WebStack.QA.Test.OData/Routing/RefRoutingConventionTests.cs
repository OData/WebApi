using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Core.UriParser.Metadata;
using Microsoft.OData.Edm;
using Nuwa;
using WebStack.QA.Test.OData.Common;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Routing
{
    [NuwaFramework]
    public class RefRoutingConventionTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            var controllers = new[] { typeof(CustomersController), typeof(OrdersController), typeof(AddressesController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));

            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            config.Services.Replace(typeof(IAssembliesResolver), resolver);

            config.SetUriResolver(new ODataUriResolver { EnableCaseInsensitive = true });

            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Customer>("Customers");
            builder.EntityType<VipCustomer>().DerivesFrom<Customer>();
            builder.EntitySet<Order>("Orders");
            builder.EntitySet<Address>("Addresses");
            builder.EntitySet<PersonalInformation>("Informations");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("POST", "/Customers(5)/Orders/$ref")]
        [InlineData("POST", "/Customers(5)/WebStack.QA.Test.OData.Routing.VipCustomer/Orders/$ref")]
        [InlineData("PUT", "/Addresses(5)/VipCustomer/$ref")]
        [InlineData("PUT", "/Customers(5)/Information/$ref")]
        [InlineData("POST", "/Customers(5)/WebStack.QA.Test.OData.Routing.VipCustomer/Addresses/$ref")]
        public void CreateRefRoutingConventionWorks(string method, string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), BaseAddress + url);
            request.Content = new StringContent("{ \"@odata.id\" : \"http://localhost:12345/Orders(25)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/Customers(5)/Orders(25)/$ref")]
        [InlineData("/Orders(25)/Customer/$ref")]
        [InlineData("/Customers(5)/WebStack.QA.Test.OData.Routing.VipCustomer/Addresses(25)/$ref")]
        [InlineData("/Addresses(25)/VipCustomer/$ref")]
        [InlineData("/Customers(5)/Information/$ref")]
        public void DeleteRefRoutingConventionWorks(string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("DELETE"), BaseAddress + url);
            HttpResponseMessage response = Client.SendAsync(request).Result;
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

    public class AddressesController : ODataController
    {
        [AcceptVerbs("PUT")]
        public IHttpActionResult CreateRefToVipCustomer([FromODataUri] int key, [FromBody] Uri reference)
        {
            if (key == 5 && reference.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        public IHttpActionResult DeleteRefToVipCustomer([FromODataUri] int key)
        {
            if (key == 25)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }
    }

    public class CustomersController : ODataController
    {
        [AcceptVerbs("POST")]
        public IHttpActionResult CreateRefToOrders([FromODataUri] int key, [FromBody] Uri reference)
        {
            if (key == 5 && reference.ToString().Equals("http://localhost:12345/Orders(25)"))
            {
                return StatusCode(HttpStatusCode.OK);
            }
            return BadRequest();
        }

        public IHttpActionResult DeleteRefToOrders([FromODataUri] int key, string relatedKey)
        {
            if (key == 5 && relatedKey.Equals("25"))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }

        public IHttpActionResult DeleteRefToAddressesFromVipCustomer([FromODataUri] int key, string relatedKey)
        {
            if (key == 5 && relatedKey.Equals("25"))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }

        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateRefToAddressesFromVipCustomer([FromODataUri] int key, [FromBody] Uri reference)
        {
            if (key == 5 && reference.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri reference)
        {
            if (key == 5 && navigationProperty == "Information" && reference.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        public IHttpActionResult DeleteRef([FromODataUri] int key, string navigationProperty)
        {
            if (key == 5 && navigationProperty == "Information")
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }
    }

    public class OrdersController : ODataController
    {
        public IHttpActionResult DeleteRefToCustomer([FromODataUri] int key)
        {
            if (key == 25)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }
    }
}
