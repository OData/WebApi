//-----------------------------------------------------------------------------
// <copyright file="DerivedTypesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DerivedTypes
{
    public class DerivedTypeTests : WebHostTestBase
    {
        public DerivedTypeTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(CustomersController), typeof(MetadataController) };
            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.MapODataServiceRoute("odata", "odata", GetEdmModel(configuration), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();

            builder.EntitySet<Customer>("Customers");
            builder.EntityType<Order>();
            builder.EntityType<VipCustomer>().DerivesFrom<Customer>();

            return builder.GetEdmModel();
        }

        [Fact]
        public async Task RestrictEntitySetToDerivedTypeInstances()
        {
            string requestUri = this.BaseAddress + "/odata/Customers/Microsoft.Test.E2E.AspNet.OData.DerivedTypes.VipCustomer";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            var expectedContent = "\"value\":[{\"Id\":2,\"Name\":\"Customer 2\",\"LoyaltyCardNo\":\"9876543210\"}]";

            Assert.Contains(expectedContent, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("Customers(2)/Microsoft.Test.E2E.AspNet.OData.DerivedTypes.VipCustomer")]
        [InlineData("Customers/Microsoft.Test.E2E.AspNet.OData.DerivedTypes.VipCustomer(2)")]
        public async Task RestrictEntityToDerivedTypeInstance(string path)
        {
            // Key preceeds name of the derived type
            string requestUri = this.BaseAddress + "/odata/" + path;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            var expectedContent = "\"Id\":2,\"Name\":\"Customer 2\",\"LoyaltyCardNo\":\"9876543210\"";

            Assert.Contains(expectedContent, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ReturnNotFound_ForKeyNotAssociatedWithDerivedType()
        {
            // Customer with Id 1 is not a VipCustomer
            string requestUri = this.BaseAddress + "/odata/Customers(1)/Microsoft.Test.E2E.AspNet.OData.DerivedTypes.VipCustomer";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.Client.SendAsync(request);
            Assert.False(response.IsSuccessStatusCode);
            Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task RestrictEntitySetToDerivedTypeInstances_ThenExpandNavProperty()
        {
            string requestUri = this.BaseAddress + "/odata/Customers/Microsoft.Test.E2E.AspNet.OData.DerivedTypes.VipCustomer?$expand=Orders";

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            var expectedContent = "\"value\":[{\"Id\":2,\"Name\":\"Customer 2\",\"LoyaltyCardNo\":\"9876543210\"," +
                "\"Orders\":[{\"Id\":2,\"Amount\":230},{\"Id\":3,\"Amount\":150}]}]";

            Assert.Contains(expectedContent, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("Customers(2)/Microsoft.Test.E2E.AspNet.OData.DerivedTypes.VipCustomer?$expand=Orders")]
        [InlineData("Customers/Microsoft.Test.E2E.AspNet.OData.DerivedTypes.VipCustomer(2)?$expand=Orders")]
        public async Task RestrictEntityToDerivedTypeInstance_ThenExpandNavProperty(string pathAndQuery)
        {
            // Key preceeds name of the derived type
            string requestUri = this.BaseAddress + "/odata/" + pathAndQuery;

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await this.Client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);

            var expectedContent = "\"Id\":2,\"Name\":\"Customer 2\",\"LoyaltyCardNo\":\"9876543210\"," +
                "\"Orders\":[{\"Id\":2,\"Amount\":230},{\"Id\":3,\"Amount\":150}]";

            Assert.Contains(expectedContent, await response.Content.ReadAsStringAsync());
        }
    }
}
