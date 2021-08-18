//-----------------------------------------------------------------------------
// <copyright file="ProductVariantExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Core.Domain.Catalog
{
    public static class ProductVariantExtensions
    {
        public static int[] ParseRequiredProductVariantIds(this ProductVariant productVariant)
        {
            if (productVariant == null)
                throw new ArgumentNullException("productVariant");

            if (String.IsNullOrEmpty(productVariant.RequiredProductVariantIds))
                return new int[0];

            var ids = new List<int>();

            foreach (var idStr in productVariant.RequiredProductVariantIds
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim()))
            {
                int id = 0;
                if (int.TryParse(idStr, out id))
                    ids.Add(id);
            }

            return ids.ToArray();
        }
    }
}
