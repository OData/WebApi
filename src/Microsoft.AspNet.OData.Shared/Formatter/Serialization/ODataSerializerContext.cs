//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
using SelectExpandClause = Microsoft.OData.UriParser.SelectExpandClause;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the <see cref="ODataSerializer"/> when serializing objects in OData message format.
    /// </summary>
    public partial class ODataSerializerContext
    {
        private ClrTypeCache _typeMappingCache;
        private IDictionary<object, object> _items;
        private ODataQueryContext _queryContext;
        private SelectExpandClause _selectExpandClause;
        private bool _isSelectExpandClauseSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        public ODataSerializerContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        /// <param name="resource">The resource whose property is being nested.</param>
        /// <param name="selectExpandClause">The <see cref="SelectExpandClause"/> for the property being nested.</param>
        /// <param name="edmProperty">The complex property being nested or the navigation property being expanded.
        /// If the resource property is the dynamic complex, the resource property is null.
        /// </param>
        /// <remarks>This constructor is used to construct the serializer context for writing nested and expanded properties.</remarks>
        public ODataSerializerContext(ResourceContext resource, SelectExpandClause selectExpandClause, IEdmProperty edmProperty)
            : this(resource, edmProperty, null, null)
        {
            SelectExpandClause = selectExpandClause;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class for nested resources.
        /// </summary>
        /// <param name="resource">The resource whose property is being nested.</param>
        /// <param name="edmProperty">The complex property being nested or the navigation property being expanded.
        /// If the resource property is the dynamic complex, the resource property is null.
        /// </param>
        /// <param name="queryContext">The <see cref="ODataQueryContext"/> for the property being nested.</param>
        /// <param name="currentSelectItem">The <see cref="SelectItem"/> for the property being nested.></param>
        internal ODataSerializerContext(ResourceContext resource, IEdmProperty edmProperty, ODataQueryContext queryContext, SelectItem currentSelectItem)
        {
            if (resource == null)
            {
                throw Error.ArgumentNull("resource");
            }

            // Clone the resource's context. Use a helper function so it can
            // handle platform-specific differences in ODataSerializerContext.
            ODataSerializerContext context = resource.SerializerContext;
            this.CopyPlatformSpecificProperties(context);

            Model = context.Model;
            Path = context.Path;
            RootElementName = context.RootElementName;
            SkipExpensiveAvailabilityChecks = context.SkipExpensiveAvailabilityChecks;
            MetadataLevel = context.MetadataLevel;
            Items = context.Items;
            ExpandReference = context.ExpandReference;

            QueryContext = queryContext;

            ExpandedResource = resource; // parent resource

            CurrentSelectItem = currentSelectItem;

            var expandedNavigationSelectItem = currentSelectItem as ExpandedNavigationSelectItem;
            if (expandedNavigationSelectItem != null)
            {
                SelectExpandClause = expandedNavigationSelectItem.SelectAndExpand;
                NavigationSource = expandedNavigationSelectItem.NavigationSource;
            }
            else
            {
                var pathSelectItem = currentSelectItem as PathSelectItem;
                if (pathSelectItem != null)
                {
                    SelectExpandClause = pathSelectItem.SelectAndExpand;
                    NavigationSource = resource.NavigationSource; // Use it's parent navigation source.
                }

                var referencedNavigation = currentSelectItem as ExpandedReferenceSelectItem;
                if (referencedNavigation != null)
                {
                    ExpandReference = true;
                    NavigationSource = referencedNavigation.NavigationSource;
                }
            }

            EdmProperty = edmProperty; // should be nested property

            if (currentSelectItem == null || (NavigationSource as IEdmUnknownEntitySet) != null)
            {
                IEdmNavigationProperty navigationProperty = edmProperty as IEdmNavigationProperty;
                if (navigationProperty != null && context.NavigationSource != null)
                {
                    NavigationSource = context.NavigationSource.FindNavigationTarget(NavigationProperty);
                }
                else
                {
                    NavigationSource = resource.NavigationSource;
                }
            }
        }

        internal IWebApiRequestMessage InternalRequest { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to use for generating OData links.
        /// </summary>
        internal IWebApiUrlHelper InternalUrlHelper { get; private set; }

        /// <summary>
        /// ODataQueryContext object, retrieved from query options for top-level context and passed down to nested serializer context as is.
        /// </summary>
        internal ODataQueryContext QueryContext
        {
            get
            {
                if (QueryOptions != null)
                {
                    return QueryOptions.Context;
                }

                return _queryContext;
            }
            private set { _queryContext = value; }
        }

        /// <summary>
        /// Gets or sets the navigation source.
        /// </summary>
        public IEdmNavigationSource NavigationSource { get; set; }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataPath"/> of the request.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the root element name which is used when writing primitive and enum types
        /// </summary>
        public string RootElementName { get; set; }

        /// <summary>
        /// Get or sets whether expensive links should be calculated.
        /// </summary>
        public bool SkipExpensiveAvailabilityChecks { get; set; }

        /// <summary>
        /// Gets or sets the metadata level of the response.
        /// </summary>
        public ODataMetadataLevel MetadataLevel { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SelectExpandClause"/>.
        /// </summary>
        public SelectExpandClause SelectExpandClause
        {
            get
            {
                // private backing field to be removed once public setter from ODataFeature is removed.
                if (_isSelectExpandClauseSet)
                {
                    return _selectExpandClause;
                }

                if (QueryOptions != null)
                {
                    if (QueryOptions.SelectExpand != null)
                    {
                        return QueryOptions.SelectExpand.ProcessedSelectExpandClause;
                    }

                    return null;
                }

                ExpandedNavigationSelectItem expandedItem = CurrentSelectItem as ExpandedNavigationSelectItem;
                if (expandedItem != null)
                {
                    return expandedItem.SelectAndExpand;
                }

                return null;
            }
            set
            {
                _isSelectExpandClauseSet = true;
                _selectExpandClause = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ExpandedNavigationSelectItem"/>.
        /// </summary>
        internal ExpandedReferenceSelectItem CurrentExpandedSelectItem
        {
            get
            {
                return CurrentSelectItem as ExpandedReferenceSelectItem;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SelectItem"/>.
        /// </summary>
        internal SelectItem CurrentSelectItem { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataQueryOptions"/>.
        /// </summary>
        public ODataQueryOptions QueryOptions { get; internal set; }

        /// <summary>
        /// Gets or sets the relative path to the resouce being serialized
        /// </summary>
        internal Queue<IEdmProperty> PropertiesInPath { get; private set; }

        /// <summary>
        /// Gets or sets the resource that is being expanded.
        /// </summary>
        public ResourceContext ExpandedResource { get; set; }

        /// <summary>
        /// Gets or sets the boolean value indicating whether it's $ref expanded.
        /// </summary>
        public bool ExpandReference { get; set; }

        /// <summary>
        /// Gets or sets the complex property being nested or navigation property being expanded.
        /// </summary>
        public IEdmProperty EdmProperty { get; set; }

        /// <summary>
        /// Gets or sets the navigation property being expanded.
        /// </summary>
        public IEdmNavigationProperty NavigationProperty
        {
            get
            {
                return EdmProperty as IEdmNavigationProperty;
            }
        }

        /// <summary>
        /// Gets a property bag associated with this context to store any generic data.
        /// </summary>
        public IDictionary<object, object> Items
        {
            get
            {
                _items = _items ?? new Dictionary<object, object>();
                return _items;
            }
            private set
            {
                _items = value;
            }
        }

        internal IEdmTypeReference GetEdmType(object instance, Type type)
        {
            IEdmTypeReference edmType;

            IEdmObject edmObject = instance as IEdmObject;
            if (edmObject != null)
            {
                edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw Error.InvalidOperation(SRResources.EdmTypeCannotBeNull, edmObject.GetType().FullName,
                        typeof(IEdmObject).Name);
                }
            }
            else
            {
                if (Model == null)
                {
                    throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
                }

                _typeMappingCache = _typeMappingCache ?? Model.GetTypeMappingCache();
                edmType = _typeMappingCache.GetEdmType(type, Model);

                if (edmType == null)
                {
                    if (instance != null)
                    {
                        edmType = _typeMappingCache.GetEdmType(instance.GetType(), Model);
                    }

                    if (edmType == null)
                    {
                        throw Error.InvalidOperation(SRResources.ClrTypeNotInModel, type);
                    }
                }
                else if (instance != null)
                {
                    IEdmTypeReference actualType = _typeMappingCache.GetEdmType(instance.GetType(), Model);
                    if (actualType != null && actualType != edmType)
                    {
                        edmType = actualType;
                    }
                }
            }

            return edmType;
        }
    }
}
