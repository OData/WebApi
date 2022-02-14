//-----------------------------------------------------------------------------
// <copyright file="MeasureDimension.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Directory
{
    /// <summary>
    /// Represents a measure dimension
    /// </summary>
    public partial class MeasureDimension : BaseEntity
    {
        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the system keyword
        /// </summary>
        public virtual string SystemKeyword { get; set; }

        /// <summary>
        /// Gets or sets the ratio
        /// </summary>
        public virtual decimal Ratio { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }
    }
}
