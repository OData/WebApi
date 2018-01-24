// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData.Builder;
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

        protected override void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
            configuration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        private static IEdmModel GetEdmModel(HttpConfiguration configuration)
        {
            var mb = new ODataConventionModelBuilder(configuration);
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
