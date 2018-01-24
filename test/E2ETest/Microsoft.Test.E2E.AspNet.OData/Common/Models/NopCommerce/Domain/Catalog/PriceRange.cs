// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
