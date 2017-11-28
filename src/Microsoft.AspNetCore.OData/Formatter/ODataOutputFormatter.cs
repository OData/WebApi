// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// <see cref="TextOutputFormatter"/> class to handle OData.
    /// </summary>
    public class ODataOutputFormatter : TextOutputFormatter
    {
        private readonly ODataVersion _version;

        /// <summary>
        /// The set of payload kinds this formatter will accept in CanWriteType.
        /// </summary>
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        private readonly ODataSerializerProvider _serializerProvider;

        private HttpRequest _request;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOutputFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataOutputFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
            : this(ODataSerializerProviderProxy.Instance, payloadKinds)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOutputFormatter"/> class.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use.</param>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataOutputFormatter(ODataSerializerProvider serializerProvider, IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (serializerProvider == null)
            {
                throw Error.ArgumentNull("serializerProvider");
            }
            if (payloadKinds == null)
            {
                throw Error.ArgumentNull("payloadKinds");
            }

            _serializerProvider = serializerProvider;
            _payloadKinds = payloadKinds;
            _version = ODataVersionConstraint.DefaultODataVersion;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOutputFormatter"/> class.
        /// </summary>
        /// <param name="formatter">The <see cref="ODataOutputFormatter"/> to copy settings from.</param>
        /// <param name="version">The OData version that this formatter supports.</param>
        /// <param name="request">The <see cref="HttpRequest"/> for the per-request formatter instance.</param>
        /// <remarks>This is a copy constructor to be used in <see cref="GetPerRequestFormatterInstance"/>.</remarks>
        internal ODataOutputFormatter(ODataOutputFormatter formatter, ODataVersion version, HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            Contract.Assert(formatter._serializerProvider != null);
            Contract.Assert(formatter._payloadKinds != null);

            // Parameter 1: formatter

            // Execept for the other two parameters, this constructor is a copy constructor, and we need to copy
            // everything on the other instance.

            // Copy this class's private fields and internal properties.
            _serializerProvider = formatter._serializerProvider;
            _payloadKinds = formatter._payloadKinds;

            // Parameter 2: version
            _version = version;

            // Parameter 3: request
            Request = request;

            if (_serializerProvider.GetType() == typeof(ODataSerializerProviderProxy))
            {
                _serializerProvider = new ODataSerializerProviderProxy
                {
                    RequestContainer = request.GetRequestContainer(),
                };
            }
        }

        /// <summary>
        /// Gets the <see cref="ODataSerializerProvider"/> that will be used by this formatter instance.
        /// </summary>
        public ODataSerializerProvider SerializerProvider
        {
            get
            {
                return _serializerProvider;
            }
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base
        /// address for OData Uri.
        /// </summary>
        public Func<HttpRequest, Uri> BaseAddressFactory { get; set; }

        /// <summary>
        /// The request message associated with the per-request formatter instance.
        /// </summary>
        public HttpRequest Request
        {
            get { return _request; }
            set
            {
                ODataSerializerProviderProxy serializerProviderProxy = _serializerProvider as ODataSerializerProviderProxy;
                if (serializerProviderProxy != null && serializerProviderProxy.RequestContainer == null)
                {
                    serializerProviderProxy.RequestContainer = value.GetRequestContainer();
                }

                _request = value;
            }
        }

        /// <inheritdoc/>
        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            Type type = context.Object.GetType();
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            this.Request = context.HttpContext.Request;
            if (Request != null)
            {
                ODataPayloadKind? payloadKind;

                Type elementType;
                if (typeof(IEdmObject).IsAssignableFrom(type) ||
                    (TypeHelper.IsCollection(type, out elementType) && typeof(IEdmObject).IsAssignableFrom(elementType)))
                {
                    payloadKind = GetEdmObjectPayloadKind(type);
                }
                else
                {
                    payloadKind = GetClrObjectResponsePayloadKind(type);
                }

                return payloadKind == null ? false : _payloadKinds.Contains(payloadKind.Value);
            }

            return false;
        }

        private ODataPayloadKind? GetClrObjectResponsePayloadKind(Type type)
        {
            // SingleResult<T> should be serialized as T.
            if (TypeHelper.IsGenericType(type) && type.GetGenericTypeDefinition() == typeof(SingleResult<>))
            {
                type = type.GetGenericArguments()[0];
            }

            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type, Request);
            return serializer == null ? null : (ODataPayloadKind?)serializer.ODataPayloadKind;
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            Type type = context.ObjectType;
            object value = context.Object;
            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;

            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (Request == null)
            {
                throw Error.InvalidOperation(SRResources.WriteToStreamAsyncMustHaveRequest);
            }

            try
            {
                WriteToStream(type, value, response.Body, request, request.ContentType, request.ContentLength);
                return TaskHelpers.Completed();
            }
            catch (Exception ex)
            {
                return TaskHelpers.FromError(ex);
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Class coupling acceptable")]
        private void WriteToStream(Type type, object value, Stream writeStream, HttpRequest request, string contentType, long? contentLength)
        {
            IEdmModel model = request.GetModel();
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            ODataSerializer serializer = GetSerializer(type, value, _serializerProvider);

            ODataPath path = request.ODataFeature().Path;
            IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

            // serialize a response
            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(new WebApiRequestHeaders(request.Headers));
            string annotationFilter = null;
            if (!String.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = ODataMessageWrapperHelper.Create(writeStream, request.Headers);
                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            ODataMessageWrapper responseMessageWrapper = ODataMessageWrapperHelper.Create(writeStream, request.Headers, request.GetRequestContainer());
            IODataResponseMessage responseMessage = responseMessageWrapper;
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            Uri baseAddress = GetBaseAddressInternal(Request);
            ODataMessageWriterSettings writerSettings = request.GetWriterSettings();
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = _version;
            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            string metadataLink = request.HttpContext.GetUrlHelper().CreateODataLink(MetadataSegment.Instance);

            if (metadataLink == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
            }

            writerSettings.ODataUri = new ODataUri
            {
                ServiceRoot = baseAddress,

                // TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
                SelectAndExpand = Request.ODataFeature().SelectExpandClause,
                Apply = Request.ODataFeature().ApplyClause,
                Path = (path == null || IsOperationPath(path)) ? null : path.ODLPath,
            };

            ODataMetadataLevel metadataLevel = ODataMetadataLevel.MinimalMetadata;
            if (!contentLength.HasValue || contentLength.Value == 0)
            {
                // TODO: Rewrite the parameter part.
                //IEnumerable<KeyValuePair<string, string>> parameters =
                //    contentType.Parameters.Select(val => new KeyValuePair<string, string>(val.Name, val.Value));
                IEnumerable<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                metadataLevel = ODataMediaTypes.GetMetadataLevel(contentType, parameters);
            }

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = new ODataSerializerContext()
                {
                    Request = Request,
                    NavigationSource = targetNavigationSource,
                    Model = model,
                    RootElementName = GetRootElementName(path) ?? "root",
                    SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet,
                    Path = path,
                    MetadataLevel = metadataLevel,
                    SelectExpandClause = Request.ODataFeature().SelectExpandClause
                };

                serializer.WriteObject(value, type, messageWriter, writeContext);
            }
        }

        /// <summary>
        /// This method is to get payload kind for untyped scenario.
        /// </summary>
        private ODataPayloadKind? GetEdmObjectPayloadKind(Type type)
        {
            if (Request.IsCountRequest())
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

        private ODataSerializer GetSerializer(Type type, object value, ODataSerializerProvider serializerProvider)
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

                serializer = serializerProvider.GetEdmTypeSerializer(edmType);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString());
                    throw new SerializationException(message);
                }
            }
            else
            {
                var applyClause = Request.ODataFeature().ApplyClause;
                // get the most appropriate serializer given that we support inheritance.
                if (applyClause == null)
                {
                    type = value == null ? type : value.GetType();
                }

                serializer = serializerProvider.GetODataPayloadSerializer(type, Request);
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

        /// <summary>
        /// Internal method used for selecting the base address to be used with OData uris.
        /// If the consumer has provided a delegate for overriding our default implementation,
        /// we call that, otherwise we default to existing behavior below.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root; must terminate with a trailing '/'.</returns>
        private Uri GetBaseAddressInternal(HttpRequest request)
        {
            if (BaseAddressFactory != null)
            {
                return BaseAddressFactory(request);
            }
            else
            {
                return ODataOutputFormatter.GetDefaultBaseAddress(request);
            }
        }

        /// <summary>
        /// Returns a base address to be used in the service root when reading or writing OData uris.
        /// </summary>
        /// <param name="request">The HttpRequest object for the given request.</param>
        /// <returns>The base address to be used as part of the service root in the OData uri; must terminate with a trailing '/'.</returns>
        public static Uri GetDefaultBaseAddress(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            string baseAddress = request.HttpContext.GetUrlHelper().CreateODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
        }

        internal static ODataVersion GetODataResponseVersion(HttpRequest request)
        {
            // OData protocol requires that you send the minimum version that the client needs to know to
            // understand the response. There is no easy way we can figure out the minimum version that the client
            // needs to understand our response. We send response headers much ahead generating the response. So if
            // the requestMessage has a OData-MaxVersion, tell the client that our response is of the same
            // version; else use the DataServiceVersionHeader. Our response might require a higher version of the
            // client and it might fail. If the client doesn't send these headers respond with the default version
            // (V4).
            return request.ODataMaxServiceVersion() ??
                request.ODataServiceVersion() ??
                ODataVersionConstraint.DefaultODataVersion;
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