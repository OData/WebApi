//-----------------------------------------------------------------------------
// <copyright file="ActionMetadataTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using Xunit;

namespace Microsoft.Test.E2E.AspNet.OData.ModelBuilder
{
    public class ActionMetadataTests : WebHostTestBase
    {
        public ActionMetadataTests(WebHostTestFixture fixture)
            :base(fixture)
        {
        }

        protected override void UpdateConfiguration(WebRouteConfiguration config)
        {
            config.Routes.Clear();
            config.MapODataServiceRoute("odata", "odata", GetModel(config), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefault());
        }

        private static IEdmModel GetModel(WebRouteConfiguration config)
        {
            ODataModelBuilder builder = config.CreateConventionModelBuilder();
            EntitySetConfiguration<ActionProduct> products = builder.EntitySet<ActionProduct>("Products");
            ActionConfiguration productsByCategory = products.EntityType.Action("GetProductsByCategory");
            ActionConfiguration getSpecialProduct = products.EntityType.Action("GetSpecialProduct");
            productsByCategory.ReturnsCollectionFromEntitySet<ActionProduct>(products);
            getSpecialProduct.ReturnsFromEntitySet<ActionProduct>(products);
            return builder.GetEdmModel();
        }

        [Fact]
        public async Task ProvideOverloadToSupplyEntitySetConfiguration()
        {
            IEdmModel model = null;
            Stream stream = await Client.GetStreamAsync(BaseAddress + "/odata/$metadata");
            using (XmlReader reader = XmlReader.Create(stream))
            {
                model = CsdlReader.Parse(reader);
            }
            IEdmAction collection = model.FindDeclaredOperations("Default.GetProductsByCategory").SingleOrDefault() as IEdmAction;
            IEdmEntityType expectedReturnType = model.FindDeclaredType(typeof(ActionProduct).FullName) as IEdmEntityType;
            Assert.NotNull(expectedReturnType);
            Assert.NotNull(collection);
            Assert.True(collection.IsBound);
            Assert.NotNull(collection.ReturnType.AsCollection());
            Assert.NotNull(collection.ReturnType.AsCollection().ElementType().AsEntity());
            Assert.Equal(expectedReturnType, collection.ReturnType.AsCollection().ElementType().AsEntity().EntityDefinition());

            IEdmAction single = model.FindDeclaredOperations("Default.GetSpecialProduct").SingleOrDefault() as IEdmAction;
            Assert.NotNull(single);
            Assert.True(single.IsBound);
            Assert.NotNull(single.ReturnType.AsEntity());
            Assert.Equal(expectedReturnType, single.ReturnType.AsEntity().EntityDefinition());
        }
    }

    public class ActionProduct
    {
        public int Id { get; set; }
    }
}
