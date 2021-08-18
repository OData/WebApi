//-----------------------------------------------------------------------------
// <copyright file="ConditionalLinkGenerationTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using Microsoft.Test.E2E.AspNet.OData.Common;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class ConditionalLinkGeneration_ProductsController : InMemoryODataController<Product, int>
    {
        bool initialized = false;

        public ConditionalLinkGeneration_ProductsController()
            : base("ID")
        {
            if (!initialized)
            {
                LocalTable.AddOrUpdate(1, new Product
                {
                    ID = 1,
                    Name = "Product 1",
                    Family = new ProductFamily
                    {
                        ID = 1,
                        Name = "Product Family 1",
                        Supplier = new Supplier
                        {
                            ID = 1,
                            Name = "Supplier 1"
                        }
                    }
                },
                (key, oldEntity) => oldEntity);

                initialized = true;
            }
        }
    }

    public class ConditionalLinkGeneration_SuppliersController : InMemoryODataController<Supplier, int>
    {
        public ConditionalLinkGeneration_SuppliersController()
            : base("ID")
        {
        }
    }

    public class ConditionalLinkGeneration_ProductFamiliesController : InMemoryODataController<ProductFamily, int>
    {
        public ConditionalLinkGeneration_ProductFamiliesController()
            : base("ID")
        {
        }
    }

    public class ConditionalLinkGeneration_ConventionModelBuilder_Tests : WebHostTestBase
    {
        public ConditionalLinkGeneration_ConventionModelBuilder_Tests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration configuration)
        {
            configuration.EnableODataSupport(GetImplicitEdmModel(configuration));
        }

        private static Microsoft.OData.Edm.IEdmModel GetImplicitEdmModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder modelBuilder = configuration.CreateConventionModelBuilder();
            var products = modelBuilder.EntitySet<Product>("ConditionalLinkGeneration_Products");
            products.HasEditLink(
               ctx => { return (Uri)null; },
               false);
            products.HasReadLink(
                ctx =>
                {
                    object id;
                    ctx.EdmObject.TryGetPropertyValue("ID", out id);
                    return new Uri(ResourceContextHelper.CreateODataLink(ctx,
                                    new EntitySetSegment(ctx.NavigationSource as IEdmEntitySet),
                                    new KeySegment(new[] {new KeyValuePair<string, object>("Id", id)}, ctx.StructuredType as IEdmEntityType, null)));
                },
                true);

            // Navigation Property is not working because of bug: http://aspnetwebstack.codeplex.com/workitem/780
            //products.HasNavigationPropertiesLink(
            //    products.EntityType.NavigationProperties,
            //    (ctx, nav) => null,
            //    false);

            modelBuilder.EntitySet<Supplier>("ConditionalLinkGeneration_Suppliers");
            modelBuilder.EntitySet<ProductFamily>("ConditionalLinkGeneration_ProductFamilies");

            var model = modelBuilder.GetEdmModel();
            return model;
        }

       // [Fact(Skip="when we compute navigationlink and association link, if readlink is set but editlink is not set, it will throw null exception")]
        public async Task EditLinkWithNullValueShouldResultInNoEditLinkinPayload()
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.BaseAddress + "/ConditionalLinkGeneration_Products(1)/");
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json;odata.metadata=full"));
            var response = await this.Client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("<link rel=\"edit\"", content);
            Assert.Contains("<link rel=\"self\"", content);
        }
    }
}
