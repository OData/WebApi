// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Caching;

namespace System.Web.Http.Metadata.Providers
{
    public abstract class CachedAssociatedMetadataProvider<TModelMetadata> : AssociatedMetadataProvider
        where TModelMetadata : ModelMetadata
    {
        private ConcurrentDictionary<Type, ICustomTypeDescriptor> _typeDescriptorCache = new ConcurrentDictionary<Type, ICustomTypeDescriptor>();
        private ConcurrentDictionary<Type, PropertyDescriptorCollection> _propertyCache = new ConcurrentDictionary<Type, PropertyDescriptorCollection>();
        // Keyed on (Type, propertyName) tuple
        private ConcurrentDictionary<Tuple<Type, string>, ModelMetadata> _prototypeCache = new ConcurrentDictionary<Tuple<Type, string>, ModelMetadata>();

        protected sealed override ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName)
        {
            // If metadata is being created for a property then containerType != null && propertyName != null
            // If metadata is being created for a type then containerType == null && propertyName == null, so we have to use modelType for the cache key.
            Type typeForCache = containerType ?? modelType;
            TModelMetadata prototype = _prototypeCache.GetOrAdd(
                    Tuple.Create(typeForCache, propertyName),
                    key => CreateMetadataPrototype(attributes, containerType, modelType, propertyName)) as TModelMetadata;

            return CreateMetadataFromPrototype(prototype, modelAccessor);
        }

        // New override for creating the prototype metadata (without the accessor)
        protected abstract TModelMetadata CreateMetadataPrototype(IEnumerable<Attribute> attributes, Type containerType, Type modelType, string propertyName);

        // New override for applying the prototype + modelAccess to yield the final metadata
        protected abstract TModelMetadata CreateMetadataFromPrototype(TModelMetadata prototype, Func<object> modelAccessor);

        public sealed override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName)
        {
            return base.GetMetadataForProperty(modelAccessor, containerType, propertyName);
        }

        protected sealed override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, PropertyDescriptor propertyDescriptor)
        {
            return base.GetMetadataForProperty(modelAccessor, containerType, propertyDescriptor);
        }

        public sealed override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType)
        {
            return base.GetMetadataForProperties(container, containerType);
        }

        public sealed override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
        {
            return base.GetMetadataForType(modelAccessor, modelType);
        }

        protected sealed override ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            return _typeDescriptorCache.GetOrAdd(
                type,
                key => base.GetTypeDescriptor(type));
        }

        protected sealed override PropertyDescriptorCollection GetProperties(Type type)
        {
            return _propertyCache.GetOrAdd(
                type,
                key => base.GetProperties(type));
        }
    }
}
