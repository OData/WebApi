// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
