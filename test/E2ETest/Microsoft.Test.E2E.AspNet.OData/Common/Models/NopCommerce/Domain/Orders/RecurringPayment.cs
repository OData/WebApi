//-----------------------------------------------------------------------------
// <copyright file="RecurringPayment.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents a recurring payment
    /// </summary>
    public partial class RecurringPayment : BaseEntity
    {
        private ICollection<RecurringPaymentHistory> _recurringPaymentHistory;

        /// <summary>
        /// Gets or sets the cycle length
        /// </summary>
        public virtual int CycleLength { get; set; }

        /// <summary>
        /// Gets or sets the cycle period identifier
        /// </summary>
        public virtual int CyclePeriodId { get; set; }

        /// <summary>
        /// Gets or sets the total cycles
        /// </summary>
        public virtual int TotalCycles { get; set; }

        /// <summary>
        /// Gets or sets the start date
        /// </summary>
        public virtual DateTimeOffset StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the payment is active
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public virtual bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the initial order identifier
        /// </summary>
        public virtual int InitialOrderId { get; set; }

        /// <summary>
        /// Gets or sets the date and time of payment creation
        /// </summary>
        public virtual DateTimeOffset CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets the next payment date
        /// </summary>
        public virtual DateTimeOffset? NextPaymentDate
        {
            get
            {
                //result
                DateTimeOffset? result = null;

                if (!this.IsActive)
                    return result;

                var historyCollection = this.RecurringPaymentHistory;
                if (historyCollection.Count >= this.TotalCycles)
                {
                    return result;
                }
                if (historyCollection.Count > 0)
                {
                    switch (this.CyclePeriod)
                    {
                        case RecurringProductCyclePeriod.Days:
                            result = this.StartDateUtc.AddDays((double)this.CycleLength * historyCollection.Count);
                            break;
                        case RecurringProductCyclePeriod.Weeks:
                            result = this.StartDateUtc.AddDays((double)(7 * this.CycleLength) * historyCollection.Count);
                            break;
                        case RecurringProductCyclePeriod.Months:
                            result = this.StartDateUtc.AddMonths(this.CycleLength * historyCollection.Count);
                            break;
                        case RecurringProductCyclePeriod.Years:
                            result = this.StartDateUtc.AddYears(this.CycleLength * historyCollection.Count);
                            break;
                        default:
                            throw new NopException("Not supported cycle period");
                    }
                }
                else
                {
                    if (this.TotalCycles > 0)
                        result = this.StartDateUtc;
                }
                //}

                return result;
            }
        }

        /// <summary>
        /// Gets the cycles remaining
        /// </summary>
        public virtual int CyclesRemaining
        {
            get
            {
                //result
                var historyCollection = this.RecurringPaymentHistory;
                int result = this.TotalCycles - historyCollection.Count;
                if (result < 0)
                    result = 0;

                return result;
            }
        }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public virtual RecurringProductCyclePeriod CyclePeriod
        {
            get
            {
                return (RecurringProductCyclePeriod)this.CyclePeriodId;
            }
            set
            {
                this.CyclePeriodId = (int)value;
            }
        }

        /// <summary>
        /// Gets or sets the recurring payment history
        /// </summary>
        public virtual ICollection<RecurringPaymentHistory> RecurringPaymentHistory
        {
            get { return _recurringPaymentHistory ?? (_recurringPaymentHistory = new List<RecurringPaymentHistory>()); }
            protected set { _recurringPaymentHistory = value; }
        }

        /// <summary>
        /// Gets the initial order
        /// </summary>
        public virtual Order InitialOrder { get; set; }
    }
}
