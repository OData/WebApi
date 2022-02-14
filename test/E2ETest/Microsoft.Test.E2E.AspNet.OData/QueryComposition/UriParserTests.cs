//-----------------------------------------------------------------------------
// <copyright file="UriParserTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public class UriParser_Model1
    {
        public int Id { get; set; }
        public UriParser_Model1 Self { get; set; }
    }

    public class UriParserTests : WebHostTestBase
    {
        public UriParserTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            mb.EntitySet<UriParser_Model1>("UriParser_Model1");
            return mb.GetEdmModel();
        }

        [Fact]
        public async Task TestDeepNestedUri()
        {
            var url = new AttackStringBuilder().Append("/UriParser_Model1(0)/").Repeat("Self/", 150).ToString();
            var response = await this.Client.GetAsync(this.BaseAddress + url);
        }
    }
}
