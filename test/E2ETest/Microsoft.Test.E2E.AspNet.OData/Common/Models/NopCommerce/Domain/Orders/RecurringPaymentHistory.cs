// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a recurring payment history
    /// </summary>
    public partial class RecurringPaymentHistory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the recurring payment identifier
        /// </summary>
        public virtual int RecurringPaymentId { get; set; }

        /// <summary>
        /// Gets or sets the recurring payment identifier
        /// </summary>
        public virtual int OrderId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of entity creation
        /// </summary>
        public virtual DateTimeOffset CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the recurring payment
        /// </summary>
        public virtual RecurringPayment RecurringPayment { get; set; }
    }
}
