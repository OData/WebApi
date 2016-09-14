using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNetCore.OData.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData.Formatter
{
    public class ODataOutputFormatter : TextOutputFormatter
    {
        private readonly ODataMessageWriterSettings _messageWriterSettings;
        private readonly ODataSerializerProvider _serializerProvider;
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        public ODataOutputFormatter(IServiceProvider provider, ODataPayloadKind payloadKinds)
            : this(new DefaultODataSerializerProvider(provider), new[] { payloadKinds })
        {

        }

        public ODataOutputFormatter(ODataSerializerProvider serializerProvider, IEnumerable<ODataPayloadKind> payloadKinds)
        {
            _messageWriterSettings = new ODataMessageWriterSettings
            {
                // TODO: 
                //Indent = true,
                EnableMessageStreamDisposal = false,
                MessageQuotas = new ODataMessageQuotas { MaxReceivedMessageSize = Int64.MaxValue },
                //AutoComputePayloadMetadataInJson = true,
            };

            _serializerProvider = serializerProvider;
            _payloadKinds = payloadKinds;
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            return Task.Run(() => WriteResponseBody(context));
        }

        private void WriteResponseBody(OutputFormatterWriteContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;

            IEdmModel model = request.ODataProperties().Model;
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            object value = null;
            object graph = null;
            var objectResult = context.Object as PageResult<object>;
            if (objectResult != null)
            {
                value = objectResult.Items;
                graph = objectResult;
            }
            else
            {
                value = context.Object;
                graph = value;
            }
            var type = value.GetType();

            ODataSerializer serializer = GetSerializer(type, value, model, new DefaultODataSerializerProvider(context.HttpContext.RequestServices), request);

            IUrlHelper urlHelper = context.HttpContext.UrlHelper();

            ODataPath path = request.ODataProperties().Path;
            IEdmNavigationSource targetNavigationSource = path == null ? null : path.NavigationSource;

            string preferHeader = RequestPreferenceHelpers.GetRequestPreferHeader(request);
            string annotationFilter = null;
            if (!String.IsNullOrEmpty(preferHeader))
            {
                ODataMessageWrapper messageWrapper = new ODataMessageWrapper(response.Body, response.Headers);
                messageWrapper.SetHeader(RequestPreferenceHelpers.PreferHeaderName, preferHeader);
                annotationFilter = messageWrapper.PreferHeader().AnnotationFilter;
            }

            IODataResponseMessage responseMessage = new ODataMessageWrapper(response.Body, response.Headers);
            if (annotationFilter != null)
            {
                responseMessage.PreferenceAppliedHeader().AnnotationFilter = annotationFilter;
            }

            Uri baseAddress = GetBaseAddress(request);
            ODataMessageWriterSettings writerSettings = _messageWriterSettings.Clone();
            writerSettings.BaseUri = baseAddress;
            writerSettings.Version = ODataProperties.DefaultODataVersion;
            writerSettings.Validations = writerSettings.Validations & ~ValidationKinds.ThrowOnUndeclaredPropertyForNonOpenType;

            string metadataLink = urlHelper.CreateODataLink(request, MetadataSegment.Instance);
            if (metadataLink == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineMetadataUrl);
            }

            writerSettings.ODataUri = new ODataUri
            {
                ServiceRoot = baseAddress,

                // TODO: 1604 Convert webapi.odata's ODataPath to ODL's ODataPath, or use ODL's ODataPath.
                SelectAndExpand = request.ODataProperties().SelectExpandClause,
                Path = (path == null) ? null : path.ODLPath
                //Path = (path == null || IsOperationPath(path)) ? null : path.ODLPath,
            };

            using (ODataMessageWriter messageWriter = new ODataMessageWriter(responseMessage, writerSettings, model))
            {
                ODataSerializerContext writeContext = new ODataSerializerContext()
                {
                    Request = request,
                    RequestContext = request.HttpContext,
                    Url = urlHelper,
                    NavigationSource = targetNavigationSource,
                    Model = model,
                    RootElementName = GetRootElementName(path) ?? "root",
                    SkipExpensiveAvailabilityChecks = serializer.ODataPayloadKind == ODataPayloadKind.ResourceSet,
                    Path = path,
                    MetadataLevel = ODataMediaTypes.GetMetadataLevel(MediaTypeHeaderValue.Parse(context.ContentType.Value)),
                    SelectExpandClause = request.ODataProperties().SelectExpandClause,
                };

                serializer.WriteObject(graph, type, messageWriter, writeContext);
            }
        }

        public override void WriteResponseHeaders(OutputFormatterWriteContext context)
        {
            HttpRequest request = context.HttpContext.Request;
            HttpResponse response = context.HttpContext.Response;

            //// When the user asks for application/json we really need to set the content type to
            //// application/json; odata.metadata=minimal. If the user provides the media type and is
            //// application/json we are going to add automatically odata.metadata=minimal. Otherwise we are
            //// going to fallback to the default implementation.

            //// When calling this formatter as part of content negotiation the content negotiator will always
            //// pick a non null media type. In case the user creates a new ObjectContent<T> and doesn't pass in a
            //// media type, we delegate to the base class to rely on the default behavior. It's the user's 
            //// responsibility to pass in the right media type.
            //var mediaType = context.SelectedContentType;
            //if (mediaType != null)
            //{
            //    if (mediaType.MediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
            //        !mediaType.Parameters.Any(p => p.Name.Equals("odata.metadata", StringComparison.OrdinalIgnoreCase)))
            //    {
            //        mediaType.Parameters.Add(new NameValueHeaderValue("odata.metadata", "minimal"));
            //    }
            //}
            //else
            //{
            //    // This is the case when a user creates a new ObjectContent<T> passing in a null mediaType
            //    base.WriteResponseHeaders(context);
            //}

            //// In general, in Web API we pick a default charset based on the supported character sets
            //// of the formatter. However, according to the OData spec, the service shouldn't be sending
            //// a character set unless explicitly specified, so if the client didn't send the charset we chose
            //// we just clean it.
            //if (headers.ContentType != null &&
            //    !Request.Headers.AcceptCharset
            //        .Any(cs => cs.Value.Equals(headers.ContentType.CharSet, StringComparison.OrdinalIgnoreCase)))
            //{
            //    headers.ContentType.CharSet = String.Empty;
            //}

            response.Headers.Append(
                ODataProperties.ODataServiceVersionHeader,
                ODataUtils.ODataVersionToString(ODataProperties.DefaultODataVersion));

            base.WriteResponseHeaders(context);
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            Type type = null;
            var pageResult = context.Object as PageResult<object>;
            if (pageResult != null)
            {
                type = pageResult.Items.GetType();
            }
            else
            {
                type = context.Object.GetType();
            }
            var request = ((OutputFormatterWriteContext)context).HttpContext.Request;

            if (request != null)
            {
                IEdmModel model = request.ODataProperties().Model;
                if (model != null)
                {
                    ODataPayloadKind? payloadKind = null;

                    Type elementType;
                    if (typeof(IEdmObject).GetTypeInfo().IsAssignableFrom(type.GetTypeInfo()) ||
                        (type.IsCollection(out elementType) && typeof(IEdmObject).GetTypeInfo().IsAssignableFrom(elementType.GetTypeInfo())))
                    {
                        payloadKind = GetEdmObjectPayloadKind(type, request);
                    }
                    else
                    {
                        payloadKind = GetClrObjectResponsePayloadKind(type, request);
                    }

                    return payloadKind == null ? false : _payloadKinds.Contains(payloadKind.Value);
                }
            }

            return false;
        }

        private ODataSerializer GetSerializer(Type type, object value, IEdmModel model, ODataSerializerProvider serializerProvider, HttpRequest request)
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
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, edmType.ToTraceString(), typeof(ODataOutputFormatter).Name);
                    throw new SerializationException(message);
                }
            }
            else
            {
                // get the most appropriate serializer given that we support inheritance.
                type = value == null ? type : value.GetType();
                serializer = serializerProvider.GetODataPayloadSerializer(type, request);
                if (serializer == null)
                {
                    string message = Error.Format(SRResources.TypeCannotBeSerialized, type.Name, typeof(ODataOutputFormatter).Name);
                    throw new SerializationException(message);
                }
            }

            return serializer;
        }

        private static Uri GetBaseAddress(HttpRequest request)
        {
            IUrlHelper urlHelper = request.HttpContext.UrlHelper();

            string baseAddress = urlHelper.CreateODataLink(request);
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
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

        private ODataPayloadKind? GetEdmObjectPayloadKind(Type type, HttpRequest request)
        {
            //if (ODataCountMediaTypeMapping.IsCountRequest(Request))
            //{
            //    return ODataPayloadKind.Value;
            //}

            Type elementType;
            if (type.IsCollection(out elementType))
            {
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType))
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
                if (typeof(IEdmComplexObject).IsAssignableFrom(elementType))
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

        private ODataPayloadKind? GetClrObjectResponsePayloadKind(Type type, HttpRequest request)
        {
            // SingleResult<T> should be serialized as T.
            //if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SingleResult<>))
            //{
            //    type = type.GetGenericArguments()[0];
            //}

            ODataSerializer serializer = _serializerProvider.GetODataPayloadSerializer(type, request);
            return serializer == null ? null : (ODataPayloadKind?)serializer.ODataPayloadKind;
        }
    }
}