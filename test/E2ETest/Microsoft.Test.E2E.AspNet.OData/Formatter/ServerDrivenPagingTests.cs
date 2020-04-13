// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter
{
    public class ServerDrivenPaging_ProductsController : InMemoryODataController<Product, int>
    {
        public ServerDrivenPaging_ProductsController()
            : base("ID")
        {
            if (this.LocalTable.Count == 0)
            {
                for (int i = 0; i < 100; i++)
                {
                    this.LocalTable.TryAdd(i, new Product
                    {
                        ID = i,
                        Name = "Test " + i,
                    });
                }
            }
        }

        [EnableQuery(PageSize = 10)]
        public override System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Product>> Get()
        {
            return base.Get();
        }
    }

    public class ServerDrivenPagingTests : WebHostTestBase<ServerDrivenPagingTests>
    {
        public ServerDrivenPagingTests(WebHostTestFixture<ServerDrivenPagingTests> fixture)
            :base(fixture)
        {
        }

        protected static void UpdateConfigure(WebRouteConfiguration configuration)
        {
            configuration.RemoveNonODataFormatters();
            configuration.EnableODataSupport(GetEdmModel(configuration));
        }

        protected static IEdmModel GetEdmModel(WebRouteConfiguration configuration)
        {
            var mb = configuration.CreateConventionModelBuilder();
            var product = mb.EntitySet<Product>("ServerDrivenPaging_Products").EntityType;
            product.Ignore(p => p.Family);
            return mb.GetEdmModel();
        }

        // [Fact]
        public async Task VerifyNextPageLinkAndInlineCountGeneratedCorrect()
        {
            // Arrange & Act
            var result = await this.Client.GetStringAsync(this.BaseAddress + "/ServerDrivenPaging_Products?$inlinecount=allPages");

            // Assert
            Assert.Contains("\"@odata.count\":100", result);
            Assert.Contains("\"@odata.nextLink\":", result);
            Assert.Contains("/ServerDrivenPaging_Products?$inlinecount=allPages&$skip=10", result);
        }
    }
}
