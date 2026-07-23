//-----------------------------------------------------------------------------
// <copyright file="SelectExpandWrapper.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    internal abstract class SelectExpandWrapper : IEdmEntityObject, ISelectExpandWrapper
    {
        private static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();
        private static readonly Func<IEdmModel, IEdmStructuredType, IPropertyMapper> _mapperProvider =
            (IEdmModel m, IEdmStructuredType t) => DefaultPropertyMapper;

        private Dictionary<string, object> _containerDict;
        private TypedEdmStructuredObject _typedEdmStructuredObject;

        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public PropertyContainer Container { get; set; }

        /// <summary>
        /// An ID to uniquely identify the model in the <see cref="ModelContainer"/>.
        /// </summary>
        public string ModelID { get; set; }

        /// <inheritdoc />
        public object UntypedInstance { get; set; }

        /// <summary>
        /// Gets or sets the instance type name
        /// </summary>
        public string InstanceType { get; set; }

        /// <summary>
        /// Indicates whether the underlying instance can be used to obtain property values.
        /// </summary>
        public bool UseInstanceForProperties { get; set; }

        /// <inheritdoc />
        public IEdmTypeReference GetEdmType()
        {
            IEdmModel model = GetModel();

            if (InstanceType != null)
            {
                IEdmStructuredType structuredType = model.FindType(InstanceType) as IEdmStructuredType;
                IEdmEntityType entityType = structuredType as IEdmEntityType;

                if (entityType != null)
                {
                    return entityType.ToEdmTypeReference(true);
                }

                return structuredType.ToEdmTypeReference(true);
            }

            Type elementType = GetElementType();
            return model.GetTypeMappingCache().GetEdmType(elementType, model);
        }

        /// <inheritdoc />
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            // look into the container first to see if it has that property. container would have it 
            // if the property was expanded.
            if (Container != null)
            {
                _containerDict = _containerDict ?? Container.ToDictionary(DefaultPropertyMapper, includeAutoSelected: true);
                if (_containerDict.TryGetValue(propertyName, out value))
                {
                    return true;
                }
            }

            // fall back to the instance.
            if (UseInstanceForProperties && UntypedInstance != null)
            {
                IEdmTypeReference edmTypeReference = GetEdmType();
                IEdmModel model = GetModel();

                // Restrict the CLR fallback to properties that are declared as *structural* properties
                // in the EDM model or are the open-type dynamic-property container
                // (IDictionary<string,object> bag). Property names not found in the container that are
                // neither an EDM-declared structural property nor the dynamic container have no basis
                // for CLR resolution and are returned as not found. Navigation properties are excluded
                // so that a declared navigation property that is absent from the container cannot reach
                // the CLR fallback.
                if (edmTypeReference is IEdmStructuredTypeReference structuredTypeRef && model != null)
                {
                    IEdmStructuralProperty structuralProperty =
                        structuredTypeRef.FindProperty(propertyName) as IEdmStructuralProperty;

                    if (structuralProperty == null)
                    {
                        PropertyInfo dynamicContainerProp =
                            EdmLibHelpers.GetDynamicPropertyDictionary(structuredTypeRef.StructuredDefinition(), model);
                        if (dynamicContainerProp == null || dynamicContainerProp.Name != propertyName)
                        {
                            value = null;
                            return false;
                        }
                    }
                }

                IEdmComplexTypeReference complexTypeReference = edmTypeReference as IEdmComplexTypeReference;
                if (complexTypeReference != null)
                {
                    _typedEdmStructuredObject = _typedEdmStructuredObject ??
                    new TypedEdmComplexObject(UntypedInstance, complexTypeReference, model);
                }
                else
                {
                    _typedEdmStructuredObject = _typedEdmStructuredObject ??
                    new TypedEdmEntityObject(UntypedInstance, edmTypeReference as IEdmEntityTypeReference, model);
                }

                return _typedEdmStructuredObject.TryGetPropertyValue(propertyName, out value);
            }

            value = null;
            return false;
        }

        public IDictionary<string, object> ToDictionary()
        {
            return ToDictionary(_mapperProvider);
        }

        public IDictionary<string, object> ToDictionary(Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider)
        {
            if (mapperProvider == null)
            {
                throw Error.ArgumentNull("mapperProvider");
            }

            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            IEdmStructuredType type = GetEdmType().AsStructured().StructuredDefinition();

            IPropertyMapper mapper = mapperProvider(GetModel(), type);
            if (mapper == null)
            {
                throw Error.InvalidOperation(SRResources.InvalidPropertyMapper, typeof(IPropertyMapper).FullName,
                    type.FullTypeName());
            }

            if (Container != null)
            {
                dictionary = Container.ToDictionary(mapper, includeAutoSelected: false);
            }

            // The user asked for all the structural properties on this instance.
            if (UseInstanceForProperties && UntypedInstance != null)
            {
                foreach (IEdmStructuralProperty property in type.StructuralProperties())
                {
                    object propertyValue;
                    if (TryGetPropertyValue(property.Name, out propertyValue))
                    {
                        string mappingName = mapper.MapProperty(property.Name);
                        if (String.IsNullOrWhiteSpace(mappingName))
                        {
                            throw Error.InvalidOperation(SRResources.InvalidPropertyMapping, property.Name);
                        }

                        dictionary[mappingName] = propertyValue;
                    }
                }
            }

            return dictionary;
        }

        protected abstract Type GetElementType();

        private IEdmModel GetModel()
        {
            Contract.Assert(ModelID != null);

            return ModelContainer.GetModel(ModelID);
        }
    }
}
