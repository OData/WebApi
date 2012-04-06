// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

namespace System.Web.Http.Metadata.Providers
{
    // This class provides a good implementation of ModelMetadataProvider for people who will be
    // using traditional classes with properties. It uses the buddy class support from
    // DataAnnotations, and consolidates the three operations down to a single override
    // for reading the attribute values and creating the metadata class.
    public abstract class AssociatedMetadataProvider : ModelMetadataProvider
    {
        protected abstract ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName);

        public override IEnumerable<ModelMetadata> GetMetadataForProperties(object container, Type containerType)
        {
            if (containerType == null)
            {
                throw Error.ArgumentNull("containerType");
            }

            return GetMetadataForPropertiesImpl(container, containerType);
        }

        private IEnumerable<ModelMetadata> GetMetadataForPropertiesImpl(object container, Type containerType)
        {
            foreach (PropertyDescriptor property in GetProperties(containerType))
            {
                Func<object> modelAccessor = container == null ? null : GetPropertyValueAccessor(container, property);
                yield return GetMetadataForProperty(modelAccessor, containerType, property);
            }
        }

        public override ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, string propertyName)
        {
            if (containerType == null)
            {
                throw Error.ArgumentNull("containerType");
            }
            if (String.IsNullOrEmpty(propertyName))
            {
                throw Error.ArgumentNullOrEmpty("propertyName");
            }

            PropertyDescriptor property = GetProperties(containerType).Find(propertyName, true);
            if (property == null)
            {
                throw Error.Argument("propertyName", SRResources.Common_PropertyNotFound, containerType, propertyName);
            }

            return GetMetadataForProperty(modelAccessor, containerType, property);
        }

        protected virtual ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, PropertyDescriptor propertyDescriptor)
        {
            IEnumerable<Attribute> attributes = propertyDescriptor.Attributes.Cast<Attribute>();
            return CreateMetadata(attributes, containerType, modelAccessor, propertyDescriptor.PropertyType, propertyDescriptor.Name);
        }

        public override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            IEnumerable<Attribute> attributes = GetTypeDescriptor(modelType).GetAttributes().Cast<Attribute>();
            return CreateMetadata(attributes, null /* containerType */, modelAccessor, modelType, null /* propertyName */);
        }

        private static Func<object> GetPropertyValueAccessor(object container, PropertyDescriptor property)
        {
            return () => property.GetValue(container);
        }

        protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            return TypeDescriptorHelper.Get(type);
        }

        protected virtual PropertyDescriptorCollection GetProperties(Type type)
        {
            return GetTypeDescriptor(type).GetProperties();
        }
    }
}
