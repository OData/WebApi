//-----------------------------------------------------------------------------
// <copyright file="ODataInputFormatter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Results;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// <see cref="TextInputFormatter"/> class to handle OData.
    /// </summary>
    public class ODataInputFormatter : TextInputFormatter
    {
        /// <summary>
        /// The set of payload kinds this formatter will accept in CanReadType.
        /// </summary>
        private readonly IEnumerable<ODataPayloadKind> _payloadKinds;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataInputFormatter"/> class.
        /// </summary>
        /// <param name="payloadKinds">The kind of payloads this formatter supports.</param>
        public ODataInputFormatter(IEnumerable<ODataPayloadKind> payloadKinds)
        {
            if (payloadKinds == null)
            {
                throw Error.ArgumentNull("payloadKinds");
            }

            _payloadKinds = payloadKinds;
        }

        /// <summary>
        /// Gets or sets a method that allows consumers to provide an alternate base
        /// address for OData Uri.
        /// </summary>
        public Func<HttpRequest, Uri> BaseAddressFactory { get; set; }

        /// <inheritdoc/>
        public override bool CanRead(InputFormatterContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.ReadFromStreamAsyncMustHaveRequest);
            }

            // Ignore non-OData requests.
            if (request.ODataFeature().Path == null)
            {
                return false;
            }

            Type type = context.ModelType;
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            ODataDeserializerProvider deserializerProvider = request.GetRequestContainer().GetRequiredService<ODataDeserializerProvider>();

            return ODataInputFormatterHelper.CanReadType(
                type,
                request.GetModel(),
                request.ODataFeature().Path,
                _payloadKinds,
                (objectType) => deserializerProvider.GetEdmTypeDeserializer(objectType),
                (objectType) => deserializerProvider.GetODataDeserializer(objectType, request));
        }

        /// <inheritdoc/>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception type is reflected into a faulted task.")]
        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            Type type = context.ModelType;
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            HttpRequest request = context.HttpContext.Request;
            if (request == null)
            {
                throw Error.InvalidOperation(SRResources.ReadFromStreamAsyncMustHaveRequest);
            }

            // If content length is 0 then return default value for this type
            RequestHeaders contentHeaders = request.GetTypedHeaders();
            object defaultValue = GetDefaultValueForType(type);
            if (contentHeaders == null || contentHeaders.ContentLength == 0)
            {
                return InputFormatterResult.Success(defaultValue);
            }

            try
            {
                Func<ODataDeserializerContext> getODataDeserializerContext = () =>
                {
                    return new ODataDeserializerContext
                    {
                        Request = request,
                    };
                };

                Action<Exception> logErrorAction = (ex) =>
                {
                    ILogger logger = context.HttpContext.RequestServices.GetService<ILogger>();
                    if (logger == null)
                    {
                        throw ex;
                    }

                    logger.LogError(ex, String.Empty);
                    if (context.HttpContext.Request.GetCompatibilityOptions().HasFlag(CompatibilityOptions.ThrowExceptionAfterLoggingModelStateError))
                    {
                        throw ex;
                    }
                };

                List<IDisposable> toDispose = new List<IDisposable>();

                ODataDeserializerProvider deserializerProvider = request.GetRequestContainer().GetRequiredService<ODataDeserializerProvider>();

                object result = await ODataInputFormatterHelper.ReadFromStreamAsync(
                    type,
                    defaultValue,
                    request.GetModel(),
                    ResultHelpers.GetODataVersion(request),
                    GetBaseAddressInternal(request),
                    new WebApiRequestMessage(request),
                    () => ODataMessageWrapperHelper.Create(new StreamWrapper(request.Body), request.Headers, request.GetODataContentIdMapping(), request.GetRequestContainer()),
                    (objectType) => deserializerProvider.GetEdmTypeDeserializer(objectType),
                    (objectType) => deserializerProvider.GetODataDeserializer(objectType, request),
                    getODataDeserializerContext,
                    (disposable) => toDispose.Add(disposable),
                    logErrorAction);

                foreach (IDisposable obj in toDispose)
                {
                    obj.Dispose();
                }

                return InputFormatterResult.Success(result);
            }
            catch (Exception ex)
            {
                context.ModelState.AddModelError(context.ModelName, ex, context.Metadata);
                return InputFormatterResult.Failure();
            }
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
                return ODataInputFormatter.GetDefaultBaseAddress(request);
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

            string baseAddress = request.GetUrlHelper().CreateODataLink();
            if (baseAddress == null)
            {
                throw new SerializationException(SRResources.UnableToDetermineBaseUrl);
            }

            return baseAddress[baseAddress.Length - 1] != '/' ? new Uri(baseAddress + '/') : new Uri(baseAddress);
        }

        /// <inheritdoc />
        public override IReadOnlyList<string> GetSupportedContentTypes(string contentType, Type objectType)
        {
            if (SupportedMediaTypes.Count == 0)
            {
                // note: this is parity with the base implementation when there are no matches
                return default;
            }

            return base.GetSupportedContentTypes(contentType, objectType);
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
    }
}
