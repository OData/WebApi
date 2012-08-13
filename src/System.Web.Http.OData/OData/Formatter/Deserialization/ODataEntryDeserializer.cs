// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
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

        public IEdmTypeReference EdmType { get; private set; }

        public IEdmModel EdmModel
        {
            get
            {
                return DeserializerProvider != null ? DeserializerProvider.EdmModel : null;
            }
        }

        public ODataDeserializerProvider DeserializerProvider { get; private set; }

        public virtual object ReadInline(object item, ODataDeserializerReadContext readContext)
        {
            throw Error.NotSupported(SRResources.DoesNotSupportReadInLine, this.GetType().Name);
        }

        internal static void RecurseEnter(ODataDeserializerReadContext readContext)
        {
            if (!readContext.IncrementCurrentReferenceDepth())
            {
                throw Error.InvalidOperation(SRResources.RecursionLimitExceeded);
            }
        }

        internal static void RecurseLeave(ODataDeserializerReadContext readContext)
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

        internal static IList CreateNewCollection()
        {
            return new List<object>();
        }

        internal static void ApplyProperty(ODataProperty property, IEdmStructuredTypeReference resourceType, object resource, ODataDeserializerProvider deserializerProvider, ODataDeserializerReadContext readContext)
        {
            IEdmProperty edmProperty = resourceType.FindProperty(property.Name);

            string propertyName = property.Name;
            IEdmTypeReference propertyType = edmProperty != null ? edmProperty.Type : null; // open properties have null values

            object value = ConvertValue(property.Value, ref propertyType, deserializerProvider, readContext);

            // If we are in patch mode and we are deserializing an entity object then we are updating Delta<T> and not T.
            if (!readContext.IsPatchMode || !resourceType.IsEntity())
            {
                resource.GetType().GetProperty(propertyName).SetValue(resource, value, index: null);
            }
            else
            {
                (resource as IDelta).TrySetPropertyValue(propertyName, value);
            }
        }

        internal static object ConvertValue(object oDataValue, ref IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider, ODataDeserializerReadContext readContext)
        {
            if (oDataValue == null)
            {
                return null;
            }

            ODataComplexValue complexValue = oDataValue as ODataComplexValue;
            if (complexValue != null)
            {
                return ConvertComplexValue(complexValue, ref propertyType, deserializerProvider, readContext);
            }

            ODataCollectionValue collection = oDataValue as ODataCollectionValue;
            if (collection != null)
            {
                Contract.Assert(propertyType != null, "Open collection properties are not supported.");
                return ConvertCollectionValue(collection, propertyType, deserializerProvider, readContext);
            }

            return ConvertPrimitiveValue(oDataValue, ref propertyType);
        }

        private static object ConvertComplexValue(ODataComplexValue complexValue, ref IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider, ODataDeserializerReadContext readContext)
        {
            IEdmComplexTypeReference edmComplexType;
            if (propertyType == null)
            {
                // open complex property
                Contract.Assert(!string.IsNullOrEmpty(complexValue.TypeName), "ODataLib should have verified that open complex value has a type name since we provided metadata.");
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

        private static object ConvertCollectionValue(ODataCollectionValue collection, IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider, ODataDeserializerReadContext readContext)
        {
            IEdmCollectionType collectionType = propertyType as IEdmCollectionType;
            Contract.Assert(collectionType != null, "The type for collection must be a IEdmCollectionType.");

            IList collectionList = CreateNewCollection();

            RecurseEnter(readContext);

            Contract.Assert(collection.Items != null, "The ODataLib reader should always populate the ODataCollectionValue.Items collection.");
            foreach (object odataItem in collection.Items)
            {
                IEdmTypeReference itemType = collectionType.ElementType;
                collectionList.Add(ConvertValue(odataItem, ref itemType, deserializerProvider, readContext));
            }

            RecurseLeave(readContext);

            return collectionList;
        }
    }
}
