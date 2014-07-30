// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// An instance of <see cref="EntityInstanceContext{TEntityType}"/> gets passed to the self link (
    /// <see cref="M:NavigationSourceConfiguration.HasIdLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasEditLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasReadLink"/>
    /// ) and navigation link (
    /// <see cref="M:NavigationSourceConfiguration.HasNavigationPropertyLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasNavigationPropertiesLink"/>
    /// ) builders and can be used by the link builders to generate links.
    /// </summary>
    public class EntityInstanceContext
    {
        private object _entityInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstanceContext"/> class.
        /// </summary>
        public EntityInstanceContext()
        {
            SerializerContext = new ODataSerializerContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityInstanceContext"/> class.
        /// </summary>
        /// <param name="serializerContext">The backing <see cref="ODataSerializerContext"/>.</param>
        /// <param name="entityType">The EDM entity type of this instance context.</param>
        /// <param name="entityInstance">The object representing the instance of this context.</param>
        public EntityInstanceContext(ODataSerializerContext serializerContext, IEdmEntityTypeReference entityType, object entityInstance)
            : this(serializerContext, entityType, AsEdmEntityObject(entityInstance, entityType, serializerContext.Model))
        {
        }

        private EntityInstanceContext(ODataSerializerContext serializerContext, IEdmEntityTypeReference entityType, IEdmEntityObject edmObject)
        {
            if (serializerContext == null)
            {
                throw Error.ArgumentNull("serializerContext");
            }

            SerializerContext = serializerContext;
            EntityType = entityType.EntityDefinition();
            EdmObject = edmObject;
        }

        /// <summary>
        /// Gets or sets the <see cref="ODataSerializerContext"/>.
        /// </summary>
        public ODataSerializerContext SerializerContext { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        public HttpRequestMessage Request
        {
            get
            {
                return SerializerContext.Request;
            }
            set
            {
                SerializerContext.Request = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belogs.
        /// </summary>
        public IEdmModel EdmModel
        {
            get
            {
                return SerializerContext.Model;
            }
            set
            {
                SerializerContext.Model = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEdmNavigationSource"/> to which this instance belongs.
        /// </summary>
        public IEdmNavigationSource NavigationSource
        {
            get
            {
                return SerializerContext.NavigationSource;
            }
            set
            {
                SerializerContext.NavigationSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEdmEntityType"/> of this entity instance.
        /// </summary>
        public IEdmEntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmEntityObject"/> backing this instance.
        /// </summary>
        public IEdmEntityObject EdmObject { get; set; }

        /// <summary>
        /// Gets or sets the value of this entity instance.
        /// </summary>
        public object EntityInstance
        {
            get
            {
                if (_entityInstance == null)
                {
                    _entityInstance = BuildEntityInstance();
                }
                return _entityInstance;
            }
            set
            {
                _entityInstance = value;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="UrlHelper"/> that may be used to generate links while serializing this entity
        /// instance.
        /// </summary>
        public UrlHelper Url
        {
            get
            {
                return SerializerContext.Url;
            }
            set
            {
                SerializerContext.Url = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether ActionAvailabilityChecks should be performed or not.
        /// </summary>
        /// <remarks>
        /// This value is used to tell the formatter whether to check availability of an action before including a link
        /// to it. When in a feed we skip this check.
        /// </remarks>
        public bool SkipExpensiveAvailabilityChecks
        {
            get
            {
                return SerializerContext.SkipExpensiveAvailabilityChecks;
            }
            set
            {
                SerializerContext.SkipExpensiveAvailabilityChecks = value;
            }
        }

        /// <summary>
        /// Gets the value of the property with the given name from the <see cref="IEdmObject"/> of this instance if present; throws if the property is
        /// not present.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The value of the property if present.</returns>
        internal object GetPropertyValue(string propertyName)
        {
            if (EdmObject == null)
            {
                throw Error.InvalidOperation(SRResources.EdmObjectNull, typeof(EntityInstanceContext).Name);
            }

            object value;
            if (EdmObject.TryGetPropertyValue(propertyName, out value))
            {
                return value;
            }
            else
            {
                IEdmTypeReference edmType = EdmObject.GetEdmType();
                if (edmType == null)
                {
                    // Provide general guidance in the message. typeof(IEdmTypeReference).Name would be too specific.
                    throw Error.InvalidOperation(SRResources.EdmTypeCannotBeNull, EdmObject.GetType().FullName,
                        typeof(IEdmObject).Name);
                }

                throw Error.InvalidOperation(SRResources.PropertyNotFound, edmType.ToTraceString(), propertyName);
            }
        }

        private object BuildEntityInstance()
        {
            if (EdmObject == null)
            {
                return null;
            }

            TypedEdmEntityObject edmEntityObject = EdmObject as TypedEdmEntityObject;
            if (edmEntityObject != null)
            {
                return edmEntityObject.Instance;
            }

            Type clrType = EdmLibHelpers.GetClrType(EntityType, EdmModel);
            if (clrType == null)
            {
                throw new InvalidOperationException(Error.Format(SRResources.MappingDoesNotContainEntityType, EntityType.FullName()));
            }

            object resource = Activator.CreateInstance(clrType);
            foreach (IEdmStructuralProperty property in EntityType.StructuralProperties())
            {
                object value;
                if (EdmObject.TryGetPropertyValue(property.Name, out value) && value != null)
                {
                    if (value.GetType().IsCollection())
                    {
                        DeserializationHelpers.SetCollectionProperty(resource, property, value, property.Name);
                    }
                    else
                    {
                        DeserializationHelpers.SetProperty(resource, property.Name, value);
                    }
                }
            }

            return resource;
        }

        private static IEdmEntityObject AsEdmEntityObject(object entityInstance, IEdmEntityTypeReference entityType, IEdmModel model)
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            IEdmEntityObject edmEntityObject = entityInstance as IEdmEntityObject;
            if (edmEntityObject != null)
            {
                return edmEntityObject;
            }
            else
            {
                return new TypedEdmEntityObject(entityInstance, entityType, model);
            }
        }
    }
}
