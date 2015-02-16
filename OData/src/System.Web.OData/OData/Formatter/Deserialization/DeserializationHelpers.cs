// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
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

            bool isDynamicProperty = false;
            string propertyName = property.Name;
            if (edmProperty != null)
            {
                propertyName = EdmLibHelpers.GetClrPropertyName(edmProperty, readContext.Model);
            }
            else
            {
                IEdmStructuredType structuredType = resourceType.StructuredDefinition();
                isDynamicProperty = structuredType != null && structuredType.IsOpen;
            }

            // dynamic properties have null values
            IEdmTypeReference propertyType = edmProperty != null ? edmProperty.Type : null;

            EdmTypeKind propertyKind;
            object value = ConvertValue(property.Value, ref propertyType, deserializerProvider, readContext,
                out propertyKind);

            if (isDynamicProperty)
            {
                SetDynamicProperty(resource, resourceType, propertyKind, propertyName, value, propertyType,
                    readContext);
            }
            else
            {
                SetDeclaredProperty(resource, propertyKind, propertyName, value, edmProperty, readContext);
            }
        }

        internal static void SetDynamicProperty(object resource, IEdmStructuredTypeReference resourceType,
            EdmTypeKind propertyKind, string propertyName, object propertyValue, IEdmTypeReference propertyType,
            ODataDeserializerContext readContext)
        {
            if (propertyKind == EdmTypeKind.Collection)
            {
                SetDynamicCollectionProperty(resource, propertyName, propertyValue, propertyType.AsCollection(),
                    resourceType.StructuredDefinition(), readContext);
            }
            else
            {
                if (propertyKind == EdmTypeKind.Enum)
                {
                    propertyValue = ConvertDynamicEnumValue(propertyValue, readContext);
                }

                SetDynamicProperty(resource, propertyName, propertyValue, resourceType.StructuredDefinition(),
                    readContext);
            }
        }

        internal static void SetDeclaredProperty(object resource, EdmTypeKind propertyKind, string propertyName,
            object propertyValue, IEdmProperty edmProperty, ODataDeserializerContext readContext)
        {
            if (propertyKind == EdmTypeKind.Collection)
            {
                SetCollectionProperty(resource, edmProperty, propertyValue, propertyName);
            }
            else
            {
                if (!readContext.IsUntyped)
                {
                    if (propertyKind == EdmTypeKind.Primitive)
                    {
                        propertyValue = EdmPrimitiveHelpers.ConvertPrimitiveValue(propertyValue,
                            GetPropertyType(resource, propertyName));
                    }
                    else if (propertyKind == EdmTypeKind.Enum)
                    {
                        propertyValue = EnumDeserializationHelpers.ConvertEnumValue(propertyValue,
                            GetPropertyType(resource, propertyName));
                    }
                }

                SetProperty(resource, propertyName, propertyValue);
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

        internal static void SetDynamicCollectionProperty(object resource, string propertyName, object value,
            IEdmCollectionTypeReference edmPropertyType, IEdmStructuredType structuredType,
            ODataDeserializerContext readContext)
        {
            Contract.Assert(value != null);
            Contract.Assert(readContext != null);
            Contract.Assert(readContext.Model != null);

            IEnumerable collection = value as IEnumerable;
            Contract.Assert(collection != null);

            Type resourceType = resource.GetType();
            Type elementType = EdmLibHelpers.GetClrType(edmPropertyType.ElementType(), readContext.Model);
            Type propertyType = typeof(ICollection<>).MakeGenericType(elementType);
            IEnumerable newCollection;
            if (CollectionDeserializationHelpers.TryCreateInstance(propertyType, edmPropertyType, elementType,
                out newCollection))
            {
                collection.AddToCollection(newCollection, elementType, resourceType, propertyName, propertyType);
                SetDynamicProperty(resource, propertyName, newCollection, structuredType, readContext);
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

        internal static void SetDynamicProperty(object resource, string propertyName, object value,
            IEdmStructuredType structuredType, ODataDeserializerContext readContext)
        {
            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                delta.TrySetPropertyValue(propertyName, value);
            }
            else
            {
                PropertyInfo propertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(structuredType,
                    readContext.Model);
                if (propertyInfo == null)
                {
                    return;
                }

                IDictionary<string, object> dynamicPropertyDictionary;
                object dynamicDictionaryObject = propertyInfo.GetValue(resource);
                if (dynamicDictionaryObject == null)
                {
                    if (!propertyInfo.CanWrite)
                    {
                        throw Error.InvalidOperation(SRResources.CannotSetDynamicPropertyDictionary, propertyName,
                            resource.GetType().FullName);
                    }

                    dynamicPropertyDictionary = new Dictionary<string, object>();
                    propertyInfo.SetValue(resource, dynamicPropertyDictionary);
                }
                else
                {
                    dynamicPropertyDictionary = (IDictionary<string, object>)dynamicDictionaryObject;
                }

                if (dynamicPropertyDictionary.ContainsKey(propertyName))
                {
                    throw Error.InvalidOperation(SRResources.DuplicateDynamicPropertyNameFound,
                        propertyName, structuredType.FullTypeName());
                }

                dynamicPropertyDictionary.Add(propertyName, value);
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
                return ConvertCollectionValue(collection, ref propertyType, deserializerProvider, readContext);
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

        private static object ConvertCollectionValue(ODataCollectionValue collection,
            ref IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider,
            ODataDeserializerContext readContext)
        {
            IEdmCollectionTypeReference collectionType;
            if (propertyType == null)
            {
                // dynamic collection property
                Contract.Assert(!String.IsNullOrEmpty(collection.TypeName),
                    "ODataLib should have verified that dynamic collection value has a type name " +
                    "since we provided metadata.");

                string elementTypeName = GetCollectionElementTypeName(collection.TypeName, isNested: false);
                IEdmModel model = readContext.Model;
                IEdmSchemaType elementType = model.FindType(elementTypeName);
                Contract.Assert(elementType != null);
                collectionType =
                    new EdmCollectionTypeReference(
                        new EdmCollectionType(elementType.ToEdmTypeReference(isNullable: false)));
                propertyType = collectionType;
            }
            else
            {
                collectionType = propertyType as IEdmCollectionTypeReference;
                Contract.Assert(collectionType != null, "The type for collection must be a IEdmCollectionType.");
            }

            ODataEdmTypeDeserializer deserializer = deserializerProvider.GetEdmTypeDeserializer(collectionType);
            return deserializer.ReadInline(collection, collectionType, readContext);
        }

        private static object ConvertDynamicEnumValue(object value, ODataDeserializerContext readContext)
        {
            Contract.Assert(value != null);
            Contract.Assert(readContext != null);
            Contract.Assert(readContext.Model != null);

            ODataEnumValue enumValue = value as ODataEnumValue;
            Contract.Assert(enumValue != null);

            IEdmType edmType = readContext.Model.FindType(enumValue.TypeName);
            Contract.Assert(edmType != null);

            if (!readContext.IsUntyped)
            {
                Type enumType = EdmLibHelpers.GetClrType(edmType, readContext.Model);
                return Enum.Parse(enumType, enumValue.Value);
            }
            else
            {
                // TODO: https://aspnetwebstack.codeplex.com/workitem/1956:
                // Enum (de)serialization on un-type doesn't work
                // Currently, just return the "string" value of the ODataEnumValue for untype enum type.
                return enumValue.Value;
            }
        }

        // The same logic from ODL to get the element type name in a collection.
        private static string GetCollectionElementTypeName(string typeName, bool isNested)
        {
            const string CollectionTypeQualifier = "Collection";
            int collectionTypeQualifierLength = CollectionTypeQualifier.Length;

            // A collection type name must not be null, it has to start with "Collection(" and end with ")"
            // and must not be "Collection()"
            if (typeName != null &&
                typeName.StartsWith(CollectionTypeQualifier + "(", StringComparison.Ordinal) &&
                typeName[typeName.Length - 1] == ')' &&
                typeName.Length != collectionTypeQualifierLength + 2)
            {
                if (isNested)
                {
                    throw new ODataException(Error.Format(SRResources.NestedCollectionsNotSupported, typeName));
                }

                string innerTypeName = typeName.Substring(collectionTypeQualifierLength + 1,
                    typeName.Length - (collectionTypeQualifierLength + 2));

                // Check if it is not a nested collection and throw if it is
                GetCollectionElementTypeName(innerTypeName, true);

                return innerTypeName;
            }

            return null;
        }
    }
}
