//-----------------------------------------------------------------------------
// <copyright file="MultipleEntitySetOnSameClrTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class MultipleEntitySetOnSameClrType_Products1Controller : InMemoryODataController<Product, int>
    {
        bool initialized = false;

        public MultipleEntitySetOnSameClrType_Products1Controller()
            : base("ID")
        {
            if (!initialized)
            {
                LocalTable.AddOrUpdate(
                    1,
                    new Product
                    {
                        ID = 1,
                        Name = "Product 1"
                    }, (key, oldEntity) => oldEntity);

                LocalTable.AddOrUpdate(
                    2,
                    new Product
                    {
                        ID = 2,
                        Name = "Product 2"
                    },
                    (key, oldEntity) => oldEntity);

                initialized = true;
            }
        }
    }

    public class MultipleEntitySetOnSameClrType_Products2Controller : InMemoryODataController<Product, int>
    {
        public MultipleEntitySetOnSameClrType_Products2Controller()
            : base("ID")
        {
        }
    }

    public class MultipleEntitySetOnSameClrTypeTests : WebHostTestBase
    {
        public MultipleEntitySetOnSameClrTypeTests(WebHostTestFixture fixture)
            : base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.JsonReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            configuration.Count().Filter().OrderBy().Expand().MaxTop(null).Select();
            configuration.EnableODataSupport(GetImplicitEdmModel(configuration));
        }

        private static IEdmModel GetImplicitEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder modelBuilder = configuration.CreateConventionModelBuilder();
            modelBuilder.EntitySet<Product>("MultipleEntitySetOnSameClrType_Products1").EntityType.Ignore(p => p.Family);
            modelBuilder.EntitySet<Product>("MultipleEntitySetOnSameClrType_Products2").EntityType.Ignore(p => p.Family);

            var model = modelBuilder.GetEdmModel();
            return model;
        }

        [Fact]
        public async Task QueryableShouldWork()
        {
            var response = await this.Client.GetAsync(this.BaseAddress + "/MultipleEntitySetOnSameClrType_Products1?$top=1");
            response.EnsureSuccessStatusCode();

            response = await this.Client.GetAsync(this.BaseAddress + "/MultipleEntitySetOnSameClrType_Products2?$top=1");
            response.EnsureSuccessStatusCode();
        }
    }
}
