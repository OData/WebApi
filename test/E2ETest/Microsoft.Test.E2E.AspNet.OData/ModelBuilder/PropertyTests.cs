// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class PropertyTestsUsingConventionModelBuilder : WebHostTestBase
    {
        public PropertyTestsUsingConventionModelBuilder(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration config)
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
        public async Task ConventionModelBuilderIgnoresPropertyWhenTold()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/PropertyCustomers(1)");
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            PropertyCustomer customer = await response.Content.ReadAsAsync<PropertyCustomer>(Enumerable.Range(0, 1).Select(f => new JsonMediaTypeFormatter()));
            Assert.NotNull(customer);
            Assert.Equal(1, customer.Id);
            Assert.Equal("Name 1", customer.Name);
            Assert.Null(customer.Secret);
        }
    }

    public class PropertyTestsUsingODataModelBuilder : WebHostTestBase
    {
        public PropertyTestsUsingODataModelBuilder(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration config)
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
        public async Task ODataModelBuilderIgnoresPropertyWhenTold()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + "/odata/PropertyCustomers(1)");
            HttpResponseMessage response = await Client.SendAsync(request);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            PropertyCustomer customer = await response.Content.ReadAsAsync<PropertyCustomer>(Enumerable.Range(0, 1).Select(f => new JsonMediaTypeFormatter()));
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
