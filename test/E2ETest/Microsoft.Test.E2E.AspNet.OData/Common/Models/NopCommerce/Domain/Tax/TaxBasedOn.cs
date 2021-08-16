//-----------------------------------------------------------------------------
// <copyright file="TaxBasedOn.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
