// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a best sellers report line
    /// </summary>
    public partial class BestsellersReportLine
    {
        /// <summary>
        /// Gets or sets the product or product variant identifier
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the total amount
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the total quantity
        /// </summary>
        public int TotalQuantity { get; set; }
    }
}
