// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;
using Newtonsoft.Json;

namespace System.Web.OData.Query.Expressions
{
    internal abstract class SelectExpandWrapper : IEdmEntityObject, ISelectExpandWrapper
    {
        /// <summary>
        /// Gets or sets the property container that contains the properties being expanded. 
        /// </summary>
        public PropertyContainer Container { get; set; }

        /// <summary>
        /// Gets or sets the instance of the element being selected and expanded.
        /// </summary>
        public object Instance { get; set; }

        /// <summary>
        /// An ID to uniquely identify the model in the <see cref="ModelContainer"/>.
        /// </summary>
        public string ModelID { get; set; }

        /// <summary>
        /// Indicates whether the underlying instance can be used to obtain property values.
        /// </summary>
        public bool UseInstanceForProperties { get; set; }

        /// <inheritdoc />
        public abstract IEdmTypeReference GetEdmType();

        /// <inheritdoc />
        public abstract IDictionary<string, object> ToDictionary();

        /// <inheritdoc />
        public abstract IDictionary<string, object> ToDictionary(Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider);

        /// <inheritdoc />
        public abstract bool TryGetPropertyValue(string propertyName, out object value);
    }

    /// <summary>
    /// Represents a container class that contains properties that are either selected or expanded using $select and $expand.
    /// </summary>
    /// <typeparam name="TElement">The element being selected and expanded.</typeparam>
    [JsonConverter(typeof(SelectExpandWrapperConverter))]
    internal class SelectExpandWrapper<TElement> : SelectExpandWrapper
    {
        private static readonly IPropertyMapper DefaultPropertyMapper = new IdentityPropertyMapper();
        private static readonly Func<IEdmModel, IEdmStructuredType, IPropertyMapper> _mapperProvider =
            (IEdmModel m, IEdmStructuredType t) => DefaultPropertyMapper;

        private Dictionary<string, object> _containerDict;
        private TypedEdmEntityObject _typedEdmEntityObject;

        /// <inheritdoc />
        public override IEdmTypeReference GetEdmType()
        {
            IEdmModel model = GetModel();
            Type elementType = GetElementType();
            return model.GetEdmTypeReference(elementType);
        }

        /// <inheritdoc />
        public override bool TryGetPropertyValue(string propertyName, out object value)
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
            if (UseInstanceForProperties && Instance != null)
            {
                _typedEdmEntityObject = _typedEdmEntityObject ??
                    new TypedEdmEntityObject(Instance, GetEdmType() as IEdmEntityTypeReference, GetModel());

                return _typedEdmEntityObject.TryGetPropertyValue(propertyName, out value);
            }

            value = null;
            return false;
        }

        public override IDictionary<string, object> ToDictionary()
        {
            return ToDictionary(_mapperProvider);
        }

        public override IDictionary<string, object> ToDictionary(Func<IEdmModel, IEdmStructuredType, IPropertyMapper> mapperProvider)
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
            if (UseInstanceForProperties && Instance != null)
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

        private Type GetElementType()
        {
            return Instance == null ? typeof(TElement) : Instance.GetType();
        }

        private IEdmModel GetModel()
        {
            Contract.Assert(ModelID != null);

            return ModelContainer.GetModel(ModelID);
        }
    }
}
