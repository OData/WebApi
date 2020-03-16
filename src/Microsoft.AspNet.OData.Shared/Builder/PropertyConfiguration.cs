// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Base class for all property configurations.
    /// </summary>
    public abstract class PropertyConfiguration
    {
        private string _name;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyConfiguration"/> class.
        /// </summary>
        /// <param name="property">The name of the property.</param>
        /// <param name="declaringType">The declaring EDM type of the property.</param>
        protected PropertyConfiguration(PropertyInfo property, StructuralTypeConfiguration declaringType)
        {
            if (property == null)
            {
                throw Error.ArgumentNull("property");
            }

            if (declaringType == null)
            {
                throw Error.ArgumentNull("declaringType");
            }

            PropertyInfo = property;
            DeclaringType = declaringType;
            AddedExplicitly = true;
            _name = property.Name;
            QueryConfiguration = new QueryConfiguration();
            DerivedTypeConstraints = new DerivedTypeConstraintSet(property.PropertyType);
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null)
                {
                    throw Error.PropertyNull();
                }

                _name = value;
            }
        }

        /// <summary>
        /// Gets the declaring type.
        /// </summary>
        public StructuralTypeConfiguration DeclaringType { get; private set; }

        /// <summary>
        /// Gets the mapping CLR <see cref="PropertyInfo"/>.
        /// </summary>
        public PropertyInfo PropertyInfo { get; private set; }

        /// <summary>
        /// Gets the CLR <see cref="Type"/> of the property.
        /// </summary>
        public abstract Type RelatedClrType { get; }

        /// <summary>
        /// List of allowed derived types that are allowed for the property. 
        /// </summary>
        public DerivedTypeConstraintSet DerivedTypeConstraints { get; private set; }

        /// <summary>
        /// Gets the <see cref="PropertyKind"/> of the property.
        /// </summary>
        public abstract PropertyKind Kind { get; }

        /// <summary>
        /// Gets or sets a value that is <c>true</c> if the property was added by the user; <c>false</c> if it was inferred through conventions.
        /// </summary>
        /// <remarks>The default value is <c>true</c></remarks>
        public bool AddedExplicitly { get; set; }

        /// <summary>
        /// Gets whether the property is restricted, i.e. not filterable, not sortable, not navigable,
        /// not expandable, not countable, or automatically expand.
        /// </summary>
        public bool IsRestricted
        {
            get { return NotFilterable || NotSortable || NotNavigable || NotExpandable || NotCountable || AutoExpand; }
        }

        /// <summary>
        /// Gets or sets whether the property is not filterable. default is false.
        /// </summary>
        public bool NotFilterable { get; set; }

        /// <summary>
        /// Gets or sets whether the property is automatically expanded. default is false.
        /// </summary>
        public bool AutoExpand { get; set; }

        /// <summary>
        /// Gets or sets whether the automatic expand will be disabled if there is a $select specify by client.
        /// </summary>
        public bool DisableAutoExpandWhenSelectIsPresent { get; set; }

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
        /// Get or sets order in "order by"  expression.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="QueryConfiguration"/>.
        /// </summary>
        public QueryConfiguration QueryConfiguration { get; set; }

        /// <summary>
        /// Sets the property as not filterable.
        /// </summary>
        public PropertyConfiguration IsNotFilterable()
        {
            NotFilterable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as nonfilterable.
        /// </summary>
        public PropertyConfiguration IsNonFilterable()
        {
            return IsNotFilterable();
        }

        /// <summary>
        /// Sets the property as filterable.
        /// </summary>
        public PropertyConfiguration IsFilterable()
        {
            NotFilterable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not sortable.
        /// </summary>
        public PropertyConfiguration IsNotSortable()
        {
            NotSortable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as unsortable.
        /// </summary>
        public PropertyConfiguration IsUnsortable()
        {
            return IsNotSortable();
        }

        /// <summary>
        /// Sets the property as sortable.
        /// </summary>
        public PropertyConfiguration IsSortable()
        {
            NotSortable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not navigable.
        /// </summary>
        public PropertyConfiguration IsNotNavigable()
        {
            IsNotSortable();
            IsNotFilterable();
            NotNavigable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as navigable.
        /// </summary>
        public PropertyConfiguration IsNavigable()
        {
            NotNavigable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not expandable.
        /// </summary>
        public PropertyConfiguration IsNotExpandable()
        {
            NotExpandable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as expandable.
        /// </summary>
        public PropertyConfiguration IsExpandable()
        {
            NotExpandable = false;
            return this;
        }

        /// <summary>
        /// Sets the property as not countable.
        /// </summary>
        public PropertyConfiguration IsNotCountable()
        {
            NotCountable = true;
            return this;
        }

        /// <summary>
        /// Sets the property as countable.
        /// </summary>
        public PropertyConfiguration IsCountable()
        {
            NotCountable = false;
            return this;
        }

        /// <summary>
        /// Sets this property is countable.
        /// </summary>
        public PropertyConfiguration Count()
        {
            QueryConfiguration.SetCount(true);
            return this;
        }

        /// <summary>
        /// Sets whether this property is countable.
        /// </summary>
        public PropertyConfiguration Count(QueryOptionSetting queryOptionSetting)
        {
            QueryConfiguration.SetCount(queryOptionSetting == QueryOptionSetting.Allowed);
            return this;
        }

        /// <summary>
        /// Sets sortable properties depends on <see cref="QueryOptionSetting"/> of this property.
        /// </summary>
        public PropertyConfiguration OrderBy(QueryOptionSetting setting, params string[] properties)
        {
            QueryConfiguration.SetOrderBy(properties, setting == QueryOptionSetting.Allowed);
            return this;
        }

        /// <summary>
        /// Sets sortable properties of this property.
        /// </summary>
        public PropertyConfiguration OrderBy(params string[] properties)
        {
            QueryConfiguration.SetOrderBy(properties, true);
            return this;
        }

        /// <summary>
        /// Sets whether all properties of this property is sortable.
        /// </summary>
        public PropertyConfiguration OrderBy(QueryOptionSetting setting)
        {
            QueryConfiguration.SetOrderBy(null, setting == QueryOptionSetting.Allowed);
            return this;
        }

        /// <summary>
        /// Sets all properties of this property is sortable.
        /// </summary>
        public PropertyConfiguration OrderBy()
        {
            QueryConfiguration.SetOrderBy(null, true);
            return this;
        }

        /// <summary>
        /// Sets filterable properties depends on <see cref="QueryOptionSetting"/> of this property.
        /// </summary>
        public PropertyConfiguration Filter(QueryOptionSetting setting, params string[] properties)
        {
            QueryConfiguration.SetFilter(properties, setting == QueryOptionSetting.Allowed);
            return this;
        }

        /// <summary>
        /// Sets filterable properties of this property.
        /// </summary>
        public PropertyConfiguration Filter(params string[] properties)
        {
            QueryConfiguration.SetFilter(properties, true);
            return this;
        }

        /// <summary>
        /// Sets whether all properties of this property is filterable.
        /// </summary>
        public PropertyConfiguration Filter(QueryOptionSetting setting)
        {
            QueryConfiguration.SetFilter(null, setting == QueryOptionSetting.Allowed);
            return this;
        }

        /// <summary>
        /// Sets all properties of this property is filterable.
        /// </summary>
        public PropertyConfiguration Filter()
        {
            QueryConfiguration.SetFilter(null, true);
            return this;
        }

        /// <summary>
        /// Sets selectable properties depends on <see cref="SelectExpandType"/> of this property.
        /// </summary>
        public PropertyConfiguration Select(SelectExpandType selectType, params string[] properties)
        {
            QueryConfiguration.SetSelect(properties, selectType);
            return this;
        }

        /// <summary>
        /// Sets selectable properties of this property.
        /// </summary>
        public PropertyConfiguration Select(params string[] properties)
        {
            QueryConfiguration.SetSelect(properties, SelectExpandType.Allowed);
            return this;
        }

        /// <summary>
        /// Sets <see cref="SelectExpandType"/> of all properties of this property is selectable.
        /// </summary>
        public PropertyConfiguration Select(SelectExpandType selectType)
        {
            QueryConfiguration.SetSelect(null, selectType);
            return this;
        }

        /// <summary>
        /// Sets all properties of this property is selectable.
        /// </summary>
        public PropertyConfiguration Select()
        {
            QueryConfiguration.SetSelect(null, SelectExpandType.Allowed);
            return this;
        }

        /// <summary>
        /// Sets the max value of $top of this property that a client can request
        /// and the maximum number of query results of this property to return.
        /// </summary>
        public PropertyConfiguration Page(int? maxTopValue, int? pageSizeValue)
        {
            QueryConfiguration.SetMaxTop(maxTopValue);
            QueryConfiguration.SetPageSize(pageSizeValue);
            return this;
        }

        /// <summary>
        /// Sets this property enable paging.
        /// </summary>
        public PropertyConfiguration Page()
        {
            QueryConfiguration.SetMaxTop(null);
            QueryConfiguration.SetPageSize(null);
            return this;
        }

        /// <summary>
        /// Sets the maximum depth of expand result,
        /// expandable properties and their <see cref="SelectExpandType"/> of this navigation property.
        /// </summary>
        public PropertyConfiguration Expand(int maxDepth, SelectExpandType expandType, params string[] properties)
        {
            QueryConfiguration.SetExpand(properties, maxDepth, expandType);
            return this;
        }

        /// <summary>
        /// Sets the expandable properties of this navigation property.
        /// </summary>
        public PropertyConfiguration Expand(params string[] properties)
        {
            QueryConfiguration.SetExpand(properties, null, SelectExpandType.Allowed);
            return this;
        }

        /// <summary>
        /// Sets the maximum depth of expand result,
        /// expandable properties of this navigation property.
        /// </summary>
        public PropertyConfiguration Expand(int maxDepth, params string[] properties)
        {
            QueryConfiguration.SetExpand(properties, maxDepth, SelectExpandType.Allowed);
            return this;
        }

        /// <summary>
        /// Sets the expandable properties and their <see cref="SelectExpandType"/> of this navigation property.
        /// </summary>
        public PropertyConfiguration Expand(SelectExpandType expandType, params string[] properties)
        {
            QueryConfiguration.SetExpand(properties, null, expandType);
            return this;
        }

        /// <summary>
        /// Sets <see cref="SelectExpandType"/> of all properties with maximum depth of expand result.
        /// </summary>
        public PropertyConfiguration Expand(SelectExpandType expandType, int maxDepth)
        {
            QueryConfiguration.SetExpand(null, maxDepth, expandType);
            return this;
        }

        /// <summary>
        /// Sets all properties expandable with maximum depth of expand result.
        /// </summary>
        public PropertyConfiguration Expand(int maxDepth)
        {
            QueryConfiguration.SetExpand(null, maxDepth, SelectExpandType.Allowed);
            return this;
        }

        /// <summary>
        /// Sets <see cref="SelectExpandType"/> of all properties.
        /// </summary>
        public PropertyConfiguration Expand(SelectExpandType expandType)
        {
            QueryConfiguration.SetExpand(null, null, expandType);
            return this;
        }

        /// <summary>
        /// Sets all properties expandable.
        /// </summary>
        public PropertyConfiguration Expand()
        {
            QueryConfiguration.SetExpand(null, null, SelectExpandType.Allowed);
            return this;
        }
    }
}
