//-----------------------------------------------------------------------------
// <copyright file="ProductReviewHelpfulness.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Nop.Core.Domain.Customers;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product review helpfulness
    /// </summary>
    public partial class ProductReviewHelpfulness : CustomerContent
    {
        /// <summary>
        /// Gets or sets the product review identifier
        /// </summary>
        public virtual int ProductReviewId { get; set; }

        /// <summary>
        /// A value indicating whether a review a helpful
        /// </summary>
        public virtual bool WasHelpful { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual ProductReview ProductReview { get; set; }
    }
}
