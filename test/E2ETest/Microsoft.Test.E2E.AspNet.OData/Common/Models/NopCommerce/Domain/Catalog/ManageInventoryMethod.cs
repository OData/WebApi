//-----------------------------------------------------------------------------
// <copyright file="ManageInventoryMethod.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a method of inventory management
    /// </summary>
    public enum ManageInventoryMethod
    {
        /// <summary>
        /// Don't track inventory for product variant
        /// </summary>
        DontManageStock = 0,
        /// <summary>
        /// Track inventory for product variant
        /// </summary>
        ManageStock = 1,
        /// <summary>
        /// Track inventory for product variant by product attributes
        /// </summary>
        ManageStockByAttributes = 2,
    }
}
