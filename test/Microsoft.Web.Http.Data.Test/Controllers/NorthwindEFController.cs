// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Web.Http.Data.EntityFramework;
using Microsoft.Web.Http.Data.Test.Models.EF;

namespace Microsoft.Web.Http.Data.Test
{
    public class NorthwindEFTestController : LinqToEntitiesDataController<NorthwindEntities>
    {
        public IQueryable<Product> GetProducts()
        {
            return this.ObjectContext.Products;
        }

        public void InsertProduct(Product product)
        {
        }

        public void UpdateProduct(Product product)
        {
        }

        protected override NorthwindEntities CreateObjectContext()
        {
            return new NorthwindEntities(TestHelpers.GetTestEFConnectionString());
        }
    }
}

namespace Microsoft.Web.Http.Data.Test.Models.EF
{
    [MetadataType(typeof(ProductMetadata))]
    public partial class Product
    {
        internal sealed class ProductMetadata
        {
            [Editable(false, AllowInitialValue = true)]
            [StringLength(777, MinimumLength = 2)]
            public string QuantityPerUnit { get; set; }

            [Range(0, 1000000)]
            public string UnitPrice { get; set; }
        }
    }
}
