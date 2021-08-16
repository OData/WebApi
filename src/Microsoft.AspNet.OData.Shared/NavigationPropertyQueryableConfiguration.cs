//-----------------------------------------------------------------------------
// <copyright file="NavigationPropertyQueryableConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a queryable configuration on an EDM navigation property, including auto expanded.
    /// </summary>
    public class NavigationPropertyQueryableConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationPropertyQueryableConfiguration"/> class.
        /// </summary>
        /// <param name="propertyConfiguration">The NavigationPropertyConfiguration containing queryable configuration.</param>
        public NavigationPropertyQueryableConfiguration(NavigationPropertyConfiguration propertyConfiguration)
        {
            AutoExpand = propertyConfiguration.Expand;
        }

        /// <summary>
        /// Gets or sets whether the property is auto expanded.
        /// </summary>
        public bool AutoExpand { get; set; }
    }
}
