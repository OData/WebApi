//-----------------------------------------------------------------------------
// <copyright file="PaymentStatus.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Payments
{
    /// <summary>
    /// Represents a payment status enumeration
    /// </summary>
    public enum PaymentStatus : int
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 10,
        /// <summary>
        /// Authorized
        /// </summary>
        Authorized = 20,
        /// <summary>
        /// Paid
        /// </summary>
        Paid = 30,
        /// <summary>
        /// Partially Refunded
        /// </summary>
        PartiallyRefunded = 35,
        /// <summary>
        /// Refunded
        /// </summary>
        Refunded = 40,
        /// <summary>
        /// Voided
        /// </summary>
        Voided = 50,
    }
}
