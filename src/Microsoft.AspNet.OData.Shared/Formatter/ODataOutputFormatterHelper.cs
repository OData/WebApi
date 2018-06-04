﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Formatter
{
    internal static class ODataOutputFormatterHelper
    {
        internal static bool TryGetContentHeader(Type type, MediaTypeHeaderValue mediaType, out MediaTypeHeaderValue newMediaType)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            newMediaType = null;

            // When the user asks for application/json we really need to set the content type to
            // application/json; odata.metadata=minimal. If the user provides the media type and is
            // application/json we are going to add automatically odata.metadata=minimal. Otherwise we are
            // going to fallback to the default implementation.

            // When calling this formatter as part of content negotiation the content negotiator will always
            // pick a non null media type. In case the user creates a new ObjectContent<T> and doesn't pass in a
            // media type, we delegate to the base class to rely on the default behavior. It's the user's
            // responsibility to pass in the right media type.

            if (mediaType != null)
            {
                if (mediaType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
                    !mediaType.Parameters.Any(p => p.Name.Equals("odata.metadata", StringComparison.OrdinalIgnoreCase)))
                {
                    mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
                }

                newMediaType = (MediaTypeHeaderValue)((ICloneable)mediaType).Clone();
                return true;
            }
            else
            {
                // This is the case when a user creates a new ObjectContent<T> passing in a null mediaType
                return false;
            }
        }

        internal static bool TryGetCharSet(MediaTypeHeaderValue mediaType, IEnumerable<string> acceptCharsetValues, out string charSet)
        {
            charSet = String.Empty;

            // In general, in Web API we pick a default charset based on the supported character sets
            // of the formatter. However, according to the OData spec, the service shouldn't be sending
            // a character set unless explicitly specified, so if the client didn't send the charset we chose
            // we just clean it.
            if (mediaType != null &&
                !acceptCharsetValues
                    .Any(cs => cs.Equals(mediaType.CharSet, StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            return false;
        }

        internal static bool CanWriteType(
            Type type,
            IEnumerable<ODataPayloadKind> payloadKinds,
            bool isGenericSingleResult,
            IWebApiRequestMessage internalRequest,
            Func<Type, ODataSerializer> getODataPayloadSerializer)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            ODataPayloadKind? payloadKind;

            Type elementType;
            if (typeof(IEdmObject).IsAssignableFrom(type) ||
                (TypeHelper.IsCollection(type, out elementType) && typeof(IEdmObject).IsAssignableFrom(elementType)))
            {
                payloadKind = GetEdmObjectPayloadKind(type, internalRequest);
            }
            else
            {
                payloadKind = GetClrObjectResponsePayloadKind(type, isGenericSingleResult, getODataPayloadSerializer);
            }

            return payloadKind == null ? false : payloadKinds.Contains(payloadKind.Value);
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        internal static void WriteToStream(
            Type type,
            object value,
            IEdmModel model,
            ODataVersion version,
            Uri baseAddress,
            MediaTypeHeaderValue contentType,
            IWebApiUrlHelper internaUrlHelper,
            IWebApiRequestMessage internalRequest,
            IWebApiHeaders internalRequestHeaders,
            Func<IServiceProvider, ODataMessageWrapper> getODataMessageWrapper,
            Func<IEdmTypeReference, ODataSerializer> getEdmTypeSerializer,
            Func<Type, ODataSerializer> getODataPayloadSerializer,
            Func<ODataSerializerContext> getODataSerializerContext)
        {
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            ODataSerializer serializer = GetSerializer(type, value, internalRequest, getEdmTypeSerializer, getODataPayloadSerializer);

            ODataPath path = internalRequest.Context.Path;
            IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

            // serialize a response
            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(internalRequestHeaders);
            string annotationFilter = null;
            if (!String.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = getODataMessageWrapper(null);
                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            ODataMessageWrapper responseMessageWrapper = getODataMessageWrapper(internalRequest.RequestContainer);
            IODataResponseMessage responseMessage = responseMessageWrapper;
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            ODataMessageWriterSettings writerSettings = internalRequest.WriterSettings;
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = version;
            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            string metadataLink = internaUrlHelper.CreateODataLink(MetadataSegment.Instance);

            if (metadataLink == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
            }

            writerSettings.ODataUri = new ODataUri
            {
                ServiceRoot = baseAddress,

                // TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
                SelectAndExpand = internalRequest.Context.SelectExpandClause,
                Apply = internalRequest.Context.ApplyClause,
                Path = (path == null || IsOperationPath(path)) ? null : path.Path,
            };

            ODataMetadataLevel metadataLevel = ODataMetadataLevel.MinimalMetadata;
            if (contentType != null)
            {
                IEnumerable<KeyValuePair<string, string>> parameters =
                    contentType.Parameters.Select(val => new KeyValuePair<string, string>(val.Name, val.Value));
                metadataLevel = ODataMediaTypes.GetMetadataLevel(contentType.MediaType, parameters);
            }

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = getODataSerializerContext();
                writeContext.NavigationSource = targetNavigationSource;
                writeContext.Model = model;
                writeContext.RootElementName = GetRootElementName(path) ?? "root";
                writeContext.SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet;
                writeContext.Path = path;
                writeContext.MetadataLevel = metadataLevel;
                writeContext.SelectExpandClause = internalRequest.Context.SelectExpandClause;

                serializer.WriteObject(value, type, messageWriter, writeContext);
            }
        }

        private static ODataPayloadKind? GetClrObjectResponsePayloadKind(Type type, bool isGenericSingleResult, Func<Type, ODataSerializer> getODataPayloadSerializer)
        {
            // SingleResult<T> should be serialized as T.
            if (isGenericSingleResult)
            {
                type = type.GetGenericArguments()[0];
            }

            ODataSerializer serializer = getODataPayloadSerializer(type);
            return serializer == null ? null : (ODataPayloadKind?)serializer.ODataPayloadKind;
        }

        private static ODataPayloadKind? GetEdmObjectPayloadKind(Type type, IWebApiRequestMessage internalRequest)
        {
            if (internalRequest.IsCountRequest())
            {
                return ODataPayloadKind.Value;
            }

            Type elementType;
            if (TypeHelper.IsCollection(type, out elementType))
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType) || typeof(IEdmEnumObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Collection;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.ResourceSet;
                }
                else if (typeof(IEdmChangedObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Delta;
                }
            }
            else
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType) || typeof(IEdmEnumObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Property;
                }
                else if (typeof(IEdmEntityObject).IsAssignableFrom(elementType))
                {
                    return ODataPayloadKind.Resource;
                }
            }

            return null;
        }

        private static ODataSerializer GetSerializer(Type type, object value, IWebApiRequestMessage internalRequest, Func<IEdmTypeReference, ODataSerializer> getEdmTypeSerializer, Func<Type, ODataSerializer> getODataPayloadSerializer)
        {
            ODataSerializer serializer;

            IEdmObject edmObject = value as IEdmObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmType = edmObject.GetEdmType();
                if (edmType == null)
                {
                    throw new SerializationException(Error.Format(SRResources.EdmTypeCannotBeNull,
                        edmObject.GetType().FullName, typeof(IEdmObject).Name));
                }

                serializer = getEdmTypeSerializer(edmType);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString());
                    throw new SerializationException(message);
                }
            }
            else
            {
                var applyClause = internalRequest.Context.ApplyClause;
                // get the most appropriate serializer given that we support inheritance.
                if (applyClause == null)
                {
                    type = value == null ? type : value.GetType();
                }

                serializer = getODataPayloadSerializer(type);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, type.Name);
                    throw new SerializationException(message);
                }
            }

            return serializer;
        }

        private static string GetRootElementName(ODataPath path)
        {
            if (path != null)
            {
                ODataPathSegment lastSegment = path.Segments.LastOrDefault();
                if (lastSegment != null)
                {
                    OperationSegment actionSegment = lastSegment as OperationSegment;
                    if (actionSegment != null)
                    {
                        IEdmAction action = actionSegment.Operations.Single() as IEdmAction;
                        if (action != null)
                        {
                            return action.Name;
                        }
                    }

                    PropertySegment propertyAccessSegment = lastSegment as PropertySegment;
                    if (propertyAccessSegment != null)
                    {
                        return propertyAccessSegment.Property.Name;
                    }
                }
            }
            return null;
        }

        // This function is used to determine whether an OData path includes operation (import) path segments.
        // We use this function to make sure the value of ODataUri.Path in ODataMessageWriterSettings is null
        // when any path segment is an operation. ODL will try to calculate the context URL if the ODataUri.Path
        // equals to null.
        private static bool IsOperationPath(ODataPath path)
        {
            if (path == null)
            {
                return false;
            }

            foreach (ODataPathSegment segment in path.Segments)
            {
                if (segment is OperationSegment ||
                    segment is OperationImportSegment)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
