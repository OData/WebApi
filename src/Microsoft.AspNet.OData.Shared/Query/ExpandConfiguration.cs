//-----------------------------------------------------------------------------
// <copyright file="ExpandConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents a configuration of an expandable property.
    /// </summary>
    public class ExpandConfiguration
    {
        /// <summary>
        /// Gets or sets the <see cref="SelectExpandType"/>.
        /// </summary>
        public SelectExpandType ExpandType { get; set; }

        /// <summary>
        /// Gets or sets the maximum depth.
        /// </summary>
        public int MaxDepth { get; set; }
    }
}
