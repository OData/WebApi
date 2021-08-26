//-----------------------------------------------------------------------------
// <copyright file="DeserializationHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    internal static class DeserializationHelpers
    {
        internal static void ApplyProperty(ODataProperty property, IEdmStructuredTypeReference resourceType, object resource,
            ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmProperty edmProperty;

            // Extract edmProperty using a Case-Sensitive match
            edmProperty = resourceType.FindProperty(property.Name);

            if (readContext != null && !readContext.DisableCaseInsensitiveRequestPropertyBinding && edmProperty == null)
            {
                // if edmProperty is null, we try a Case-Insensitive match.
                bool propertyMatched = false;

                IEnumerable<IEdmStructuralProperty> structuralProperties = resourceType.StructuralProperties();
                foreach(IEdmStructuralProperty structuralProperty in structuralProperties)
                {
                    if(structuralProperty.Name.Equals(property.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        // We throw an exception when we have more than 1 case-insensitive matches and no case sensitive matches.
                        if (propertyMatched)
                        {
                            throw new ODataException(Error.Format(SRResources.CannotDeserializeUnknownProperty, property.Name, resourceType.Definition));
                        }
                        propertyMatched = true;
                        edmProperty = resourceType.FindProperty(structuralProperty.Name);
                    }
                }
            }

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

            if (!isDynamicProperty && edmProperty == null)
            {
                throw new ODataException(
                    Error.Format(SRResources.CannotDeserializeUnknownProperty, property.Name, resourceType.Definition));
            }

            // dynamic properties have null values
            IEdmTypeReference propertyType = edmProperty != null ? edmProperty.Type : null;

            EdmTypeKind propertyKind;
            object value = ConvertValue(property.Value, ref propertyType, deserializerProvider, readContext,
                out propertyKind);

            if (isDynamicProperty)
            {
                SetDynamicProperty(resource, resourceType, propertyKind, propertyName, value, propertyType,
                    readContext.Model);
            }
            else
            {
                SetDeclaredProperty(resource, propertyKind, propertyName, value, edmProperty, readContext);
            }
        }

        internal static void ApplyInstanceAnnotations(object resource, IEdmStructuredTypeReference structuredType, ODataResourceBase oDataResource,
            ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            //Apply instance annotations for both entityobject/changedobject/delta and normal resources

            IODataInstanceAnnotationContainer instanceAnnotationContainer = null;
            IODataInstanceAnnotationContainer transientAnnotationContainer = null;

            EdmEntityObject edmObject = resource as EdmEntityObject;

            if (edmObject != null)
            {
                instanceAnnotationContainer = edmObject.PersistentInstanceAnnotationsContainer;
                transientAnnotationContainer = edmObject.TransientInstanceAnnotationContainer;
            }
            else
            {
                PropertyInfo propertyInfo = EdmLibHelpers.GetInstanceAnnotationsContainer(structuredType.StructuredDefinition(), readContext.Model);
                if (propertyInfo != null)
                {
                    instanceAnnotationContainer = GetAnnotationContainer(propertyInfo, resource);
                }

                IDeltaSetItem deltaItem = resource as IDeltaSetItem;

                if (deltaItem != null)
                {
                    transientAnnotationContainer = deltaItem.TransientInstanceAnnotationContainer;
                }                
            }

            if (instanceAnnotationContainer == null && transientAnnotationContainer == null)
            {
                return;
            }

            SetInstanceAnnotations(oDataResource, instanceAnnotationContainer, transientAnnotationContainer, deserializerProvider, readContext);            
        }

        internal static void ApplyODataIdContainer(object resource, IEdmStructuredTypeReference structuredType, ODataResourceBase oDataResource,
            ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            //Apply instance annotations for both entityobject/changedobject/delta and normal resources

            IODataInstanceAnnotationContainer instanceAnnotationContainer = null;
            IODataInstanceAnnotationContainer transientAnnotationContainer = null;

            EdmEntityObject edmObject = resource as EdmEntityObject;

            if (edmObject != null)
            {
                instanceAnnotationContainer = edmObject.PersistentInstanceAnnotationsContainer;
                transientAnnotationContainer = edmObject.TransientInstanceAnnotationContainer;
            }
            else
            {
                PropertyInfo propertyInfo = EdmLibHelpers.GetInstanceAnnotationsContainer(structuredType.StructuredDefinition(), readContext.Model);
                if (propertyInfo != null)
                {
                    instanceAnnotationContainer = GetAnnotationContainer(propertyInfo, resource);
                }

                IDeltaSetItem deltaItem = resource as IDeltaSetItem;

                if (deltaItem != null)
                {
                    transientAnnotationContainer = deltaItem.TransientInstanceAnnotationContainer;
                }
            }

            if (instanceAnnotationContainer == null && transientAnnotationContainer == null)
            {
                return;
            }

            SetInstanceAnnotations(oDataResource, instanceAnnotationContainer, transientAnnotationContainer, deserializerProvider, readContext);
        }

        internal static void SetDynamicProperty(object resource, IEdmStructuredTypeReference resourceType,
            EdmTypeKind propertyKind, string propertyName, object propertyValue, IEdmTypeReference propertyType,
            IEdmModel model)
        {
            if (propertyKind == EdmTypeKind.Collection && propertyValue.GetType() != typeof(EdmComplexObjectCollection)
                && propertyValue.GetType() != typeof(EdmEnumObjectCollection))
            {
                SetDynamicCollectionProperty(resource, propertyName, propertyValue, propertyType.AsCollection(),
                    resourceType.StructuredDefinition(), model);
            }
            else
            {
                SetDynamicProperty(resource, propertyName, propertyValue, resourceType.StructuredDefinition(),
                    model);
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
                }

                SetProperty(resource, propertyName, propertyValue);
            }
        }

        internal static void SetCollectionProperty(object resource, IEdmProperty edmProperty, object value, string propertyName, bool isDelta = false)
        {
            Contract.Assert(edmProperty != null);

            SetCollectionProperty(resource, propertyName, edmProperty.Type.AsCollection(), value, clearCollection: false, isDelta);
        }

        internal static void SetCollectionProperty(object resource, string propertyName,
            IEdmCollectionTypeReference edmPropertyType, object value, bool clearCollection, bool isDelta = false)
        {
            if (value != null)
            {
                IEnumerable collection = value as IEnumerable;
                Contract.Assert(collection != null,
                    "SetCollectionProperty is always passed the result of ODataFeedDeserializer or ODataCollectionDeserializer");

                Type resourceType = resource.GetType();
                Type propertyType = GetPropertyType(resource, propertyName);

                Type elementType;
                if (!TypeHelper.IsCollection(propertyType, out elementType))
                {
                    string message = Error.Format(SRResources.PropertyIsNotCollection, propertyType.FullName, propertyName, resourceType.FullName);
                    throw new SerializationException(message);
                }

                IEnumerable newCollection;
                if (CanSetProperty(resource, propertyName) &&
                    CollectionDeserializationHelpers.TryCreateInstance(propertyType, edmPropertyType, elementType, out newCollection, isDelta))
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
            IEdmModel model)
        {
            Contract.Assert(value != null);
            Contract.Assert(model != null);

            IEnumerable collection = value as IEnumerable;
            Contract.Assert(collection != null);

            Type resourceType = resource.GetType();
            Type elementType = EdmLibHelpers.GetClrType(edmPropertyType.ElementType(), model);
            Type propertyType = typeof(ICollection<>).MakeGenericType(elementType);
            IEnumerable newCollection;
            if (CollectionDeserializationHelpers.TryCreateInstance(propertyType, edmPropertyType, elementType,
                out newCollection))
            {
                collection.AddToCollection(newCollection, elementType, resourceType, propertyName, propertyType);
                SetDynamicProperty(resource, propertyName, newCollection, structuredType, model);
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
            IEdmStructuredType structuredType, IEdmModel model)
        {
            IDelta delta = resource as IDelta;
            if (delta != null)
            {
                delta.TrySetPropertyValue(propertyName, value);
            }
            else
            {
                PropertyInfo propertyInfo = EdmLibHelpers.GetDynamicPropertyDictionary(structuredType,
                    model);
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

        internal static void SetInstanceAnnotations(ODataResourceBase oDataResource, IODataInstanceAnnotationContainer instanceAnnotationContainer,
            IODataInstanceAnnotationContainer transientAnnotationContainer, ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            if(oDataResource.InstanceAnnotations != null)
            {
                foreach (ODataInstanceAnnotation annotation in oDataResource.InstanceAnnotations)
                {
                    if (!TransientAnnotations.TransientAnnotationTerms.Contains(annotation.Name))
                    {
                        AddInstanceAnnotationToContainer(instanceAnnotationContainer, deserializerProvider, readContext, annotation, string.Empty);
                    }
                    else
                    {
                        AddInstanceAnnotationToContainer(transientAnnotationContainer, deserializerProvider, readContext, annotation, string.Empty);
                    }
                }
            }

            foreach(ODataProperty property in oDataResource.Properties)
            {
                if(property.InstanceAnnotations != null)
                {
                    foreach (ODataInstanceAnnotation annotation in property.InstanceAnnotations)
                    {
                        AddInstanceAnnotationToContainer(instanceAnnotationContainer, deserializerProvider, readContext, annotation, property.Name);
                    }
                }
            }
        }

        private static void AddInstanceAnnotationToContainer(IODataInstanceAnnotationContainer instanceAnnotationContainer, ODataDeserializerProvider deserializerProvider, 
            ODataDeserializerContext readContext, ODataInstanceAnnotation annotation, string propertyName)
        {
            IEdmTypeReference propertyType = null;

            object annotationValue = ConvertAnnotationValue(annotation.Value, ref propertyType, deserializerProvider, readContext);

            if (string.IsNullOrEmpty(propertyName))
            {
                instanceAnnotationContainer.AddResourceAnnotation(annotation.Name, annotationValue);
            }
            else
            {
                instanceAnnotationContainer.AddPropertyAnnotation(propertyName,annotation.Name, annotationValue);
            }
        }
                
        public static IODataInstanceAnnotationContainer GetAnnotationContainer(PropertyInfo propertyInfo, object resource)
        {            
            object value;                        
            IDelta delta = resource as IDelta;

            if (delta != null)
            {
                delta.TryGetPropertyValue(propertyInfo.Name, out value);
            }
            else
            {
                value = propertyInfo.GetValue(resource);
            }            

            IODataInstanceAnnotationContainer instanceAnnotationContainer = value as IODataInstanceAnnotationContainer;

            if (instanceAnnotationContainer == null)
            {
                try
                {
                    if (propertyInfo.PropertyType == typeof(ODataInstanceAnnotationContainer) || propertyInfo.PropertyType == typeof(IODataInstanceAnnotationContainer))
                    {
                        instanceAnnotationContainer = new ODataInstanceAnnotationContainer();
                    }
                    else
                    {
                        instanceAnnotationContainer = Activator.CreateInstance(propertyInfo.PropertyType) as IODataInstanceAnnotationContainer;
                    }
                    
                    if (delta != null)
                    {
                        delta.TrySetPropertyValue(propertyInfo.Name, instanceAnnotationContainer);
                    }
                    else
                    {
                        propertyInfo.SetValue(resource, instanceAnnotationContainer);
                    }
                }
                catch(Exception ex)
                {
                    throw new ODataException(Error.Format(SRResources.CannotCreateInstanceForProperty, propertyInfo.Name), ex);
                }
            }

            return instanceAnnotationContainer;
        }

        internal static object ConvertValue(object oDataValue, ref IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider,
            ODataDeserializerContext readContext, out EdmTypeKind typeKind)
        {
            if (oDataValue == null)
            {
                typeKind = EdmTypeKind.None;
                return null;
            }

            ODataEnumValue enumValue = oDataValue as ODataEnumValue;
            if (enumValue != null)
            {
                typeKind = EdmTypeKind.Enum;
                return ConvertEnumValue(enumValue, ref propertyType, deserializerProvider, readContext);
            }

            ODataCollectionValue collection = oDataValue as ODataCollectionValue;
            if (collection != null)
            {
                typeKind = EdmTypeKind.Collection;
                return ConvertCollectionValue(collection, ref propertyType, deserializerProvider, readContext);
            }

            ODataUntypedValue untypedValue = oDataValue as ODataUntypedValue;
            if (untypedValue != null)
            {
                Contract.Assert(!String.IsNullOrEmpty(untypedValue.RawValue));

                if (untypedValue.RawValue.StartsWith("[", StringComparison.Ordinal) ||
                    untypedValue.RawValue.StartsWith("{", StringComparison.Ordinal))
                {
                    throw new ODataException(Error.Format(SRResources.InvalidODataUntypedValue, untypedValue.RawValue));
                }

                oDataValue = ConvertPrimitiveValue(untypedValue.RawValue);
            }

            typeKind = EdmTypeKind.Primitive;
            return oDataValue;
        }

        internal static object ConvertAnnotationValue(object oDataValue, ref IEdmTypeReference propertyType, ODataDeserializerProvider deserializerProvider,
    ODataDeserializerContext readContext)
        {
            if (oDataValue == null)
            {
                return null;
            }

            ODataEnumValue enumValue = oDataValue as ODataEnumValue;
            if (enumValue != null)
            {
                return ConvertEnumValue(enumValue, ref propertyType, deserializerProvider, readContext);
            }

            ODataCollectionValue collection = oDataValue as ODataCollectionValue;
            if (collection != null)
            {
                return ConvertCollectionValue(collection, ref propertyType, deserializerProvider, readContext);
            }

            ODataUntypedValue untypedValue = oDataValue as ODataUntypedValue;
            if (untypedValue != null)
            {
                Contract.Assert(!String.IsNullOrEmpty(untypedValue.RawValue));

                if (untypedValue.RawValue.StartsWith("[", StringComparison.Ordinal) ||
                    untypedValue.RawValue.StartsWith("{", StringComparison.Ordinal))
                {
                    throw new ODataException(Error.Format(SRResources.InvalidODataUntypedValue, untypedValue.RawValue));
                }

                oDataValue = ConvertPrimitiveValue(untypedValue.RawValue);
            }

            ODataResourceValue annotationVal = oDataValue as ODataResourceValue;
            if (annotationVal != null)
            {
                var annotationType = readContext.Model.FindDeclaredType(annotationVal.TypeName).ToEdmTypeReference(true) as IEdmStructuredTypeReference;

                ODataResourceDeserializer deserializer = new ODataResourceDeserializer(deserializerProvider);

                object resource = deserializer.CreateResourceInstance(annotationType, readContext);

                if (resource != null)
                {
                    foreach (var prop in annotationVal.Properties)
                    {
                        deserializer.ApplyStructuralProperty(resource, prop, annotationType, readContext);
                    }
                }
               
                return resource;
            }

            ODataPrimitiveValue primitiveValue = oDataValue as ODataPrimitiveValue;
            if (primitiveValue != null)
            {
                return EdmPrimitiveHelpers.ConvertPrimitiveValue(primitiveValue.Value, primitiveValue.Value.GetType());
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

        private static object ConvertPrimitiveValue(string value)
        {
            double doubleValue;
            int intValue;
            decimal decimalValue;

            if (String.CompareOrdinal(value, "null") == 0)
            {
                return null;
            }

            if (Int32.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out intValue))
            {
                return intValue;
            }

            // todo: if it is Ieee754Compatible, parse decimal after double
            if (Decimal.TryParse(value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out decimalValue))
            {
                return decimalValue;
            }

            if (Double.TryParse(value, NumberStyles.Float, NumberFormatInfo.InvariantInfo, out doubleValue))
            {
                return doubleValue;
            }

            if (!value.StartsWith("\"", StringComparison.Ordinal) || !value.EndsWith("\"", StringComparison.Ordinal))
            {
                throw new ODataException(Error.Format(SRResources.InvalidODataUntypedValue, value));
            }

            // `JsonValueUtils`'s `GetEscapedJsonString` method in ODL converts string
            // to Json-formatted string with specific special characters escaped.
            return UnescapeString(value.Substring(1, value.Length - 2));
        }

        private static object ConvertEnumValue(ODataEnumValue enumValue, ref IEdmTypeReference propertyType,
            ODataDeserializerProvider deserializerProvider, ODataDeserializerContext readContext)
        {
            IEdmEnumTypeReference edmEnumType;
            if (propertyType == null)
            {
                // dynamic enum property
                Contract.Assert(!String.IsNullOrEmpty(enumValue.TypeName),
                    "ODataLib should have verified that dynamic enum value has a type name since we provided metadata.");
                IEdmModel model = readContext.Model;
                IEdmType edmType = model.FindType(enumValue.TypeName);
                Contract.Assert(edmType.TypeKind == EdmTypeKind.Enum, "ODataLib should have verified that enum value has a enum resource type.");
                edmEnumType = new EdmEnumTypeReference(edmType as IEdmEnumType, isNullable: true);
                propertyType = edmEnumType;
            }
            else
            {
                edmEnumType = propertyType.AsEnum();
            }

            ODataEdmTypeDeserializer deserializer = deserializerProvider.GetEdmTypeDeserializer(edmEnumType);
            return deserializer.ReadInline(enumValue, propertyType, readContext);
        }

        /// <summary>
        /// Converts any escaped characters in the input string.
        /// </summary>
        /// <param name="inputString">The input string containing the text to convert.</param>
        /// <returns>String with any escaped characters converted to their unescaped form.</returns>
        private static string UnescapeString(string inputString)
        {
            if (string.IsNullOrWhiteSpace(inputString))
            {
                return inputString;
            }

            // Make use `Regex`'s `Unescape` method to pick out any escaped special characters and unescape them
            return Regex.Unescape(inputString);
        }

        // The same logic from ODL to get the element type name in a collection.
        internal static string GetCollectionElementTypeName(string typeName, bool isNested)
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
