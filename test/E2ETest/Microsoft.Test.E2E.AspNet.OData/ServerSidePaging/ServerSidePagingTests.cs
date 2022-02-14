//-----------------------------------------------------------------------------
// <copyright file="ServerSidePagingTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ServerSidePaging
{
    public class ServerSidePagingTests : WebHostTestBase
    {
        public ServerSidePagingTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Select().Filter().OrderBy().Expand().Count().MaxTop(null);
            // NOTE: Brackets in prefix to force a call into `RouteCollection`'s `GetVirtualPath`
            configuration.MapODataServiceRoute(
                routeName: "bracketsInPrefix",
                routePrefix: "{a}",
                model: GetEdmModel(configuration),
                pathHandler: new DefaultODataPathHandler(),
                routingConventions: ODataRoutingConventions.CreateDefault());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<ServerSidePagingOrder>("ServerSidePagingOrders").EntityType.HasRequired(d => d.ServerSidePagingCustomer);
            builder.EntitySet<ServerSidePagingCustomer>("ServerSidePagingCustomers").EntityType.HasMany(d => d.ServerSidePagingOrders);
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ValidNextLinksGenerated()
        {
            var requestUri = this.BaseAddress + "/prefix/ServerSidePagingCustomers?$expand=ServerSidePagingOrders";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Customer 1 => 6 Orders, Customer 2 => 5 Orders, Customer 3 => 4 Orders, ...
            // NextPageLink will be expected on the Customers collection as well as
            // the Orders child collection on Customer 1
            Assert.Contains("@odata.nextLink", content);
            Assert.Contains("/prefix/ServerSidePagingCustomers?$expand=ServerSidePagingOrders&$skip=5",
                content);
            // Orders child collection
            Assert.Contains("ServerSidePagingOrders@odata.nextLink", content);
            Assert.Contains("/prefix/ServerSidePagingCustomers(1)/ServerSidePagingOrders?$skip=5",
                content);
        }
    }
}
