//-----------------------------------------------------------------------------
// <copyright file="ResourceContext.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query.Expressions;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An instance of <see cref="ResourceContext"/> gets passed to the self link (
    /// <see cref="M:NavigationSourceConfiguration.HasIdLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasEditLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasReadLink"/>
    /// ) and navigation link (
    /// <see cref="M:NavigationSourceConfiguration.HasNavigationPropertyLink"/>,
    /// <see cref="M:NavigationSourceConfiguration.HasNavigationPropertiesLink"/>
    /// ) builders and can be used by the link builders to generate links.
    /// </summary>
    public partial class ResourceContext
    {
        private object _resourceInstance;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceContext"/> class.
        /// </summary>
        public ResourceContext()
        {
            SerializerContext = new ODataSerializerContext();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceContext"/> class.
        /// </summary>
        /// <param name="serializerContext">The backing <see cref="ODataSerializerContext"/>.</param>
        /// <param name="structuredType">The EDM structured type of this instance context.</param>
        /// <param name="resourceInstance">The object representing the instance of this context.</param>
        public ResourceContext(ODataSerializerContext serializerContext, IEdmStructuredTypeReference structuredType, object resourceInstance)
            : this(serializerContext, structuredType, AsEdmResourceObject(resourceInstance, structuredType, serializerContext.Model))
        {
        }

        private ResourceContext(ODataSerializerContext serializerContext, IEdmStructuredTypeReference structuredType, IEdmStructuredObject edmObject)
        {
            if (serializerContext == null)
            {
                throw Error.ArgumentNull("serializerContext");
            }

            SerializerContext = serializerContext;
            StructuredType = structuredType.StructuredDefinition();
            EdmObject = edmObject;
        }

        /// <summary>
        /// Gets or sets the <see cref="ODataSerializerContext"/>.
        /// </summary>
        public ODataSerializerContext SerializerContext { get; set; }

        /// <summary>
        /// Gets or sets the HTTP request that caused this instance to be generated.
        /// </summary>
        internal IWebApiRequestMessage InternalRequest
        {
            get
            {
                return SerializerContext.InternalRequest;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IEdmModel"/> to which this instance belongs.
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

        internal bool IsPostRequest
        {
            get
            {
#if NETCORE
                return Request == null ? false : String.Equals(Request.Method, "post", StringComparison.OrdinalIgnoreCase);
#else
                return Request == null ? false : String.Equals(Request.Method.ToString(), "post", StringComparison.OrdinalIgnoreCase);
#endif
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
        /// Gets or sets the <see cref="IEdmStructuredType"/> of this resource instance.
        /// </summary>
        public IEdmStructuredType StructuredType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IEdmStructuredObject"/> backing this instance.
        /// </summary>
        public IEdmStructuredObject EdmObject { get; set; }

        /// <summary>
        /// Gets or sets the value of this resource instance.
        /// </summary>
        public object ResourceInstance
        {
            get
            {
                if (_resourceInstance == null)
                {
                    _resourceInstance = BuildResourceInstance();
                }

                return _resourceInstance;
            }
            set
            {
                _resourceInstance = value;
            }
        }

        /// <summary>
        /// Gets or sets a <see cref="IWebApiUrlHelper"/> that may be used to generate links while serializing this resource
        /// instance.
        /// </summary>
        internal IWebApiUrlHelper InternalUrlHelper
        {
            get
            {
                return SerializerContext.InternalUrlHelper;
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
        /// Gets or sets the dynamic complex or collection of complex properties should be nested in this instance.
        /// </summary>
        /// <remarks>
        /// The key is the dynamic property name.
        /// The value is the dynamic property value.
        /// </remarks>
        [SuppressMessage("Microsoft.Usage", "CA2227:EnableSetterForProperty", Justification = "Enable setter for dictionary property")]
        public IDictionary<string, object> DynamicComplexProperties { get; set; }

        /// <summary>
        /// Gets the value of the property with the given name from the <see cref="IEdmObject"/> of this instance if present; throws if the property is
        /// not present.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <returns>The value of the property if present.</returns>
        public object GetPropertyValue(string propertyName)
        {
            if (EdmObject == null)
            {
                throw Error.InvalidOperation(SRResources.EdmObjectNull, typeof(ResourceContext).Name);
            }

            object value;
            if (SerializerContext.IsDeltaOfT)
            {
                IDelta delta = ResourceInstance as IDelta;
                if (delta != null && delta.TryGetPropertyValue(propertyName, out value))
                {
                    return value;
                }
            }
                        
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

        private object BuildResourceInstance()
        {
            if (EdmObject == null)
            {
                return null;
            }

            TypedEdmStructuredObject edmStructruredObject = EdmObject as TypedEdmStructuredObject;
            if (edmStructruredObject != null)
            {
                return edmStructruredObject.Instance;
            }

            SelectExpandWrapper selectExpandWrapper = EdmObject as SelectExpandWrapper;
            if (selectExpandWrapper != null && selectExpandWrapper.UntypedInstance != null)
            {
                return selectExpandWrapper.UntypedInstance;
            }

            Type clrType = EdmLibHelpers.GetClrType(StructuredType, EdmModel);
            if (clrType == null)
            {
                throw new InvalidOperationException(Error.Format(SRResources.MappingDoesNotContainResourceType, StructuredType.FullTypeName()));
            }

            object resource = Activator.CreateInstance(clrType);
            foreach (IEdmStructuralProperty property in StructuredType.StructuralProperties())
            {
                object value;
                if (EdmObject.TryGetPropertyValue(property.Name, out value) && value != null)
                {
                    if (value is SelectExpandWrapper)
                    {
                        // Skip the select expand property
                        continue;
                    }

                    string propertyName = EdmLibHelpers.GetClrPropertyName(property, EdmModel);

                    if (TypeHelper.IsCollection(value.GetType()))
                    {
                        DeserializationHelpers.SetCollectionProperty(resource, property, value, propertyName);
                    }
                    else
                    {
                        DeserializationHelpers.SetProperty(resource, propertyName, value);
                    }
                }
            }

            return resource;
        }

        private static IEdmStructuredObject AsEdmResourceObject(object resourceInstance, IEdmStructuredTypeReference structuredType, IEdmModel model)
        {
            if (structuredType == null)
            {
                throw Error.ArgumentNull("structuredType");
            }

            IEdmStructuredObject edmStructuredObject = resourceInstance as IEdmStructuredObject;
            if (edmStructuredObject != null)
            {
                return edmStructuredObject;
            }

            if (structuredType.IsEntity())
            {
                return new TypedEdmEntityObject(resourceInstance, structuredType.AsEntity(), model);
            }

            Contract.Assert(structuredType.IsComplex());
            return new TypedEdmComplexObject(resourceInstance, structuredType.AsComplex(), model);
        }
    }
}
