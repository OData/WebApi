// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ODataOrderByTest
{
    public class ODataOrderByTest : WebHostTestBase<ODataOrderByTest>
    {
        public ODataOrderByTest(WebHostTestFixture<ODataOrderByTest> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            var controllers = new[] { typeof(ItemsController) };
            configuration.AddControllers(controllers);
            configuration.IncludeErrorDetail = true;

            configuration.Routes.Clear();
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);

            configuration.MapODataServiceRoute(
                routeName: "odata",
                routePrefix: "odata",
                model: OrderByEdmModel.GetModel(configuration));

            configuration.EnsureInitialized();
        }

        [Fact]
        public async Task TestStableOrder_WithCompositeKeyOrderedByColumnAttribute_UseColumnAttributeToDetermineTheKeyOrder()
        {   // Arrange
            await TestOrderedQuery<Item>("Items");
        }

        [Fact]
        public async Task TestStableOrder_WithCompositeKeyOrderedByColumnAttribute_UseColumnAttributeToDetermineTheKeyOrder2()
        {
            await TestOrderedQuery<Item2>("Items2");
        }

        [Fact]
        public async Task TestStableOrder_WithCompositeKeyOrderedByColumnAttribute_AndContainingEnums_UseColumnAttributeToDetermineTheKeyOrder()
        {
            await TestOrderedQuery<ItemWithEnum>("ItemsWithEnum");
        }

        [Fact]
        public async Task TestStableOrder_WithCompositeKeyNotOrdered_OrderTheKeyByPropertyName()
        {
            await TestOrderedQuery<ItemWithoutColumn>("ItemsWithoutColumn");
        }

        private async Task TestOrderedQuery<T>(string entitySet) where T : OrderedItem, new()
        {
            // Arrange
            var requestUri = $"{BaseAddress}/odata/{entitySet}?$top=10";
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            // Act
            var response = await Client.SendAsync(request);
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var rawResult = await response.Content.ReadAsStringAsync();
            var jsonResult = JObject.Parse(rawResult);
            var jsonValue = jsonResult.SelectToken("value");
            Assert.NotNull(jsonValue);
            var concreteResult = jsonValue.ToObject<List<T>>();
            Assert.NotEmpty(concreteResult);
            var expected = Enumerable.Range(1, 4).Select(i => new T() { ExpectedOrder = i }).ToList();
            Assert.Equal(expected, concreteResult, new OrderedItemComparer<T>());
        }

        private sealed class OrderedItemComparer<T> : IEqualityComparer<T> where T : OrderedItem
        {
            public bool Equals(T x, T y)
            {
                return x.ExpectedOrder == y.ExpectedOrder;
            }

            public int GetHashCode(T obj)
            {
                return obj.ExpectedOrder.GetHashCode();
            }
        }
    }
}