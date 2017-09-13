// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a backorder mode
    /// </summary>
    public enum BackorderMode
    {
        /// <summary>
        /// No backorders
        /// </summary>
        NoBackorders = 0,
        /// <summary>
        /// Allow qty below 0
        /// </summary>
        AllowQtyBelow0 = 1,
        /// <summary>
        /// Allow qty below 0 and notify customer
        /// </summary>
        AllowQtyBelow0AndNotifyCustomer = 2,
    }
}
