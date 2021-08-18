//-----------------------------------------------------------------------------
// <copyright file="PriceRange.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a price range
    /// </summary>
    public partial class PriceRange
    {
        /// <summary>
        /// From
        /// </summary>
        public virtual decimal? From { get; set; }
        /// <summary>
        /// To
        /// </summary>
        public virtual decimal? To { get; set; }
    }
}
