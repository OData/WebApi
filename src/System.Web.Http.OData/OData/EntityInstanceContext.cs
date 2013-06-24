// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Formatter.Deserialization;
using System.Web.Http.OData.Formatter.Serialization;
using System.Web.Http.OData.Properties;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
{
    /// <summary>
    /// An instance of <see cref="EntityInstanceContext{TEntityType}"/> gets passed to the self link (<see cref="M:EntitySetConfiguration.HasIdLink"/>, <see cref="M:EntitySetConfiguration.HasEditLink"/>, <see cref="M:EntitySetConfiguration.HasReadLink"/>)
    /// and navigation link (<see cref="M:EntitySetConfiguration.HasNavigationPropertyLink"/>, <see cref="M:EntitySetConfiguration.HasNavigationPropertiesLink"/>) builders and can be used by the link builders to generate links.
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
            : this(serializerContext, entityType, AsEdmStructuredObject(entityInstance, entityType))
        {
        }

        private EntityInstanceContext(ODataSerializerContext serializerContext, IEdmEntityTypeReference entityType, IEdmStructuredObject edmObject)
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
        /// Gets or sets the <see cref="IEdmEntitySet"/> to which this instance belongs.
        /// </summary>
        public IEdmEntitySet EntitySet
        {
            get
            {
                return SerializerContext.EntitySet;
            }
            set
            {
                SerializerContext.EntitySet = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEdmEntityType"/> of this entity instance.
        /// </summary>
        public IEdmEntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmStructuredObject"/> backing this instance.
        /// </summary>
        public IEdmStructuredObject EdmObject { get; set; }

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
            if (EdmObject.TryGetValue(propertyName, out value))
            {
                return value;
            }
            else
            {
                IEdmTypeReference edmType = EdmObject.GetEdmType();
                if (edmType == null)
                {
                    throw Error.InvalidOperation(SRResources.EdmTypeCannotBeNull, EdmObject.GetType(), typeof(IEdmObject));
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

            EdmStructuredObject edmStructuredObject = EdmObject as EdmStructuredObject;
            if (edmStructuredObject != null)
            {
                return edmStructuredObject.Instance;
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
                if (EdmObject.TryGetValue(property.Name, out value) && value != null)
                {
                    if (value.GetType().IsCollection())
                    {
                        DeserializationHelpers.SetCollectionProperty(resource, property.Name, isDelta: false, value: value);
                    }
                    else
                    {
                        DeserializationHelpers.SetProperty(resource, property.Name, isDelta: false, value: value);
                    }
                }
            }

            return resource;
        }

        private static IEdmStructuredObject AsEdmStructuredObject(object entityInstance, IEdmEntityTypeReference entityType)
        {
            if (entityType == null)
            {
                throw Error.ArgumentNull("entityType");
            }

            IEdmStructuredObject edmObject = entityInstance as IEdmStructuredObject;
            if (edmObject != null)
            {
                return edmObject;
            }
            else
            {
                return new EdmStructuredObject(entityInstance, entityType);
            }
        }
    }
}
