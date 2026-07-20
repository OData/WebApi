//-----------------------------------------------------------------------------
// <copyright file="DeltaNestedUpdatablePropertiesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Dynamic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.DeltaNestedUpdatableProperties
{
    public class DeltaNestedCustomer
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DeltaNestedAddress Address { get; set; }
    }

    public class DeltaNestedAddress
    {
        public string City { get; set; }
        public string State { get; set; }
        public string StreetAddress { get; set; }
        public int ZipCode { get; set; }
    }

    public class DeltaNestedCustomersController : TestODataController
    {
        // The customer whose nested Address is treated as server-protected: the controller removes
        // "Address" from the delta's UpdatableProperties so a PATCH cannot change the nested resource.
        private const int ProtectedNestedCustomerKey = 1;

        public ITestActionResult Patch([FromODataUri] int key, Delta<DeltaNestedCustomer> delta)
        {
            var original = CreateCustomer(key);

            if (key == ProtectedNestedCustomerKey)
            {
                delta.UpdatableProperties.Remove("Address");
            }

            delta.Patch(original);
            return Ok(original);
        }

        private static DeltaNestedCustomer CreateCustomer(int key)
        {
            return new DeltaNestedCustomer
            {
                Id = key,
                FirstName = "Bob",
                LastName = "Smith",
                Address = new DeltaNestedAddress
                {
                    City = "Redmond",
                    State = "WA",
                    StreetAddress = "21110 NE 44th St",
                    ZipCode = 98074
                }
            };
        }
    }

    public class DeltaNestedUpdatablePropertiesTests : WebHostTestBase
    {
        public DeltaNestedUpdatablePropertiesTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: GetEdmModel(configuration),
                pathHandler: new DefaultODataPathHandler(),
                routingConventions: ODataRoutingConventions.CreateDefault());
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<DeltaNestedCustomer>("DeltaNestedCustomers");
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task Patch_DoesNotApplyNestedResource_WhenNestedPropertyRemovedFromUpdatableProperties()
        {
            // Arrange - customer 1's nested Address is removed from UpdatableProperties by the controller.
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), this.BaseAddress + "/odata/DeltaNestedCustomers(1)");
            dynamic body = new ExpandoObject();
            body.FirstName = "Alice";
            body.Address = new { City = "Sammamish", StreetAddress = "23213 NE 15th Ct" };
            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, content);
            var result = JObject.Parse(content);
            // The structural property still flows through.
            Assert.Equal("Alice", (string)result["FirstName"]);
            // The nested resource is left exactly as it was; the delta's Address is not copied.
            Assert.Equal("Redmond", (string)result["Address"]["City"]);
            Assert.Equal("21110 NE 44th St", (string)result["Address"]["StreetAddress"]);
        }

        [Fact]
        public async Task Patch_AppliesNestedResource_WhenUpdatablePropertiesUntrimmed()
        {
            // Arrange - customer 2 is not protected; the default (untrimmed) behavior applies the nested Address.
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), this.BaseAddress + "/odata/DeltaNestedCustomers(2)");
            dynamic body = new ExpandoObject();
            body.FirstName = "Alice";
            body.Address = new { City = "Sammamish" };
            request.Content = new StringContent(JsonConvert.SerializeObject(body));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            // Act
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.True(response.IsSuccessStatusCode, content);
            var result = JObject.Parse(content);
            Assert.Equal("Alice", (string)result["FirstName"]);
            // The changed nested value is copied and the unchanged nested values are retained.
            Assert.Equal("Sammamish", (string)result["Address"]["City"]);
            Assert.Equal("WA", (string)result["Address"]["State"]);
        }
    }
}
