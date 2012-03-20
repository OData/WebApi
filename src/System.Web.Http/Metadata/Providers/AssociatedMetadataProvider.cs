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
        private static void ApplyMetadataAwareAttributes(IEnumerable<Attribute> attributes, ModelMetadata result)
        {
            foreach (IMetadataAware awareAttribute in attributes.OfType<IMetadataAware>())
            {
                awareAttribute.OnMetadataCreated(result);
            }
        }

        protected abstract ModelMetadata CreateMetadata(IEnumerable<Attribute> attributes, Type containerType, Func<object> modelAccessor, Type modelType, string propertyName);

        protected virtual IEnumerable<Attribute> FilterAttributes(Type containerType, PropertyDescriptor propertyDescriptor, IEnumerable<Attribute> attributes)
        {
            // The Model property on ViewPage and ViewUserControl is marked as ReadOnly
#if false
            if (typeof(ViewPage).IsAssignableFrom(containerType) || typeof(ViewUserControl).IsAssignableFrom(containerType))
            {
                return attributes.Where(a => !(a is ReadOnlyAttribute));
            }
#endif

            return attributes;
        }

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
            foreach (PropertyDescriptor property in GetTypeDescriptor(containerType).GetProperties())
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

            ICustomTypeDescriptor typeDescriptor = GetTypeDescriptor(containerType);
            PropertyDescriptor property = typeDescriptor.GetProperties().Find(propertyName, true);
            if (property == null)
            {
                throw Error.Argument("propertyName", SRResources.Common_PropertyNotFound, containerType, propertyName);
            }

            return GetMetadataForProperty(modelAccessor, containerType, property);
        }

        protected virtual ModelMetadata GetMetadataForProperty(Func<object> modelAccessor, Type containerType, PropertyDescriptor propertyDescriptor)
        {
            IEnumerable<Attribute> attributes = FilterAttributes(containerType, propertyDescriptor, propertyDescriptor.Attributes.Cast<Attribute>());
            ModelMetadata result = CreateMetadata(attributes, containerType, modelAccessor, propertyDescriptor.PropertyType, propertyDescriptor.Name);
            ApplyMetadataAwareAttributes(attributes, result);
            return result;
        }

        public override ModelMetadata GetMetadataForType(Func<object> modelAccessor, Type modelType)
        {
            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            IEnumerable<Attribute> attributes = GetTypeDescriptor(modelType).GetAttributes().Cast<Attribute>();
            ModelMetadata result = CreateMetadata(attributes, null /* containerType */, modelAccessor, modelType, null /* propertyName */);
            ApplyMetadataAwareAttributes(attributes, result);
            return result;
        }

        private static Func<object> GetPropertyValueAccessor(object container, PropertyDescriptor property)
        {
            return () => property.GetValue(container);
        }

        protected virtual ICustomTypeDescriptor GetTypeDescriptor(Type type)
        {
            return TypeDescriptorHelper.Get(type);
        }
    }
}
