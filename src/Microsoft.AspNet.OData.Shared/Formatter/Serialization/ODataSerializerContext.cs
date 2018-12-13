// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
using OrderByClause = Microsoft.OData.UriParser.OrderByClause;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        public ODataSerializerContext()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataSerializerContext"/> class.
        /// </summary>
        /// <param name="resource">The resource whose navigation property is being expanded.</param>
        /// <param name="selectExpandClause">The <see cref="SelectExpandClause"/> for the navigation property being expanded.</param>
        /// <param name="edmProperty">The complex property being nested or the navigation property being expanded.
        /// If the resource property is the dynamic complex, the resource property is null.
        /// </param>
        /// <remarks>This constructor is used to construct the serializer context for writing nested and expanded properties.</remarks>
        public ODataSerializerContext(ResourceContext resource, SelectExpandClause selectExpandClause, IEdmProperty edmProperty)
            : this(resource, selectExpandClause, edmProperty, null, null)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="selectExpandClause"></param>
        /// <param name="edmProperty"></param>
        /// <param name="orderByClause"></param>
        /// <param name="expandedItem"></param>
        public ODataSerializerContext(ResourceContext resource, SelectExpandClause selectExpandClause, IEdmProperty edmProperty, OrderByClause orderByClause, ExpandedNavigationSelectItem expandedItem)
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

            ExpandedResource = resource; // parent resource
            SelectExpandClause = selectExpandClause;
            OrderByClause = orderByClause;
            ExpandedNavigationSelectItem = expandedItem;
            EdmProperty = edmProperty; // should be nested property

            if (context.NavigationSource != null)
            {
                IEdmNavigationProperty navigationProperty = edmProperty as IEdmNavigationProperty;
                if (navigationProperty != null)
                {
                    NavigationSource = context.NavigationSource.FindNavigationTarget(NavigationProperty);
                }
            }
        }

        internal IWebApiRequestMessage InternalRequest { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to use for generating OData links.
        /// </summary>
        internal IWebApiUrlHelper InternalUrlHelper { get; private set; }

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
        public SelectExpandClause SelectExpandClause { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ExpandedNavigationSelectItem"/>.
        /// </summary>
        public ExpandedNavigationSelectItem ExpandedNavigationSelectItem { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="OrderByClause"/>.
        /// </summary>
        public OrderByClause OrderByClause { get; set; }

        /// <summary>
        /// Gets or sets the resource that is being expanded.
        /// </summary>
        public ResourceContext ExpandedResource { get; set; }

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
