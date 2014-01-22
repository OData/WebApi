// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Builder;

namespace System.Web.OData
{
    /// <summary>
    /// Represents a queryable restriction on an EDM property, including nonfilterable, unsortable, not navigable, not expandable.
    /// </summary>
    public class QueryableRestrictions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryableRestrictions"/> class.
        /// </summary>
        public QueryableRestrictions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryableRestrictions"/> class.
        /// </summary>
        /// <param name="propertyConfiguration">The PropertyConfiguration containing queryable restrictions.</param>
        public QueryableRestrictions(PropertyConfiguration propertyConfiguration)
        {
            NonFilterable = propertyConfiguration.NonFilterable;
            Unsortable = propertyConfiguration.Unsortable;
            NotNavigable = propertyConfiguration.NotNavigable;
            NotExpandable = propertyConfiguration.NotExpandable;
        }

        /// <summary>
        /// Gets or sets whether the property is nonfilterable. default is false.
        /// </summary>
        public bool NonFilterable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is unsortable. default is false.
        /// </summary>
        public bool Unsortable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is not navigable. default is false.
        /// </summary>
        public bool NotNavigable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is not expandable. default is false.
        /// </summary>
        public bool NotExpandable { get; set; }
    }
}
