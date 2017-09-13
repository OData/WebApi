// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a return status
    /// </summary>
    public enum ReturnRequestStatus : int
    {
        /// <summary>
        /// Pending
        /// </summary>
        Pending = 0,
        /// <summary>
        /// Received
        /// </summary>
        Received = 10,
        /// <summary>
        /// Return authorized
        /// </summary>
        ReturnAuthorized = 20,
        /// <summary>
        /// Item(s) repaired
        /// </summary>
        ItemsRepaired = 30,
        /// <summary>
        /// Item(s) refunded
        /// </summary>
        ItemsRefunded = 40,
        /// <summary>
        /// Request rejected
        /// </summary>
        RequestRejected = 50,
        /// <summary>
        /// Cancelled
        /// </summary>
        Cancelled = 60,
    }
}
