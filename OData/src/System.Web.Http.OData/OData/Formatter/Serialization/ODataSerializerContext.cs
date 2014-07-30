// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using SelectExpandClause = Microsoft.Data.OData.Query.SemanticAst.SelectExpandClause;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Context information used by the <see cref="ODataSerializer"/> when serializing objects in OData message format.
    /// </summary>
    public class ODataSerializerContext
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
        /// <param name="entity">The entity whose navigation property is being expanded.</param>
        /// <param name="selectExpandClause">The <see cref="SelectExpandClause"/> for the navigation property being expanded.</param>
        /// <param name="navigationProperty">The navigation property being expanded.</param>
        /// <remarks>This constructor is used to construct the serializer context for writing expanded properties.</remarks>
        public ODataSerializerContext(EntityInstanceContext entity, SelectExpandClause selectExpandClause, IEdmNavigationProperty navigationProperty)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            if (navigationProperty == null)
            {
                throw Error.ArgumentNull("navigationProperty");
            }

            ODataSerializerContext context = entity.SerializerContext;

            Request = context.Request;
            RequestContext = context.RequestContext;
            Url = context.Url;
            EntitySet = context.EntitySet;
            Model = context.Model;
            Path = context.Path;
            RootElementName = context.RootElementName;
            SkipExpensiveAvailabilityChecks = context.SkipExpensiveAvailabilityChecks;
            MetadataLevel = context.MetadataLevel;
            Items = context.Items;

            ExpandedEntity = entity;
            SelectExpandClause = selectExpandClause;
            NavigationProperty = navigationProperty;
            EntitySet = context.EntitySet.FindNavigationTarget(navigationProperty);
        }

        /// <summary>
        /// Gets or sets the HTTP Request whose response is being serialized.
        /// </summary>
        public HttpRequestMessage Request { get; set; }

        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        public HttpRequestContext RequestContext { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="UrlHelper"/> to use for generating OData links.
        /// </summary>
        public UrlHelper Url { get; set; }

        /// <summary>
        /// Gets or sets the entity set.
        /// </summary>
        public IEdmEntitySet EntitySet { get; set; }

        /// <summary>
        /// Gets or sets the EDM model associated with the request.
        /// </summary>
        public IEdmModel Model { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataPath"/> of the request.
        /// </summary>
        public ODataPath Path { get; set; }

        /// <summary>
        /// Gets or sets the root element name which is used when writing primitive types
        /// and complex types.
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
        /// Gets or sets the entity that is being expanded.
        /// </summary>
        public EntityInstanceContext ExpandedEntity { get; set; }

        /// <summary>
        /// Gets or sets the navigation property being expanded.
        /// </summary>
        public IEdmNavigationProperty NavigationProperty { get; set; }

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
                    throw Error.InvalidOperation(SRResources.ClrTypeNotInModel, type);
                }
            }

            return edmType;
        }
    }
}
