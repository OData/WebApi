// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.OData.Builder;

namespace System.Web.OData
{
    /// <summary>
    /// Represents a queryable restriction on an EDM property, including not filterable, not sortable,
    /// not navigable, not expandable, not countable.
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
            NotFilterable = propertyConfiguration.NotFilterable;
            NotSortable = propertyConfiguration.NotSortable;
            NotNavigable = propertyConfiguration.NotNavigable;
            NotExpandable = propertyConfiguration.NotExpandable;
            NotCountable = propertyConfiguration.NotCountable;
        }

        /// <summary>
        /// Gets or sets whether the property is not filterable. default is false.
        /// </summary>
        public bool NotFilterable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is nonfilterable. default is false.
        /// </summary>
        public bool NonFilterable
        {
            get { return NotFilterable; }
            set { NotFilterable = value; }
        }

        /// <summary>
        /// Gets or sets whether the property is not sortable. default is false.
        /// </summary>
        public bool NotSortable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is unsortable. default is false.
        /// </summary>
        public bool Unsortable
        {
            get { return NotSortable; }
            set { NotSortable = value; }
        }

        /// <summary>
        /// Gets or sets whether the property is not navigable. default is false.
        /// </summary>
        public bool NotNavigable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is not expandable. default is false.
        /// </summary>
        public bool NotExpandable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is not countable. default is false.
        /// </summary>
        public bool NotCountable { get; set; }
    }
}
