// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Tax
{
    /// <summary>
    /// Represents the tax based on
    /// </summary>
    public enum TaxBasedOn
    {
        /// <summary>
        /// Billing address
        /// </summary>
        BillingAddress = 1,
        /// <summary>
        /// Shipping address
        /// </summary>
        ShippingAddress = 2,
        /// <summary>
        /// Default address
        /// </summary>
        DefaultAddress = 3,
    }
}
