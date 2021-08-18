//-----------------------------------------------------------------------------
// <copyright file="LowStockActivity.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a low stock activity
    /// </summary>
    public enum LowStockActivity
    {
        /// <summary>
        /// Nothing
        /// </summary>
        Nothing = 0,
        /// <summary>
        /// Disable buy button
        /// </summary>
        DisableBuyButton = 1,
        /// <summary>
        /// Unpublish
        /// </summary>
        Unpublish = 2,
    }
}
