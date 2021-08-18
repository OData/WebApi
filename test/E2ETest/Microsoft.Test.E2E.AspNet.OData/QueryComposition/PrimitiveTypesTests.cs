//-----------------------------------------------------------------------------
// <copyright file="PrimitiveTypesTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Extensions;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class PrimitiveTypesController : TestNonODataController
    {
        private List<string> stringList = new List<string>();

        public PrimitiveTypesController()
        {
            stringList.Add("One");
            stringList.Add("Two");
            stringList.Add("Three");
        }

        public IQueryable<string> GetIQueryableOfString()
        {
            return stringList.AsQueryable();
        }

        [EnableQuery]
        public IEnumerable<string> GetIEnumerableOfString()
        {
            return stringList.AsEnumerable();
        }
    }

    public class PrimitiveTypesTests : WebHostTestBase
    {
        public PrimitiveTypesTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null);
            configuration.AddODataQueryFilter();
            configuration.EnableDependencyInjection();
        }

        [Theory]
        [InlineData("/api/PrimitiveTypes/GetIQueryableOfString?$skip=1&$top=1")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$skip=1&$top=1")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$filter=$it eq 'Two'")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$filter=indexof($it, 'Two') ne -1")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$orderby=$it desc&$top=1")]
        [InlineData("/api/PrimitiveTypes/GetIEnumerableOfString?$filter=$it gt 'Three'")]
        public async Task TestSkipAndTopOnString(string url)
        {
            var response = await this.Client.GetAsync(this.BaseAddress + url);
            response.EnsureSuccessStatusCode();
            var actual = await response.Content.ReadAsObject<IEnumerable<string>>();

            Assert.Equal("Two", actual.First());
        }
    }
}
