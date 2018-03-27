// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.Products;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class TopSkipOrderByTests : WebHostTestBase
    {
        public TopSkipOrderByTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        public static TheoryDataSet<string> ActionNames
        {
            get
            {
                return new TheoryDataSet<string>()
                {
                    "GetByQuerableAttribute",
                    "GetByODataQueryOptions",
                    "GetHttpResponseByQuerableAttribute",
                };
            }
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [MemberData(nameof(ActionNames))]
        public async Task TestTop(string actionName)
        {
            this.Client.Timeout = TimeSpan.FromDays(1);
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$top=1");
            var result = await response.Content.ReadAsObject<IEnumerable<Customer>>();

            Assert.Single(result);
            Assert.Equal(1, result.First().Id);
        }

        [Theory]
        [MemberData(nameof(ActionNames))]
        public async Task TestSkip(string actionName)
        {
            this.Client.Timeout = TimeSpan.FromDays(1);
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$skip=1");
            var result = await response.Content.ReadAsObject<IEnumerable<Customer>>();

            Assert.Equal(2, result.Count());
            Assert.Equal(2, result.First().Id);
        }

        [Theory]
        [MemberData(nameof(ActionNames))]
        public async Task TestOrderBy(string actionName)
        {
            this.Client.Timeout = TimeSpan.FromDays(1);
            var response = await this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$orderby=Name");
            var result = await response.Content.ReadAsObject<IEnumerable<Customer>>();

            Assert.Equal(3, result.Count());
            Assert.Equal(2, result.First().Id);
            Assert.Equal("Jerry", result.First().Name);

            response = await this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$orderby=Name desc");
            result = await response.Content.ReadAsObject<IEnumerable<Customer>>();

            Assert.Equal(1, result.First().Id);

            response = await this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$orderby=Id desc,Name desc");
            result = await response.Content.ReadAsObject<IEnumerable<Customer>>();

            Assert.Equal(2, result.First().Id);
            Assert.Equal("Mike", result.First().Name);
        }

        [Fact]
        public async Task TestOtherQueries()
        {
            var response = await Client.GetAsync(BaseAddress + "/api/TopSkipOrderByTests/GetODataQueryOptions?$skiptoken=abc&$expand=abc&$select=abc&$count=abc&$deltatoken=abc");
            var results = await response.Content.ReadAsObject<Dictionary<string, string>[]>();
            var result = results[0];

            Assert.Equal("abc", result["skipToken"]);
            Assert.Equal("abc", result["expand"]);
            Assert.Equal("abc", result["select"]);
            Assert.Equal("abc", result["count"]);
            Assert.Equal("abc", result["deltaToken"]);
        }
    }
}
