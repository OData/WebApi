//-----------------------------------------------------------------------------
// <copyright file="TransformedPropertyPortTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Test.Abstraction;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using Xunit;
using Customer = Microsoft.AspNet.OData.Test.Query.ApplyQueryOptionTest.TransformedOpenCustomer;
using Address = Microsoft.AspNet.OData.Test.Query.ApplyQueryOptionTest.TransformedOpenAddress;
using Profile = Microsoft.AspNet.OData.Test.Query.ApplyQueryOptionTest.TransformedOpenProfile;

namespace Microsoft.AspNet.OData.Test.Query
{
    public class TransformedPropertyPortTests
    {
        [Theory]
        [InlineData("compute(Id as ItemId)/compute(substring(Instance/HiddenName,0,20) as Result)", "[null,null,null,null]")]
        [InlineData("compute(Id as ItemId)/compute(substring(Instance/Profile/Label,0,20) as Result)", "[null,null,null,null]")]
        [InlineData("compute(Id as ItemId)/compute(Commission add 1 as Result)", "[251,191,371,311]")]
        [InlineData("compute(Id as ItemId)/compute(ItemId add 1 as NextId)/compute(Commission add NextId as Result)", "[252,193,374,320]")]
        [InlineData("compute(Id as ItemId)/compute(Instance/Address/Commission add 1 as Result)", "[11,21,31,41]")]
        public async Task ControllerComputeHonorsEdmBoundariesAndRetainsDynamicSources(string apply, string expected)
        {
            using (TestServer server = TestServerFactory.Create(new[] { typeof(TransformedCustomersController) }, routes =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(routes);
                var entity = builder.EntitySet<Customer>("TransformedCustomers").EntityType;
                entity.Ignore(customer => customer.HiddenName);
                entity.Ignore(customer => customer.Profile);
                builder.ComplexType<Address>();
                routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
            }))
            using (HttpClient client = server.CreateClient())
            using (HttpResponseMessage response = await client.GetAsync("/odata/TransformedCustomers?$apply=" + apply.Replace(" ", "%20")))
            {
                string body = await response.Content.ReadAsStringAsync();
                Assert.True(response.StatusCode == HttpStatusCode.OK, body);
                JObject result = JObject.Parse(body);
                Assert.True(JToken.DeepEquals(JArray.Parse(expected), new JArray(result["value"].Select(item => item["Result"]))), body);
            }
        }

        [Fact]
        public async Task ControllerGroupByDoesNotReadIgnoredClrPropertyAfterCompute()
        {
            using (TestServer server = TestServerFactory.Create(new[] { typeof(TransformedCustomersController) }, routes =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(routes);
                builder.EntitySet<Customer>("TransformedCustomers").EntityType.Ignore(customer => customer.HiddenName);
                routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
            }))
            using (HttpClient client = server.CreateClient())
            using (HttpResponseMessage response = await client.GetAsync("/odata/TransformedCustomers?$apply=compute(Id%20as%20ItemId)/groupby((Instance/HiddenName))"))
            {
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                JToken result = Assert.Single(JObject.Parse(await response.Content.ReadAsStringAsync())["value"]);
                Assert.Equal(JTokenType.Null, result["HiddenName"].Type);
            }
        }

        [Fact]
        public async Task ControllerRejectsWrapperClrPropertiesAfterGroupBy()
        {
            using (TestServer server = TestServerFactory.Create(new[] { typeof(TransformedCustomersController) }, routes =>
            {
                var builder = ODataConventionModelBuilderFactory.Create(routes);
                builder.EntitySet<Customer>("TransformedCustomers");
                routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
            }))
            using (HttpClient client = server.CreateClient())
            using (HttpResponseMessage response = await client.GetAsync("/odata/TransformedCustomers?$apply=groupby((Id))/groupby((Values))"))
            {
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                Assert.Contains("Values", await response.Content.ReadAsStringAsync());
            }
        }
    }

    public class TransformedCustomersController : TestODataController
    {
        [EnableQuery]
        public IQueryable<Customer> Get()
        {
            return new[]
            {
                new Customer { Id = 1, Name = "Nancy", HiddenName = "Nancy secret", Profile = new Profile { Label = "Nancy secret" }, Address = new Address { DynamicProperties = new Dictionary<string, object> { { "Commission", 10 } } }, DynamicProperties = new Dictionary<string, object> { { "Commission", 250 } } },
                new Customer { Id = 2, Name = "Andrew", HiddenName = "Andrew secret", Profile = new Profile { Label = "Andrew secret" }, Address = new Address { DynamicProperties = new Dictionary<string, object> { { "Commission", 20 } } }, DynamicProperties = new Dictionary<string, object> { { "Commission", 190 } } },
                new Customer { Id = 3, Name = "Janet", HiddenName = "Janet secret", Profile = new Profile { Label = "Janet secret" }, Address = new Address { DynamicProperties = new Dictionary<string, object> { { "Commission", 30 } } }, DynamicProperties = new Dictionary<string, object> { { "Commission", 370 } } },
                new Customer { Id = 9, Name = "Anne", HiddenName = "Anne secret", Profile = new Profile { Label = "Anne secret" }, Address = new Address { DynamicProperties = new Dictionary<string, object> { { "Commission", 40 } } }, DynamicProperties = new Dictionary<string, object> { { "Commission", 310 } } }
            }.AsQueryable();
        }
    }
}
