// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Expose functionality to convert an function parameter value into a CLR object.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
        Justification = "Relies on many ODataLib classes.")]
    public static class ODataModelBinderConverter
    {
        private static readonly MethodInfo EnumTryParseMethod = typeof(Enum).GetMethods()
            .Single(m => m.Name == "TryParse" && m.GetParameters().Length == 2);

        private static readonly MethodInfo CastMethodInfo = typeof(Enumerable).GetMethod("Cast");

        /// <summary>
        /// Convert an OData value into a CLR object.
        /// </summary>
        /// <param name="graph">The given object.</param>
        /// <param name="edmTypeReference">The EDM type of the given object.</param>
        /// <param name="clrType">The CLR type of the given object.</param>
        /// <param name="parameterName">The parameter name of the given object.</param>
        /// <param name="readContext">The <see cref="ODataDeserializerContext"/> use to convert.</param>
        /// <param name="requestContainer">The dependency injection container for the request.</param>
        /// <returns>The converted object.</returns>
        public static object Convert(object graph, IEdmTypeReference edmTypeReference,
            Type clrType, string parameterName, ODataDeserializerContext readContext,
            IServiceProvider requestContainer)
        {
            if (graph == null || graph is ODataNullValue)
            {
                return null;
            }

            // collection of primitive, enum
            ODataCollectionValue collectionValue = graph as ODataCollectionValue;
            if (collectionValue != null)
            {
                return ConvertCollection(collectionValue, edmTypeReference, clrType, parameterName, readContext,
                    requestContainer);
            }

            // enum value
            ODataEnumValue enumValue = graph as ODataEnumValue;
            if (enumValue != null)
            {
                IEdmEnumTypeReference edmEnumType = edmTypeReference.AsEnum();
                Contract.Assert(edmEnumType != null);

                ODataDeserializerProvider deserializerProvider =
                    requestContainer.GetRequiredService<ODataDeserializerProvider>();

                ODataEnumDeserializer deserializer =
                    (ODataEnumDeserializer)deserializerProvider.GetEdmTypeDeserializer(edmEnumType);

                return deserializer.ReadInline(enumValue, edmEnumType, readContext);
            }

            // primitive value
            if (edmTypeReference.IsPrimitive())
            {
                ConstantNode node = graph as ConstantNode;
                return EdmPrimitiveHelpers.ConvertPrimitiveValue(node != null ? node.Value : graph, clrType);
            }

            // Resource, ResourceSet, Entity Reference or collection of entity reference
            return ConvertResourceOrResourceSet(graph, edmTypeReference, readContext);
        }

        internal static object ConvertTo(string valueString, Type type)
        {
            if (valueString == null)
            {
                return null;
            }

            if (TypeHelper.IsNullable(type) && String.Equals(valueString, "null", StringComparison.Ordinal))
            {
                return null;
            }

            // TODO 1668: ODL beta1's ODataUriUtils.ConvertFromUriLiteral does not support converting uri literal
            // to ODataEnumValue, but beta1's ODataUriUtils.ConvertToUriLiteral supports converting ODataEnumValue
            // to uri literal.
            if (TypeHelper.IsEnum(type))
            {
                string[] values = valueString.Split(new[] { '\'' }, StringSplitOptions.None);
                if (values.Length == 3 && String.IsNullOrEmpty(values[2]))
                {
                    // Remove the type name if the enum value is a fully qualified literal.
                    valueString = values[1];
                }

                Type enumType = TypeHelper.GetUnderlyingTypeOrSelf(type);
                object[] parameters = new[] { valueString, Enum.ToObject(enumType, 0) };
                bool isSuccessful = (bool)EnumTryParseMethod.MakeGenericMethod(enumType).Invoke(null, parameters);

                if (!isSuccessful)
                {
                    throw Error.InvalidOperation(SRResources.ModelBinderUtil_ValueCannotBeEnum, valueString, type.Name);
                }

                return parameters[1];
            }

            // The logic of "public static object ConvertFromUriLiteral(string value, ODataVersion version);" treats
            // the date value string (for example: 2015-01-02) as DateTimeOffset literal, and return a DateTimeOffset
            // object. However, the logic of
            // "object ConvertFromUriLiteral(string value, ODataVersion version, IEdmModel model, IEdmTypeReference typeReference);"
            // can return the correct Date object.
            if (type == typeof(Date) || type == typeof(Date?))
            {
                EdmCoreModel model = EdmCoreModel.Instance;
                IEdmPrimitiveTypeReference dateTypeReference = EdmLibHelpers.GetEdmPrimitiveTypeReferenceOrNull(type);
                return ODataUriUtils.ConvertFromUriLiteral(valueString, ODataVersion.V4, model, dateTypeReference);
            }

            object value;
            try
            {
                value = ODataUriUtils.ConvertFromUriLiteral(valueString, ODataVersion.V4);
            }
            catch
            {
                if (type == typeof(string))
                {
                    return valueString;
                }

                throw;
            }

            bool isNonStandardEdmPrimitive;
            EdmLibHelpers.IsNonstandardEdmPrimitive(type, out isNonStandardEdmPrimitive);

            if (isNonStandardEdmPrimitive)
            {
                return EdmPrimitiveHelpers.ConvertPrimitiveValue(value, type);
            }
            else
            {
                type = Nullable.GetUnderlyingType(type) ?? type;
                return System.Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
            }
        }

        private static object ConvertCollection(ODataCollectionValue collectionValue,
            IEdmTypeReference edmTypeReference, Type clrType, string parameterName,
            ODataDeserializerContext readContext, IServiceProvider requestContainer)
        {
            Contract.Assert(collectionValue != null);

            IEdmCollectionTypeReference collectionType = edmTypeReference as IEdmCollectionTypeReference;
            Contract.Assert(collectionType != null);

            ODataDeserializerProvider deserializerProvider =
                requestContainer.GetRequiredService<ODataDeserializerProvider>();
            ODataCollectionDeserializer deserializer =
                (ODataCollectionDeserializer)deserializerProvider.GetEdmTypeDeserializer(collectionType);

            object value = deserializer.ReadInline(collectionValue, collectionType, readContext);
            if (value == null)
            {
                return null;
            }

            IEnumerable collection = value as IEnumerable;
            Contract.Assert(collection != null);

            Type elementType;
            if (!TypeHelper.IsCollection(clrType, out elementType))
            {
                // EdmEntityCollectionObject and EdmComplexCollectionObject are collection types.
                throw new ODataException(String.Format(CultureInfo.InvariantCulture,
                    SRResources.ParameterTypeIsNotCollection, parameterName, clrType));
            }

            IEnumerable newCollection;
            if (CollectionDeserializationHelpers.TryCreateInstance(clrType, collectionType, elementType,
                out newCollection))
            {
                collection.AddToCollection(newCollection, elementType, parameterName, clrType);
                if (clrType.IsArray)
                {
                    newCollection = CollectionDeserializationHelpers.ToArray(newCollection, elementType);
                }

                return newCollection;
            }

            return null;
        }

        private static object ConvertResourceOrResourceSet(object oDataValue, IEdmTypeReference edmTypeReference,
            ODataDeserializerContext readContext)
        {
            string valueString = oDataValue as string;
            Contract.Assert(valueString != null);

            if (edmTypeReference.IsNullable && String.Equals(valueString, "null", StringComparison.Ordinal))
            {
                return null;
            }

            IWebApiRequestMessage request = readContext.InternalRequest;
            ODataMessageReaderSettings oDataReaderSettings = request.ReaderSettings;

            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(valueString)))
            {
                stream.Seek(0, SeekOrigin.Begin);

                IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(stream, null,
                    request.ODataContentIdMapping);
                using (
                    ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage,
                        oDataReaderSettings, readContext.Model))
                {
                    if (edmTypeReference.IsCollection())
                    {
                        return ConvertResourceSet(oDataMessageReader, edmTypeReference, readContext);
                    }
                    else
                    {
                        return ConvertResource(oDataMessageReader, edmTypeReference, readContext);
                    }
                }
            }
        }

        private static object ConvertResourceSet(ODataMessageReader oDataMessageReader,
            IEdmTypeReference edmTypeReference, ODataDeserializerContext readContext)
        {
            IEdmCollectionTypeReference collectionType = edmTypeReference.AsCollection();

            EdmEntitySet tempEntitySet = null;
            if (collectionType.ElementType().IsEntity())
            {
                tempEntitySet = new EdmEntitySet(readContext.Model.EntityContainer, "temp",
                    collectionType.ElementType().AsEntity().EntityDefinition());
            }

            // TODO: Sam xu, can we use the parameter-less overload
            ODataReader odataReader = oDataMessageReader.CreateODataUriParameterResourceSetReader(tempEntitySet,
                collectionType.ElementType().AsStructured().StructuredDefinition());
            ODataResourceSetWrapper resourceSet =
                odataReader.ReadResourceOrResourceSet() as ODataResourceSetWrapper;

            ODataDeserializerProvider deserializerProvider = readContext.InternalRequest.DeserializerProvider;

            ODataResourceSetDeserializer resourceSetDeserializer =
                (ODataResourceSetDeserializer)deserializerProvider.GetEdmTypeDeserializer(collectionType);

            object result = resourceSetDeserializer.ReadInline(resourceSet, collectionType, readContext);
            IEnumerable enumerable = result as IEnumerable;
            if (enumerable != null)
            {
                IEnumerable newEnumerable = enumerable;
                if (collectionType.ElementType().IsEntity())
                {
                    newEnumerable = CovertResourceSetIds(enumerable, resourceSet, collectionType, readContext);
                }

                if (readContext.IsUntyped)
                {
                    return newEnumerable.ConvertToEdmObject(collectionType);
                }
                else
                {
                    IEdmTypeReference elementTypeReference = collectionType.ElementType();

                    Type elementClrType = EdmLibHelpers.GetClrType(elementTypeReference,
                        readContext.Model);
                    IEnumerable castedResult =
                        CastMethodInfo.MakeGenericMethod(elementClrType)
                            .Invoke(null, new object[] { newEnumerable }) as IEnumerable;
                    return castedResult;
                }
            }

            return null;
        }

        private static object ConvertResource(ODataMessageReader oDataMessageReader, IEdmTypeReference edmTypeReference,
            ODataDeserializerContext readContext)
        {
            EdmEntitySet tempEntitySet = null;
            if (edmTypeReference.IsEntity())
            {
                IEdmEntityTypeReference entityType = edmTypeReference.AsEntity();
                tempEntitySet = new EdmEntitySet(readContext.Model.EntityContainer, "temp",
                    entityType.EntityDefinition());
            }

            // TODO: Sam xu, can we use the parameter-less overload
            ODataReader resourceReader = oDataMessageReader.CreateODataUriParameterResourceReader(tempEntitySet,
                edmTypeReference.ToStructuredType());

            object item = resourceReader.ReadResourceOrResourceSet();

            ODataResourceWrapper topLevelResource = item as ODataResourceWrapper;
            Contract.Assert(topLevelResource != null);

            ODataDeserializerProvider deserializerProvider = readContext.InternalRequest.DeserializerProvider;

            ODataResourceDeserializer entityDeserializer =
                (ODataResourceDeserializer)deserializerProvider.GetEdmTypeDeserializer(edmTypeReference);
            object value = entityDeserializer.ReadInline(topLevelResource, edmTypeReference, readContext);

            if (edmTypeReference.IsEntity())
            {
                IEdmEntityTypeReference entityType = edmTypeReference.AsEntity();
                return CovertResourceId(value, topLevelResource.ResourceBase as ODataResource, entityType, readContext);
            }

            return value;
        }

        private static IEnumerable CovertResourceSetIds(IEnumerable sources, ODataResourceSetWrapper resourceSet,
            IEdmCollectionTypeReference collectionType, ODataDeserializerContext readContext)
        {
            IEdmEntityTypeReference entityTypeReference = collectionType.ElementType().AsEntity();
            int i = 0;
            foreach (object item in sources)
            {
                object newItem = CovertResourceId(item, resourceSet.Resources[i].ResourceBase as ODataResource, entityTypeReference,
                    readContext);
                i++;
                yield return newItem;
            }
        }

        private static object CovertResourceId(object source, ODataResource resource,
            IEdmEntityTypeReference entityTypeReference, ODataDeserializerContext readContext)
        {
            Contract.Assert(resource != null);
            Contract.Assert(source != null);

            if (resource.Id == null || resource.Properties.Any())
            {
                return source;
            }

            IWebApiRequestMessage request = readContext.InternalRequest;
            IWebApiUrlHelper urlHelper = readContext.InternalUrlHelper;

            DefaultODataPathHandler pathHandler = new DefaultODataPathHandler();
            string serviceRoot = urlHelper.CreateODataLink(
                request.Context.RouteName,
                request.PathHandler,
                new List<ODataPathSegment>());

            IEnumerable<KeyValuePair<string, object>> keyValues = GetKeys(pathHandler, serviceRoot, resource.Id,
                request.RequestContainer);

            IList<IEdmStructuralProperty> keys = entityTypeReference.Key().ToList();

            if (keys.Count == 1 && keyValues.Count() == 1)
            {
                // TODO: make sure the enum key works
                object propertyValue = keyValues.First().Value;
                DeserializationHelpers.SetDeclaredProperty(source, EdmTypeKind.Primitive, keys[0].Name, propertyValue,
                    keys[0], readContext);
                return source;
            }

            IDictionary<string, object> keyValuesDic = keyValues.ToDictionary(e => e.Key, e => e.Value);
            foreach (IEdmStructuralProperty key in keys)
            {
                object value;
                if (keyValuesDic.TryGetValue(key.Name, out value))
                {
                    // TODO: make sure the enum key works
                    DeserializationHelpers.SetDeclaredProperty(source, EdmTypeKind.Primitive, key.Name, value, key,
                        readContext);
                }
            }

            return source;
        }

        private static IEnumerable<KeyValuePair<string, object>> GetKeys(DefaultODataPathHandler pathHandler,
            string serviceRoot, Uri uri, IServiceProvider requestContainer)
        {
            ODataPath odataPath = pathHandler.Parse(serviceRoot, uri.ToString(), requestContainer);
            KeySegment segment = odataPath.Segments.OfType<KeySegment>().Last();
            if (segment == null)
            {
                throw Error.InvalidOperation(SRResources.EntityReferenceMustHasKeySegment, uri);
            }

            return segment.Keys;
        }
    }
}
