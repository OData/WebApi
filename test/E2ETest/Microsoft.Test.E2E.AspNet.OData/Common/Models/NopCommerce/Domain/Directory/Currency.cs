//-----------------------------------------------------------------------------
// <copyright file="Currency.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Nop.Core.Domain.Localization;

namespace Nop.Core.Domain.Directory
{
    /// <summary>
    /// Represents a currency
    /// </summary>
    public partial class Currency : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the currency code
        /// </summary>
        public virtual string CurrencyCode { get; set; }

        /// <summary>
        /// Gets or sets the rate
        /// </summary>
        public virtual decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets the display locale
        /// </summary>
        public virtual string DisplayLocale { get; set; }

        /// <summary>
        /// Gets or sets the custom formatting
        /// </summary>
        public virtual string CustomFormatting { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public virtual bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public virtual DateTimeOffset CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public virtual DateTimeOffset UpdatedOnUtc { get; set; }
    }

}
