// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    internal abstract class SelectExpandWrapper : ISelectExpandWrapper
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

        /// <summary>
        /// The instance of the object that is being wrapped, if available.
        /// </summary>
        // Ideally this would be called "Instance" and SelectExpandWrapper<> would use the "new"
        // keyword to replace it with a variation that returns the correct type. Unfortunately the
        // scenarios that consume "Instance" do so using reflection, and such an approach would
        // introduce ambiguity that would need to be handled in numerous places. Since this is an
        // internal construct it fine to leave like this.
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
        object ISelectExpandWrapper.Instance
        {
            get { return UntypedInstance; }
        }

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
                if (GetEdmType() is IEdmComplexTypeReference)
                {
                    _typedEdmStructuredObject = _typedEdmStructuredObject ??
                    new TypedEdmComplexObject(UntypedInstance, GetEdmType() as IEdmComplexTypeReference, GetModel());
                }
                else
                {
                    _typedEdmStructuredObject = _typedEdmStructuredObject ??
                    new TypedEdmEntityObject(UntypedInstance, GetEdmType() as IEdmEntityTypeReference, GetModel());
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
