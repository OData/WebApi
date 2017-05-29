using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Nuwa;
using Xunit;

namespace WebStack.QA.Test.OData.ModelBuilder
{
    [NuwaFramework]
    public class PropertyTestsUsingConventionModelBuilder
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            var customers = builder.EntitySet<PropertyCustomer>("PropertyCustomers");
            customers.EntityType.Ignore(p => p.Secret);
            return builder.GetEdmModel();
        }

        [Fact]
        public void ConventionModelBuilderIgnoresPropertyWhenTold()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/PropertyCustomers(1)");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            PropertyCustomer customer = response.Content.ReadAsAsync<PropertyCustomer>(Enumerable.Range(0, 1).Select(f => new JsonMediaTypeFormatter())).Result;
            Assert.NotNull(customer);
            Assert.Equal(1, customer.Id);
            Assert.Equal("Name 1", customer.Name);
            Assert.Null(customer.Secret);
        }
    }

    [NuwaFramework]
    public class PropertyTestsUsingODataModelBuilder
    {
        [NuwaBaseAddress]
        public string BaseAddress { get; set; }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var customers = builder.EntitySet<PropertyCustomer>("PropertyCustomers");
            customers.EntityType.HasKey(x => x.Id);
            customers.HasIdLink(c => new Uri("http://localhost:12345"), true);
            customers.EntityType.Property(p => p.Name);
            return builder.GetEdmModel();
        }

        [Fact]
        public void ODataModelBuilderIgnoresPropertyWhenTold()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/PropertyCustomers(1)");
            HttpResponseMessage response = Client.SendAsync(request).Result;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            PropertyCustomer customer = response.Content.ReadAsAsync<PropertyCustomer>(Enumerable.Range(0, 1).Select(f => new JsonMediaTypeFormatter())).Result;
            Assert.NotNull(customer);
            Assert.Equal(1, customer.Id);
            Assert.Equal("Name 1", customer.Name);
            Assert.Null(customer.Secret);
        }
    }

    public class PropertyCustomer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Secret { get; set; }
    }


    public class PropertyCustomersController : ODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            return Ok(new PropertyCustomer { Id = 1, Name = "Name " + 1, Secret = "Secret " + 1 });
        }
    }
}
