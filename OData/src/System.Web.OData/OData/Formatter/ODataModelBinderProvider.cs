// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.ValueProviders;
using System.Web.OData.Batch;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter.Deserialization;
using System.Web.OData.Properties;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// Provides a <see cref="IModelBinder"/> for EDM primitive types.
    /// </summary>
    public class ODataModelBinderProvider : ModelBinderProvider
    {
        /// <inheritdoc />
        public override IModelBinder GetBinder(HttpConfiguration configuration, Type modelType)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (modelType == null)
            {
                throw Error.ArgumentNull("modelType");
            }

            return new ODataModelBinder();
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Relies on many ODataLib classes.")]
        internal class ODataModelBinder : IModelBinder
        {
            private static readonly MethodInfo EnumTryParseMethod = typeof(Enum).GetMethods()
                        .Single(m => m.Name == "TryParse" && m.GetParameters().Length == 2);

            private static readonly MethodInfo CastMethodInfo = typeof(Enumerable).GetMethod("Cast");

            private static readonly ODataDeserializerProvider DeserializerProvider = new DefaultODataDeserializerProvider();

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We don't want to fail in model binding.")]
            public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                if (bindingContext == null)
                {
                    throw Error.ArgumentNull("bindingContext");
                }

                if (bindingContext.ModelMetadata == null)
                {
                    throw Error.Argument("bindingContext", SRResources.ModelBinderUtil_ModelMetadataCannotBeNull);
                }

                string modelName = ODataParameterValue.ParameterValuePrefix + bindingContext.ModelName;
                ValueProviderResult value = bindingContext.ValueProvider.GetValue(modelName);
                if (value == null)
                {
                    value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
                    if (value == null)
                    {
                        return false;
                    }
                }

                bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

                try
                {
                    ODataParameterValue paramValue = value.RawValue as ODataParameterValue;
                    if (paramValue != null)
                    {
                        bindingContext.Model = ConvertTo(paramValue, actionContext, bindingContext);
                        return true;
                    }

                    // Support key value's [FromODataUri] binding
                    string valueString = value.RawValue as string;
                    if (valueString != null)
                    {
                        bindingContext.Model = ConvertTo(valueString, bindingContext.ModelType);
                        return true;
                    }

                    return false;
                }
                catch (ODataException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, ex.Message);
                    return false;
                }
                catch (ValidationException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, Error.Format(SRResources.ValueIsInvalid, value.RawValue, ex.Message));
                    return false;
                }
                catch (FormatException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, Error.Format(SRResources.ValueIsInvalid, value.RawValue, ex.Message));
                    return false;
                }
                catch (Exception e)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, e);
                    return false;
                }
            }

            internal static object ConvertTo(ODataParameterValue parameterValue, HttpActionContext actionContext, ModelBindingContext bindingContext)
            {
                Contract.Assert(parameterValue != null && parameterValue.EdmType != null);

                object oDataValue = parameterValue.Value;
                if (oDataValue == null || oDataValue is ODataNullValue)
                {
                    return null;
                }

                IEdmTypeReference edmTypeReference = parameterValue.EdmType;
                ODataDeserializerContext readContext = BuildDeserializerContext(actionContext, bindingContext, edmTypeReference);

                // complex value
                ODataComplexValue complexValue = oDataValue as ODataComplexValue;
                if (complexValue != null)
                {
                    IEdmComplexTypeReference edmComplexType = edmTypeReference.AsComplex();
                    Contract.Assert(edmComplexType != null);

                    ODataComplexTypeDeserializer deserializer =
                        (ODataComplexTypeDeserializer)DeserializerProvider.GetEdmTypeDeserializer(edmComplexType);

                    return deserializer.ReadInline(complexValue, edmComplexType, readContext);
                }

                // collection of primitive, enum, complex
                ODataCollectionValue collectionValue = oDataValue as ODataCollectionValue;
                if (collectionValue != null)
                {
                    return ConvertCollection(collectionValue, edmTypeReference, bindingContext, readContext);
                }

                // enum value
                ODataEnumValue enumValue = oDataValue as ODataEnumValue;
                if (enumValue != null)
                {
                    IEdmEnumTypeReference edmEnumType = edmTypeReference.AsEnum();
                    Contract.Assert(edmEnumType != null);

                    ODataEnumDeserializer deserializer =
                        (ODataEnumDeserializer)DeserializerProvider.GetEdmTypeDeserializer(edmEnumType);

                    return deserializer.ReadInline(enumValue, edmEnumType, readContext);
                }

                // primitive value
                if (edmTypeReference.IsPrimitive())
                {
                    return EdmPrimitiveHelpers.ConvertPrimitiveValue(oDataValue, bindingContext.ModelType);
                }

                // Entity, Feed, Entity Reference or collection of entity reference
                return ConvertFeedOrEntry(oDataValue, edmTypeReference, readContext);
            }

            internal static object ConvertCollection(ODataCollectionValue collectionValue, IEdmTypeReference edmTypeReference,
                ModelBindingContext bindingContext, ODataDeserializerContext readContext)
            {
                Contract.Assert(collectionValue != null);

                IEdmCollectionTypeReference collectionType = edmTypeReference as IEdmCollectionTypeReference;
                Contract.Assert(collectionType != null);

                ODataCollectionDeserializer deserializer =
                    (ODataCollectionDeserializer)DeserializerProvider.GetEdmTypeDeserializer(collectionType);

                object value = deserializer.ReadInline(collectionValue, collectionType, readContext);
                if (value == null)
                {
                    return null;
                }

                IEnumerable collection = value as IEnumerable;
                Contract.Assert(collection != null);

                Type clrType = bindingContext.ModelType;
                Type elementType;
                if (!clrType.IsCollection(out elementType))
                {
                    // EdmEntityCollectionObject and EdmComplexCollectionObject are collection types.
                    throw new ODataException(String.Format(CultureInfo.InvariantCulture,
                        SRResources.ParameterTypeIsNotCollection, bindingContext.ModelName, clrType));
                }

                IEnumerable newCollection;
                if (CollectionDeserializationHelpers.TryCreateInstance(clrType, collectionType, elementType, out newCollection))
                {
                    collection.AddToCollection(newCollection, elementType, bindingContext.ModelName, bindingContext.ModelType);
                    if (clrType.IsArray)
                    {
                        newCollection = CollectionDeserializationHelpers.ToArray(newCollection, elementType);
                    }

                    return newCollection;
                }

                return null;
            }

            internal static object ConvertFeedOrEntry(object oDataValue, IEdmTypeReference edmTypeReference, ODataDeserializerContext readContext)
            {
                string valueString = oDataValue as string;
                Contract.Assert(valueString != null);

                if (edmTypeReference.IsNullable && String.Equals(valueString, "null", StringComparison.Ordinal))
                {
                    return null;
                }

                HttpRequestMessage request = readContext.Request;
                ODataMessageReaderSettings oDataReaderSettings = new ODataMessageReaderSettings();

                using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(valueString)))
                {
                    stream.Seek(0, SeekOrigin.Begin);

                    IODataRequestMessage oDataRequestMessage = new ODataMessageWrapper(stream, null,
                        request.GetODataContentIdMapping());
                    using (
                        ODataMessageReader oDataMessageReader = new ODataMessageReader(oDataRequestMessage,
                            oDataReaderSettings, readContext.Model))
                    {
                        request.RegisterForDispose(oDataMessageReader);

                        if (edmTypeReference.IsCollection())
                        {
                            return ConvertFeed(oDataMessageReader, edmTypeReference, readContext);
                        }
                        else
                        {
                            return ConvertEntity(oDataMessageReader, edmTypeReference, readContext);
                        }
                    }
                }
            }

            internal static object ConvertFeed(ODataMessageReader oDataMessageReader, IEdmTypeReference edmTypeReference, ODataDeserializerContext readContext)
            {
                IEdmCollectionTypeReference collectionType = edmTypeReference.AsCollection();

                EdmEntitySet tempEntitySet = new EdmEntitySet(readContext.Model.EntityContainer, "temp",
                    collectionType.ElementType().AsEntity().EntityDefinition());

                ODataReader odataReader = oDataMessageReader.CreateODataFeedReader(tempEntitySet,
                    collectionType.ElementType().AsEntity().EntityDefinition());
                ODataFeedWithEntries feed =
                    ODataEntityDeserializer.ReadEntryOrFeed(odataReader) as ODataFeedWithEntries;

                ODataFeedDeserializer feedDeserializer =
                    (ODataFeedDeserializer)DeserializerProvider.GetEdmTypeDeserializer(collectionType);

                object result = feedDeserializer.ReadInline(feed, collectionType, readContext);
                IEnumerable enumerable = result as IEnumerable;
                if (enumerable != null)
                {
                    IEnumerable newEnumerable = CovertFeedIds(enumerable, feed, collectionType, readContext);
                    if (readContext.IsUntyped)
                    {
                        EdmEntityObjectCollection entityCollection =
                            new EdmEntityObjectCollection(collectionType);
                        foreach (EdmEntityObject entity in newEnumerable)
                        {
                            entityCollection.Add(entity);
                        }

                        return entityCollection;
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

            internal static object ConvertEntity(ODataMessageReader oDataMessageReader, IEdmTypeReference edmTypeReference,
                ODataDeserializerContext readContext)
            {
                IEdmEntityTypeReference entityType = edmTypeReference.AsEntity();

                EdmEntitySet tempEntitySet = new EdmEntitySet(readContext.Model.EntityContainer, "temp",
                    entityType.EntityDefinition());

                ODataReader entryReader = oDataMessageReader.CreateODataEntryReader(tempEntitySet,
                    entityType.EntityDefinition());

                object item = ODataEntityDeserializer.ReadEntryOrFeed(entryReader);

                ODataEntryWithNavigationLinks topLevelEntry = item as ODataEntryWithNavigationLinks;
                Contract.Assert(topLevelEntry != null);

                ODataEntityDeserializer entityDeserializer =
                    (ODataEntityDeserializer)DeserializerProvider.GetEdmTypeDeserializer(entityType);
                object entity = entityDeserializer.ReadInline(topLevelEntry, entityType, readContext);
                return CovertEntityId(entity, topLevelEntry.Entry, entityType, readContext);
            }

            internal static IEnumerable CovertFeedIds(IEnumerable sources, ODataFeedWithEntries feed,
                IEdmCollectionTypeReference collectionType, ODataDeserializerContext readContext)
            {
                IEdmEntityTypeReference entityTypeReference = collectionType.ElementType().AsEntity();
                int i = 0;
                foreach (object item in sources)
                {
                    object newItem = CovertEntityId(item, feed.Entries[i].Entry, entityTypeReference, readContext);
                    i++;
                    yield return newItem;
                }
            }

            internal static object CovertEntityId(object source, ODataEntry entry, IEdmEntityTypeReference entityTypeReference, ODataDeserializerContext readContext)
            {
                Contract.Assert(entry != null);
                Contract.Assert(source != null);

                if (entry.Id == null || entry.Properties.Any())
                {
                    return source;
                }

                HttpRequestMessage request = readContext.Request;
                IEdmModel edmModel = readContext.Model;

                DefaultODataPathHandler pathHandler = new DefaultODataPathHandler();
                string serviceRoot = GetServiceRoot(request);
                IEnumerable<KeyValuePair<string, object>> keyValues = GetKeys(pathHandler, edmModel, serviceRoot, entry.Id);

                IList<IEdmStructuralProperty> keys = entityTypeReference.Key().ToList();

                if (keys.Count == 1 && keyValues.Count() == 1)
                {
                    // TODO: make sure the enum key works
                    object propertyValue = keyValues.First().Value;
                    DeserializationHelpers.SetDeclaredProperty(source, EdmTypeKind.Primitive, keys[0].Name, propertyValue, keys[0], readContext);
                    return source;
                }

                IDictionary<string, object> keyValuesDic = keyValues.ToDictionary(e => e.Key, e => e.Value);
                foreach (IEdmStructuralProperty key in keys)
                {
                    object value;
                    if (keyValuesDic.TryGetValue(key.Name, out value))
                    {
                        // TODO: make sure the enum key works
                        DeserializationHelpers.SetDeclaredProperty(source, EdmTypeKind.Primitive, key.Name, value, key, readContext);
                    }
                }

                return source;
            }

            internal static string GetServiceRoot(HttpRequestMessage request)
            {
                return request.GetUrlHelper().CreateODataLink(
                    request.ODataProperties().RouteName,
                    request.ODataProperties().PathHandler, new List<ODataPathSegment>());
            }

            internal static IEnumerable<KeyValuePair<string, object>> GetKeys(DefaultODataPathHandler pathHandler, IEdmModel edmModel, string serviceRoot, Uri uri)
            {
                ODataPath odataPath = pathHandler.Parse(edmModel, serviceRoot, uri.ToString());
                KeySegment segment = odataPath.Segments.OfType<KeySegment>().Last();
                if (segment == null)
                {
                    throw Error.InvalidOperation(SRResources.EntityReferenceMustHasKeySegment, uri);
                }

                return segment.Keys;
            }

            internal static ODataDeserializerContext BuildDeserializerContext(HttpActionContext actionContext,
                ModelBindingContext bindingContext, IEdmTypeReference edmTypeReference)
            {
                HttpRequestMessage request = actionContext.Request;
                ODataPath path = request.ODataProperties().Path;
                IEdmModel edmModel = request.ODataProperties().Model;

                return new ODataDeserializerContext
                {
                    Path = path,
                    Model = edmModel,
                    Request = request,
                    ResourceType = bindingContext.ModelType,
                    ResourceEdmType = edmTypeReference,
                    RequestContext = request.GetRequestContext()
                };
            }

            internal static object ConvertTo(string valueString, Type type)
            {
                if (valueString == null)
                {
                    return null;
                }

                if (type.IsNullable() && String.Equals(valueString, "null", StringComparison.Ordinal))
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

                object value = ODataUriUtils.ConvertFromUriLiteral(valueString, ODataVersion.V4);

                bool isNonStandardEdmPrimitive;
                EdmLibHelpers.IsNonstandardEdmPrimitive(type, out isNonStandardEdmPrimitive);

                if (isNonStandardEdmPrimitive)
                {
                    return EdmPrimitiveHelpers.ConvertPrimitiveValue(value, type);
                }
                else
                {
                    type = Nullable.GetUnderlyingType(type) ?? type;
                    return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
