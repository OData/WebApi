// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ODataOrderByTest
{
    public class ODataOrderByTest : WebHostTestBase
    {
        public ODataOrderByTest(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            var controllers = new[] { typeof(ItemsController) };
            TestAssemblyResolver resolver = new TestAssemblyResolver(new TypesInjectionAssembly(controllers));
            configuration.Services.Replace(typeof(IAssembliesResolver), resolver);

            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            configuration.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: OrderByEdmModel.GetModel());

            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task TestOrderByResultItem()
        {   // Arrange
            var requestUri = string.Format("{0}/odata/Items", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResult = await response.Content.ReadAsStringAsync();
            var jsonResult = JObject.Parse(rawResult);
            var jsonValue = jsonResult.SelectToken("value");
            Assert.NotNull(jsonValue);
            var concreteResult = jsonValue.ToObject<List<Item>>();
            Assert.NotEmpty(concreteResult);
            for (var i = 0; i < concreteResult.Count - 1; i++)
            {
                var value = string.Format("#{0}", i + 1);
                Assert.True(concreteResult[i].Name.StartsWith(value), "Incorrect order.");
            }
        }

        [Fact]
        public async Task TestOrderByResultItem2()
        {   // Arrange
            var requestUri = string.Format("{0}/odata/Items2", BaseAddress);
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await Client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResult = await response.Content.ReadAsStringAsync();
            var jsonResult = JObject.Parse(rawResult);
            var jsonValue = jsonResult.SelectToken("value");
            Assert.NotNull(jsonValue);
            var concreteResult = jsonValue.ToObject<List<Item2>>();
            Assert.NotEmpty(concreteResult);
            for (var i = 0; i < concreteResult.Count - 1; i++)
            {
                var value = string.Format("#{0}", i + 1);
                Assert.True(concreteResult[i].Name.StartsWith(value), "Incorrect order.");
            }
        }
    }
}