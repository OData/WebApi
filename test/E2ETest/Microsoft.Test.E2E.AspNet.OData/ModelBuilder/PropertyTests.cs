// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Xunit;
#else
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Xunit;
#endif

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class PropertyTestsUsingConventionModelBuilder : WebHostTestBase<PropertyTestsUsingConventionModelBuilder>
    {
        public PropertyTestsUsingConventionModelBuilder(WebHostTestFixture<PropertyTestsUsingConventionModelBuilder> fixture)
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
            PropertyCustomer customer = await response.Content.ReadAsObject<PropertyCustomer>();
            Assert.NotNull(customer);
            Assert.Equal(1, customer.Id);
            Assert.Equal("Name 1", customer.Name);
            Assert.Null(customer.Secret);
        }
    }

    public class PropertyTestsUsingODataModelBuilder : WebHostTestBase<PropertyTestsUsingODataModelBuilder>
    {
        public PropertyTestsUsingODataModelBuilder(WebHostTestFixture<PropertyTestsUsingODataModelBuilder> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
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
            PropertyCustomer customer = await response.Content.ReadAsObject<PropertyCustomer>();
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


    public class PropertyCustomersController : TestODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public ITestActionResult Get([FromODataUri] int key)
        {
            return Ok(new PropertyCustomer { Id = 1, Name = "Name " + 1, Secret = "Secret " + 1 });
        }
    }
}
