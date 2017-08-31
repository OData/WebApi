// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace System.Web.OData.Query
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
