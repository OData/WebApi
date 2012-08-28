// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Web.Http.OData.Properties;
using System.Xml.Linq;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Base class for all <see cref="ODataDeserializer" />'s that deserialize into an object backed by <see cref="IEdmType"/>.
    /// </summary>
    public abstract class ODataEntryDeserializer : ODataDeserializer
    {
        protected ODataEntryDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind)
            : base(payloadKind)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            EdmType = edmType;
        }

        protected ODataEntryDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind, ODataDeserializerProvider deserializerProvider)
            : this(edmType, payloadKind)
        {
            DeserializerProvider = deserializerProvider;
        }

        /// <summary>
        /// The edm type.
        /// </summary>
        public IEdmTypeReference EdmType { get; private set; }

        public IEdmModel EdmModel
        {
            get
            {
                return DeserializerProvider != null ? DeserializerProvider.EdmModel : null;
            }
        }

        /// <summary>
        /// The <see cref="ODataDeserializerProvider"/> to use for deserializing inner items.
        /// </summary>
        public ODataDeserializerProvider DeserializerProvider { get; private set; }

        /// <summary>
        /// Deserializes the item into a new object of type corresponding to <see cref="EdmType"/>.
        /// </summary>
        /// <param name="item">The item to deserialize.</param>
        /// <param name="readContext">The <see cref="ODataDeserializerContext"/></param>
        /// <returns>The deserialized object.</returns>
        public virtual object ReadInline(object item, ODataDeserializerContext readContext)
        {
            throw Error.NotSupported(SRResources.DoesNotSupportReadInLine, GetType().Name);
        }

        internal static void RecurseEnter(ODataDeserializerContext readContext)
        {
            if (!readContext.IncrementCurrentReferenceDepth())
            {
                throw Error.InvalidOperation(SRResources.RecursionLimitExceeded);
            }
        }

        internal static void RecurseLeave(ODataDeserializerContext readContext)
        {
            readContext.DecrementCurrentReferenceDepth();
        }

        internal static object CreateResource(IEdmComplexType edmComplexType, IEdmModel edmModel)
        {
            Type clrType = EdmLibHelpers.GetClrType(new EdmComplexTypeReference(edmComplexType, isNullable: true), edmModel);
            if (clrType == null)
            {
                throw Error.Argument("edmComplexType", SRResources.MappingDoesNotContainEntityType, edmComplexType.FullName());
            }

            return Activator.CreateInstance(clrType);
        }

        internal static IList CreateNewCollection(Type elementType)
        {
            return Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)) as IList;
        }

        internal static void ApplyProperty(ODataProperty property, IEdmStructuredTypeReference resourceType, object resource, ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmProperty edmProperty = resourceType.FindProperty(property.Name);

            string propertyName = property.Name;
            IEdmTypeReference propertyType = edmProperty != null ? edmProperty.Type : null; // open properties have null values

            EdmTypeKind propertyKind;
            object value = ConvertValue(property.Value, ref propertyType, deserializerProvider, readContext, out propertyKind);

            bool isDelta = readContext.IsPatchMode && resourceType.IsEntity();

            if (propertyKind == EdmTypeKind.Primitive)
            {
                value = ConvertPrimitiveValue(value, GetPropertyType(resource, propertyName, isDelta), propertyName, resource.GetType().FullName);
            }

            SetProperty(resource, propertyName, isDelta, value);
        }

        internal static void SetProperty(object resource, string propertyName, bool isDelta, object value)
        {
            if (!isDelta)
            {
                resource.GetType().GetProperty(propertyName).SetValue(resource, value, index: null);
            }
            else
            {
                // If we are in patch mode and we are deserializing an entity object then we are updating Delta<T> and not T.
                (resource as IDelta).TrySetPropertyValue(propertyName, value);
            }
        }

        internal static object ConvertValue(object oDataValue, ref IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext, out EdmTypeKind typeKind)
        {
            if (oDataValue == null)
            {
                typeKind = EdmTypeKind.None;
                return null;
            }

            ODataComplexValue complexValue = oDataValue as ODataComplexValue;
            if (complexValue != null)
            {
                typeKind = EdmTypeKind.Complex;
                return ConvertComplexValue(complexValue, ref propertyType, deserializerProvider, readContext);
            }

            ODataCollectionValue collection = oDataValue as ODataCollectionValue;
            if (collection != null)
            {
                typeKind = EdmTypeKind.Collection;
                Contract.Assert(propertyType != null, "Open collection properties are not supported.");
                return ConvertCollectionValue(collection, propertyType, deserializerProvider, readContext);
            }

            typeKind = EdmTypeKind.Primitive;
            return ConvertPrimitiveValue(oDataValue, ref propertyType);
        }

        internal static Type GetPropertyType(object resource, string propertyName, bool isDelta)
        {
            Contract.Assert(resource != null);
            Contract.Assert(propertyName != null);

            if (isDelta)
            {
                IDelta delta = resource as IDelta;
                Contract.Assert(delta != null);

                Type type;
                delta.TryGetPropertyType(propertyName, out type);
                return type;
            }
            else
            {
                PropertyInfo property = resource.GetType().GetProperty(propertyName);
                return property == null ? null : property.PropertyType;
            }
        }

        internal static object ConvertPrimitiveValue(object value, Type type, string propertyName, string typeName)
        {
            Contract.Assert(value != null);
            Contract.Assert(type != null);

            // if value is of the same type nothing to do here.
            if (value.GetType() == type || value.GetType() == Nullable.GetUnderlyingType(type))
            {
                return value;
            }

            string str = value as string;

            if (type == typeof(char))
            {
                if (str == null || str.Length != 1)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeStringLengthOne, propertyName, typeName));
                }

                return str[0];
            }
            else if (type == typeof(char?))
            {
                if (str == null || str.Length > 1)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeStringMaxLengthOne, propertyName, typeName));
                }

                return str.Length > 0 ? str[0] : (char?)null;
            }
            else if (type == typeof(char[]))
            {
                if (str == null)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeString, propertyName, typeName));
                }

                return str.ToCharArray();
            }
            else if (type == typeof(Binary))
            {
                return new Binary((byte[])value);
            }
            else if (type == typeof(XElement))
            {
                if (str == null)
                {
                    throw new ValidationException(Error.Format(SRResources.PropertyMustBeString, propertyName, typeName));
                }

                return XElement.Parse(str);
            }
            else
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                Contract.Assert(type == typeof(uint) || type == typeof(ushort) || type == typeof(ulong));

                // Note that we are not casting the return value to nullable<T> as even if we do it
                // CLR would unbox it back to T.
                return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
        }

        private static object ConvertComplexValue(ODataComplexValue complexValue, ref IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmComplexTypeReference edmComplexType;
            if (propertyType == null)
            {
                // open complex property
                Contract.Assert(!String.IsNullOrEmpty(complexValue.TypeName), "ODataLib should have verified that open complex value has a type name since we provided metadata.");
                IEdmType edmType = deserializerProvider.EdmModel.FindType(complexValue.TypeName);
                Contract.Assert(edmType.TypeKind == EdmTypeKind.Complex, "ODataLib should have verified that complex value has a complex resource type.");
                edmComplexType = new EdmComplexTypeReference(edmType as IEdmComplexType, isNullable: true);
            }
            else
            {
                edmComplexType = propertyType.AsComplex();
            }

            ODataEntryDeserializer deserializer = deserializerProvider.GetODataDeserializer(edmComplexType);
            return deserializer.ReadInline(complexValue, readContext);
        }

        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyType", Justification = "TODO: remove when implement TODO below")]
        private static object ConvertPrimitiveValue(object oDataValue, ref IEdmTypeReference propertyType)
        {
            // TODO: Bug 467612: check for type conversion issues here
            Contract.Assert(propertyType == null || propertyType.TypeKind() == EdmTypeKind.Primitive, "Only primitive types are supported by this method.");

            return oDataValue;
        }

        private static object ConvertCollectionValue(ODataCollectionValue collection, IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmCollectionTypeReference collectionType = propertyType as IEdmCollectionTypeReference;
            Contract.Assert(collectionType != null, "The type for collection must be a IEdmCollectionType.");

            IList collectionList = CreateNewCollection(EdmLibHelpers.GetClrType(collectionType.ElementType(), deserializerProvider.EdmModel));

            RecurseEnter(readContext);

            Contract.Assert(collection.Items != null, "The ODataLib reader should always populate the ODataCollectionValue.Items collection.");
            foreach (object odataItem in collection.Items)
            {
                IEdmTypeReference itemType = collectionType.ElementType();
                EdmTypeKind propertyKind;
                collectionList.Add(ConvertValue(odataItem, ref itemType, deserializerProvider, readContext, out propertyKind));
                Contract.Assert(propertyKind != EdmTypeKind.Primitive, "no collection property support yet.");
            }

            RecurseLeave(readContext);

            return collectionList;
        }
    }
}
