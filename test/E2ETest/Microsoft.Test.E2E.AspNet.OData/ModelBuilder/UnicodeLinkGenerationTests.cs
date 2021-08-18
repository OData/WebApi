//-----------------------------------------------------------------------------
// <copyright file="UnicodeLinkGenerationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class UnicodeLinkGeneration_Products : InMemoryODataController<Product, int>
    {
        public UnicodeLinkGeneration_Products()
            : base("ID")
        {
        }
    }

    public class UnicodeLinkGenerationTests : WebHostTestBase
    {
        public UnicodeLinkGenerationTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

            configuration.EnableODataSupport(GetImplicitEdmModel(configuration));
        }

        private static IEdmModel GetImplicitEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder modelBuilder = configuration.CreateConventionModelBuilder();
            modelBuilder.EntitySet<Product>("UnicodeLinkGeneration_Products");

            return modelBuilder.GetEdmModel();
        }
    }
}
