using Microsoft.Data.Edm;
using Nuwa;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.OData;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Routing
{
    [NuwaFramework]
    public class LinksRoutingConventionTests
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Routes.MapODataServiceRoute("odata", "", GetModel());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Customer> customers = builder.EntitySet<Customer>("Customers");
            EntityTypeConfiguration<VipCustomer> vipCustoemer = builder.Entity<VipCustomer>().DerivesFrom<Customer>();
            EntitySetConfiguration<Order> orders = builder.EntitySet<Order>("Orders");
            EntitySetConfiguration<Address> address = builder.EntitySet<Address>("Addresses");
            EntitySetConfiguration<PersonalInformation> information = builder.EntitySet<PersonalInformation>("Informations");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("POST", "/Customers(5)/$links/Orders")]
        [InlineData("PUT", "/Customers(5)/$links/Orders")]
        [InlineData("POST", "/Customers(5)/WebStack.QA.Test.OData.Routing.VipCustomer/$links/Orders")]
        [InlineData("PUT", "/Customers(5)/WebStack.QA.Test.OData.Routing.VipCustomer/$links/Orders")]
        [InlineData("POST", "/Addresses(5)/$links/VipCustomer")]
        [InlineData("PUT", "/Addresses(5)/$links/VipCustomer")]
        [InlineData("POST", "/Customers(5)/$links/Information")]
        [InlineData("PUT", "/Customers(5)/$links/Information")]
        [InlineData("POST", "/Customers(5)/WebStack.QA.Test.OData.Routing.VipCustomer/$links/Addresses")]
        [InlineData("PUT", "/Customers(5)/WebStack.QA.Test.OData.Routing.VipCustomer/$links/Addresses")]
        public void CreateLinksRoutingConventionWorks(string method, string url)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), BaseAddress + url);
            request.Content = new StringContent("{ url: \"http://localhost:12345/Orders(25)\"}");
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory]
        [InlineData("/Customers(5)/$links/Orders(25)")]
        [InlineData("/Orders(25)/$links/Customer")]
        [InlineData("/Customers(5)/WebStack.QA.Test.OData.Routing.VipCustomer/$links/Addresses(25)")]
        [InlineData("/Addresses(25)/$links/VipCustomer")]
        [InlineData("/Customers(5)/$links/Information")]
        public void DeleteLinksRoutingConventionWorks(string url)
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
        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateLinkToVipCustomer([FromODataUri] int key, [FromBody] Uri link)
        {
            if (key == 5 && link.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        public IHttpActionResult DeleteLinkToVipCustomer([FromODataUri] int key)
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
        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateLinkToOrders([FromODataUri] int key, [FromBody] Uri link)
        {
            if (key == 5 && link.ToString().Equals("http://localhost:12345/Orders(25)"))
            {
                return StatusCode(HttpStatusCode.OK);
            }
            return BadRequest();
        }

        public IHttpActionResult DeleteLinkToOrders([FromODataUri] int key, string relatedKey)
        {
            if (key == 5 && relatedKey.Equals("25"))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }

        public IHttpActionResult DeleteLinkToAddressesFromVipCustomer([FromODataUri] int key, string relatedKey)
        {
            if (key == 5 && relatedKey.Equals("25"))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }

        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateLinkToAddressesFromVipCustomer([FromODataUri] int key, [FromBody] Uri link)
        {
            if (key == 5 && link.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        [AcceptVerbs("POST", "PUT")]
        public IHttpActionResult CreateLink([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            if (key == 5 && navigationProperty == "Information" && link.ToString() == "http://localhost:12345/Orders(25)")
            {
                return Ok();
            }
            return BadRequest();
        }

        public IHttpActionResult DeleteLink([FromODataUri] int key, string navigationProperty)
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
        public IHttpActionResult DeleteLinkToCustomer([FromODataUri] int key)
        {
            if (key == 25)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest();
        }
    }
}
