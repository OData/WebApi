// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Nop.Core.Domain.Directory
{
    /// <summary>
    /// Represents an exchange rate
    /// </summary>
    public partial class ExchangeRate
    {
        /// <summary>
        /// Creates a new instance of the ExchangeRate class
        /// </summary>
        public ExchangeRate()
        {
            CurrencyCode = string.Empty;
            Rate = 1.0m;
        }

        /// <summary>
        /// The three letter ISO code for the Exchange Rate, e.g. USD
        /// </summary>
        public virtual string CurrencyCode { get; set; }

        /// <summary>
        /// The conversion rate of this currency from the base currency
        /// </summary>
        public virtual decimal Rate { get; set; }

        /// <summary>
        /// When was this exchange rate updated from the data source (the internet data xml feed)
        /// </summary>
        public virtual DateTimeOffset UpdatedOn { get; set; }

        /// <summary>
        /// Format the rate into a string with the currency code, e.g. "USD 0.72543"
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} {1}", this.CurrencyCode, this.Rate);
        }
    }
}
