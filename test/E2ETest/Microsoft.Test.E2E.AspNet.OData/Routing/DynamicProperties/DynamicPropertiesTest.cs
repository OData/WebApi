// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Routing.DynamicProperties
{
    public class DynamicPropertiesTest : WebHostTestBase<DynamicPropertiesTest>
    {
        public DynamicPropertiesTest(WebHostTestFixture<DynamicPropertiesTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { 
                typeof(DynamicCustomersController),
                typeof(DynamicSingleCustomerController),
                typeof(MetadataController),
            };

            configuration.AddControllers(controllers);

            configuration.Routes.Clear();
            configuration.MapODataServiceRoute(routeName: "odata", routePrefix: "odata",
                model: GetEdmModel(configuration));

            configuration.EnsureInitialized();
        }

        [Theory]
        [InlineData("DynamicCustomers(1)/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty_1")]
        [InlineData("DynamicCustomers(2)/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount_2")]
        [InlineData("DynamicCustomers(3)/Order/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromOrder_3")]
        [InlineData("DynamicCustomers(4)/Microsoft.Test.E2E.AspNet.OData.Routing.DynamicProperties.DynamicVipCustomer/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty_4")]
        [InlineData("DynamicCustomers(5)/Microsoft.Test.E2E.AspNet.OData.Routing.DynamicProperties.DynamicVipCustomer/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount_5")]
        [InlineData("DynamicSingleCustomer/DynamicPropertyName", "DynamicPropertyName_GetDynamicProperty")]
        [InlineData("DynamicSingleCustomer/Account/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromAccount")]
        [InlineData("DynamicSingleCustomer/Order/DynamicPropertyName", "DynamicPropertyName_GetDynamicPropertyFromOrder")]
        [InlineData("DynamicCustomers(1)/Id", "Id_1")]
        public async Task AccessPropertyTest(string uri, string expected)
        {
            string requestUri = string.Format("{0}/odata/{1}", BaseAddress, uri);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(expected, await response.Content.ReadAsStringAsync());
        }

        [Theory]
        [InlineData("Put")]
        [InlineData("Patch")]
        [InlineData("Post")]
        [InlineData("Delete")]
        public async Task AccessDynamicPropertyWithWrongMethodTest(string method)
        {
            string requestUri = string.Format("{0}/odata/DynamicCustomers(1)/DynamicPropertyName", BaseAddress);

            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), requestUri);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("DynamicCustomers(2)/SecondAccount/DynamicPropertyName")]
        [InlineData("DynamicSingleCustomer/SecondAccount/DynamicPropertyName")]
        public async Task AccessDynamicPropertyWithoutImplementMethod(string uri)
        {
            string requestUri = string.Format("{0}/odata/{1}", BaseAddress, uri);

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            builder.EntitySet<DynamicCustomer>("DynamicCustomers");
            builder.Singleton<DynamicSingleCustomer>("DynamicSingleCustomer");
            return builder.GetEdmModel();
        }
    }
}
