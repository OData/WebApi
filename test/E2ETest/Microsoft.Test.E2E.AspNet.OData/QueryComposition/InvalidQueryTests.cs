// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
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
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class InvalidQueryTests : WebHostTestBase<InvalidQueryTests>
    {
        public InvalidQueryTests(WebHostTestFixture<InvalidQueryTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.Count().Filter().OrderBy().Expand().MaxTop(null);
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<InvalidQueryCustomer> invalidQueryCustomers = builder.EntitySet<InvalidQueryCustomer>("InvalidQueryCustomers");
            return builder.GetEdmModel();
        }

        [Theory]
        [InlineData("/odata/InvalidQueryCustomers?$filter=xxid eq 5")]
        [InlineData("/odata/InvalidQueryCustomers(5)?$filter=xxid eq 5")]
        [InlineData("/odata/InvalidQueryCustomers?$orderby=xxid")]
        [InlineData("/odata/InvalidQueryCustomers(5)?$orderby=xxid asc")]
        [InlineData("/odata/InvalidQueryCustomers?$orderby=xxid desc")]
        public async Task ParseErrorsProduceMeaningfulMessages(string query)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = await Client.SendAsync(request);
            dynamic error = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("The query specified in the URI is not valid. Could not find a property named 'xxid' on type 'Microsoft.Test.E2E.AspNet.OData.QueryComposition.InvalidQueryCustomer'.",
                         (string)error["error"]["message"]);
        }

        [Theory]
        [InlineData("/odata/InvalidQueryCustomers(5)?$filter=id eq 5")]
        [InlineData("/odata/InvalidQueryCustomers(5)?$orderby=id asc")]
        public async Task ShouldThrow_CollectionRequiredForLastSegment(string query)
        {
            // Default ODataUriResolver with EnableCaseSensitive = true should be able parse the property 'id'
            // to valid edm property named 'Id'.
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = await Client.SendAsync(request);
            dynamic error = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.Equal("The query specified in the URI is not valid. The requested resource is not a collection. Query options $filter, $orderby, $count, $skip, and $top can be applied only on collections.",
                         (string)error["error"]["message"]);
        }

        [Theory]
        [InlineData("/odata/InvalidQueryCustomers?$filter=id eq 5")]
        [InlineData("/odata/InvalidQueryCustomers?$orderby=id")]
        [InlineData("/odata/InvalidQueryCustomers?$orderby=id desc")]
        public async Task ShouldWork_ParserEnableCaseInsensitiveByDefault(string query)
        {
            // Default ODataUriResolver with EnableCaseSensitive = true should be able to resolve name 'id'
            // to valid edm property named 'Id'.
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseAddress + query);
            HttpResponseMessage response = await Client.SendAsync(request);
            JObject jsonRsp = JObject.Parse(await response.Content.ReadAsStringAsync());
            Assert.NotNull(jsonRsp["value"]);
        }
    }

    public class InvalidQueryCustomersController : TestODataController
    {
        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public ITestActionResult Get()
        {
            return Ok(Enumerable.Range(0, 1).Select(i => new InvalidQueryCustomer { }).AsQueryable());
        }

        [EnableQuery(PageSize = 10, MaxExpansionDepth = 2)]
        public ITestActionResult Get([FromODataUri] int key)
        {
            return Ok(TestSingleResult.Create(Enumerable.Range(0, 1).Select(i => new InvalidQueryCustomer { }).AsQueryable()));
        }
    }

    public class InvalidQueryCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
