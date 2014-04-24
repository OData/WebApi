// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace System.Web.OData.Formatter.Deserialization
{
    internal static class DeserializationHelpers
    {
        internal static void ApplyProperty(ODataProperty property, IEdmStructuredTypeReference resourceType, object resource,
            ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmProperty edmProperty = resourceType.FindProperty(property.Name);

            // try to deserializer the dynamic properties for open type.
            if (edmProperty == null)
            {
                // the logic here works for open complex type and open entity type.
                IEdmStructuredType structuredType = resourceType.StructuredDefinition();
                if (structuredType != null && structuredType.IsOpen)
                {
                    if (ApplyDynamicProperty(property, structuredType, resource, deserializerProvider, readContext))
                    {
                        return;
                    }
                }
            }

            string propertyName = property.Name;
            if (edmProperty != null)
            {
                propertyName = EdmLibHelpers.GetClrPropertyName(edmProperty, readContext.Model);
            }
            IEdmTypeReference propertyType = edmProperty != null ? edmProperty.Type : null; // open properties have null values

            EdmTypeKind propertyKind;
            object value = ConvertValue(property.Value, ref propertyType, deserializerProvider, readContext, out propertyKind);

            if (propertyKind == EdmTypeKind.Collection)
            {
                SetCollectionProperty(resource, edmProperty, value, propertyName);
            }
            else
            {
                if (!readContext.IsUntyped)
                {
                    if (propertyKind == EdmTypeKind.Primitive)
                    {
                        value = EdmPrimitiveHelpers.ConvertPrimitiveValue(value, GetPropertyType(resource, propertyName));
                    }
                    else if (propertyKind == EdmTypeKind.Enum)
                    {
                        value = EnumDeserializationHelpers.ConvertEnumValue(value, GetPropertyType(resource, propertyName));
                    }
                }

                SetProperty(resource, propertyName, value);
            }
        }

        internal static void SetCollectionProperty(object resource, IEdmProperty edmProperty, object value, string propertyName)
        {
            Contract.Assert(edmProperty != null);

            SetCollectionProperty(resource, propertyName, edmProperty.Type.AsCollection(), value, clearCollection: false);
        }

        internal static void SetCollectionProperty(object resource, string propertyName,
            IEdmCollectionTypeReference edmPropertyType, object value, bool clearCollection)
        {
            if (value != null)
            {
                IEnumerable collection = value as IEnumerable;
                Contract.Assert(collection != null,
                    "SetCollectionProperty is always passed the result of ODataFeedDeserializer or ODataCollectionDeserializer");

                Type resourceType = resource.GetType();
                Type propertyType = GetPropertyType(resource, propertyName);

                Type elementType;
                if (!propertyType.IsCollection(out elementType))
                {
                    string message = Error.Format(SRResources.PropertyIsNotCollection, propertyType.FullName, propertyName, resourceType.FullName);
                    throw new SerializationException(message);
                }

                IEnumerable newCollection;
                if (CanSetProperty(resource, propertyName) &&
                    CollectionDeserializationHelpers.TryCreateInstance(propertyType, edmPropertyType, elementType, out newCollection))
                {
                    // settable collections
                    collection.AddToCollection(newCollection, elementType, resourceType, propertyName, propertyType);
                    if (propertyType.IsArray)
                    {
                        newCollection = CollectionDeserializationHelpers.ToArray(newCollection, elementType);
                    }

                    SetProperty(resource, propertyName, newCollection);
                }
                else
                {
                    // get-only collections.
                    newCollection = GetProperty(resource, propertyName) as IEnumerable;
                    if (newCollection == null)
                    {
                        string message = Error.Format(SRResources.CannotAddToNullCollection, propertyName, resourceType.FullName);
                        throw new SerializationException(message);
                    }

                    if (clearCollection)
                    {
                        newCollection.Clear(propertyName, resourceType);
                    }

                    collection.AddToCollection(newCollection, elementType, resourceType, propertyName, propertyType);
                }
            }
        }

        internal static void SetProperty(object resource, string propertyName, object value)
        {
            IDelta delta = resource as IDelta;
            if (delta == null)
            {
                resource.GetType().GetProperty(propertyName).SetValue(resource, value, index: null);
            }
            else
            {
                delta.TrySetPropertyValue(propertyName, value);
            }
        }

        internal static object ConvertValue(object oDataValue, ref IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider,
            ODataDeserializerContext readContext, out EdmTypeKind typeKind)
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

            if (oDataValue is ODataEnumValue)
            {
                typeKind = EdmTypeKind.Enum;   
            }
            else
            {
                typeKind = EdmTypeKind.Primitive;
            }

            return oDataValue;
        }

        internal static Type GetPropertyType(object resource, string propertyName)
        {
            Contract.Assert(resource != null);
            Contract.Assert(propertyName != null);

            IDelta delta = resource as IDelta;
            if (delta != null)
            {
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

        private static object ConvertComplexValue(ODataComplexValue complexValue, ref IEdmTypeReference propertyType,
            ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmComplexTypeReference edmComplexType;
            if (propertyType == null)
            {
                // open complex property
                Contract.Assert(!String.IsNullOrEmpty(complexValue.TypeName),
                    "ODataLib should have verified that open complex value has a type name since we provided metadata.");
                IEdmModel model = readContext.Model;
                IEdmType edmType = model.FindType(complexValue.TypeName);
                Contract.Assert(edmType.TypeKind == EdmTypeKind.Complex, "ODataLib should have verified that complex value has a complex resource type.");
                edmComplexType = new EdmComplexTypeReference(edmType as IEdmComplexType, isNullable: true);
                propertyType = edmComplexType;
            }
            else
            {
                edmComplexType = propertyType.AsComplex();
            }

            ODataEdmTypeDeserializer deserializer = deserializerProvider.GetEdmTypeDeserializer(edmComplexType);
            return deserializer.ReadInline(complexValue, propertyType, readContext);
        }

        private static bool CanSetProperty(object resource, string propertyName)
        {
            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                return true;
            }
            else
            {
                PropertyInfo property = resource.GetType().GetProperty(propertyName);
                return property != null && property.GetSetMethod() != null;
            }
        }

        private static object GetProperty(object resource, string propertyName)
        {
            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                object value;
                delta.TryGetPropertyValue(propertyName, out value);
                return value;
            }
            else
            {
                PropertyInfo property = resource.GetType().GetProperty(propertyName);
                Contract.Assert(property != null, "ODataLib should have already verified that the property exists on the type.");
                return property.GetValue(resource, index: null);
            }
        }

        private static object ConvertCollectionValue(ODataCollectionValue collection, IEdmTypeReference propertyType,
            ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmCollectionTypeReference collectionType = propertyType as IEdmCollectionTypeReference;
            Contract.Assert(collectionType != null, "The type for collection must be a IEdmCollectionType.");

            ODataEdmTypeDeserializer deserializer = deserializerProvider.GetEdmTypeDeserializer(collectionType);
            return deserializer.ReadInline(collection, collectionType, readContext);
        }

        private static bool ApplyDynamicProperty(ODataProperty property, IEdmStructuredType structuredType,
            object resource, ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            PropertyInfo propertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(structuredType,
                        readContext.Model);
            if (propertyInfo == null)
            {
                return false;
            }

            IDictionary<string, object> dynamicPropertyDictionary = propertyInfo.GetValue(resource)
                as IDictionary<string, object>;

            if (dynamicPropertyDictionary == null)
            {
                dynamicPropertyDictionary = new Dictionary<string, object>();
                propertyInfo.SetValue(resource, dynamicPropertyDictionary);
            }

            if (dynamicPropertyDictionary.ContainsKey(property.Name))
            {
                throw Error.InvalidOperation(SRResources.DuplicateDynamicPropertyNameFound,
                    property.Name, structuredType.FullTypeName());
            }

            EdmTypeKind propertyKind;
            IEdmTypeReference propertyType = null;
            object value = ConvertValue(property.Value, ref propertyType, deserializerProvider, readContext, out propertyKind);

            if (propertyKind == EdmTypeKind.Collection)
            {
                throw Error.InvalidOperation(SRResources.CollectionNotAllowedAsDynamicProperty, property.Name);
            }

            if (propertyKind == EdmTypeKind.Enum)
            {
                ODataEnumValue enumValue = (ODataEnumValue)value;
                IEdmModel model = readContext.Model;
                IEdmType edmType = model.FindType(enumValue.TypeName);
                if (edmType == null)
                {
                    return false;
                }

                Type enumType = EdmLibHelpers.GetClrType(edmType, model);
                value = Enum.Parse(enumType, enumValue.Value);
            }

            dynamicPropertyDictionary.Add(property.Name, value);
            return true;
        }
    }
}
