//-----------------------------------------------------------------------------
// <copyright file="QueryableRestrictions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Builder;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a queryable restriction on an EDM property, including not filterable, not sortable,
    /// not navigable, not expandable, not countable, automatically expand.
    /// </summary>
    public class QueryableRestrictions
    {
        private bool _autoExpand;

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
            DisableAutoExpandWhenSelectIsPresent = propertyConfiguration.DisableAutoExpandWhenSelectIsPresent;
            _autoExpand = propertyConfiguration.AutoExpand;
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

        /// <summary>
        /// Gets or sets whether the property is automatically expanded. default is false.
        /// </summary>
        public bool AutoExpand 
        {
            get { return !NotExpandable && _autoExpand; }
            set { _autoExpand = value; }
        }

        /// <summary>
        /// If set to <c>true</c> then automatic expand will be disabled if there is a $select specify by client.
        /// </summary>
        public bool DisableAutoExpandWhenSelectIsPresent { get; set; }
    }
}
