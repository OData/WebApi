using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Nuwa;
using WebStack.QA.Common.WebHost;
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
            //configuration.SetEdmModel(Svc.EdmModelHelper.GetEdmModel());
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
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
        public void TestOtherQueries()
        {
            var response = this.Client.GetAsync(this.BaseAddress + "/api/TopSkipOrderByTests/GetODataQueryOptions?$skiptoken=abc&$expand=abc&$select=abc&$inlinecount=abc").Result;
            var result = response.Content.ReadAsAsync<Dictionary<string, string>[]>().Result[0];
            Assert.Equal("abc", result["SkipToken"]);
            Assert.Equal("abc", result["Expand"]);
            Assert.Equal("abc", result["Select"]);
            Assert.Equal("abc", result["InlineCount"]);
        }
    }
}
