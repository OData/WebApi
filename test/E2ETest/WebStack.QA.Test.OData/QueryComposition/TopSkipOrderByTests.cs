// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Extensions;
using Nuwa;
using WebStack.QA.Common.XUnit;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Models.Products;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.QueryComposition
{
    public class TopSkipOrderByTests : ODataTestBase
    {
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

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [PropertyData("ActionNames")]
        public void TestTop(string actionName)
        {
            this.Client.Timeout = TimeSpan.FromDays(1);
            var response = this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$top=1").Result;
            var result = response.Content.ReadAsAsync<IEnumerable<Customer>>().Result;

            Assert.Equal(1, result.Count());
            Assert.Equal(1, result.First().Id);
        }

        [Theory]
        [PropertyData("ActionNames")]
        public void TestSkip(string actionName)
        {
            this.Client.Timeout = TimeSpan.FromDays(1);
            var response = this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$skip=1").Result;
            var result = response.Content.ReadAsAsync<IEnumerable<Customer>>().Result;

            Assert.Equal(2, result.Count());
            Assert.Equal(2, result.First().Id);
        }

        [Theory]
        [PropertyData("ActionNames")]
        public void TestOrderBy(string actionName)
        {
            this.Client.Timeout = TimeSpan.FromDays(1);
            var response = this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$orderby=Name").Result;
            var result = response.Content.ReadAsAsync<IEnumerable<Customer>>().Result;

            Assert.Equal(3, result.Count());
            Assert.Equal(2, result.First().Id);
            Assert.Equal("Jerry", result.First().Name);

            response = this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$orderby=Name desc").Result;
            result = response.Content.ReadAsAsync<IEnumerable<Customer>>().Result;

            Assert.Equal(1, result.First().Id);

            response = this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/" + actionName + "?$orderby=Id desc,Name desc").Result;
            result = response.Content.ReadAsAsync<IEnumerable<Customer>>().Result;

            Assert.Equal(2, result.First().Id);
            Assert.Equal("Mike", result.First().Name);
        }

        [Fact]
        public async Task TestOtherQueries()
        {
            var response = await Client.GetAsync(BaseAddress + "/api/TopSkipOrderByTests/GetODataQueryOptions?$skiptoken=abc&$expand=abc&$select=abc&$count=abc&$deltatoken=abc");
            var results = await response.Content.ReadAsAsync<Dictionary<string, string>[]>();
            var result = results[0];

            Assert.Equal("abc", result["SkipToken"]);
            Assert.Equal("abc", result["Expand"]);
            Assert.Equal("abc", result["Select"]);
            Assert.Equal("abc", result["Count"]);
            Assert.Equal("abc", result["DeltaToken"]);
        }
    }
}
