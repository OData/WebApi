//-----------------------------------------------------------------------------
// <copyright file="ProductFamily.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies
{
    public class ProductFamily
    {
        public ProductFamily()
        {
            Products = new List<Product>();
        }

        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public long LongProperty { get; set; }

        public virtual Supplier Supplier { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
