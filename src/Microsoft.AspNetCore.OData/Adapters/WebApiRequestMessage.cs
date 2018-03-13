// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi request message to OData WebApi.
    /// </summary>
    internal class WebApiRequestMessage : IWebApiRequestMessage
    {
        /// <summary>
        /// The inner request wrapped by this instance.
        /// </summary>
        internal HttpRequest innerRequest;

        /// <summary>
        /// Initializes a new instance of the WebApiRequestMessage class.
        /// </summary>
        /// <param name="request">The inner request.</param>
        public WebApiRequestMessage(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            this.innerRequest = request;

            IODataFeature feature = request.ODataFeature();
            if (feature != null)
            {
                this.Context = new WebApiContext(feature);
            }

            // Get the ODataOptions from the global service provider.
            ODataOptions options = request.HttpContext.RequestServices.GetRequiredService<ODataOptions>();
            if (options != null)
            {
                this.Options = new WebApiOptions(options);
            }
        }

        /// <summary>
        /// Gets the contents of the HTTP message. 
        /// </summary>
        public IWebApiContext Context { get; private set; }

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
        /// <returns></returns>
        public bool IsCountRequest()
        {
            ODataPath path = this.innerRequest.ODataFeature().Path;
            return path != null && path.Segments.LastOrDefault() is CountSegment;
        }

        /// <summary>
        /// Gets the HTTP method used by the HTTP request message.
        /// </summary>
        public ODataRequestMethod Method
        {
            get
            {
                string method = this.innerRequest.Method.ToUpperInvariant();

                bool ignoreCase = true;
                ODataRequestMethod methodEnum = ODataRequestMethod.Unknown;
                if (Enum.TryParse<ODataRequestMethod>(method, ignoreCase, out methodEnum))
                {
                    return methodEnum;
                }

                return ODataRequestMethod.Unknown;
            }
        }

        /// <summary>
        /// Get the options associated with the request.
        /// </summary>
        public IWebApiOptions Options { get; private set; }

        /// <summary>
        /// The request container associated with the request.
        /// </summary>
        public IServiceProvider RequestContainer
        {
            get { return this.innerRequest.GetRequestContainer(); }
        }

        /// <summary>
        /// Gets the Uri used for the HTTP request.
        /// </summary>
        public Uri RequestUri
        {
            get { return new Uri(this.innerRequest.GetEncodedUrl()); }
        }

        /// <summary>
        /// get the deserializer provider associated with the request.
        /// </summary>
        /// <returns></returns>
        public ODataDeserializerProvider DeserializerProvider
        {
            get
            {
                return this.innerRequest.GetRequestContainer().GetRequiredService<ODataDeserializerProvider>();
            }
        }

        /// <summary>
        /// Get the next page link for a given page size.
        /// </summary>
        /// <param name="pageSize">The page size.</param>
        /// <returns></returns>
        public Uri GetNextPageLink(int pageSize)
        {
            return this.innerRequest.GetNextPageLink(pageSize);
        }

        /// <summary>
        /// Creates an ETag from concurrency property names and values.
        /// </summary>
        /// <param name="properties">The input property names and values.</param>
        /// <returns>The generated ETag string.</returns>
        public string CreateETag(IDictionary<string, object> properties)
        {
            IODataFeature feature = this.innerRequest.ODataFeature();
            if (feature == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            return this.innerRequest.GetETagHandler().CreateETag(properties)?.ToString();
        }

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag>.
        /// </summary>
        /// <remarks>This function uses types that are AspNet-specific.</remarks>
        public ETag GetETag(EntityTagHeaderValue etagHeaderValue)
        {
            return this.innerRequest.GetETag(etagHeaderValue);
        }

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag>.
        /// </summary>
        /// <remarks>This function uses types that are AspNet-specific.</remarks>
        public ETag GetETag<TEntity>(EntityTagHeaderValue etagHeaderValue)
        {
            return this.innerRequest.GetETag<TEntity>(etagHeaderValue);
        }

        /// <summary>
        /// Gets a list of content Id mappings associated with the request.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ODataContentIdMapping
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the path handler associated with the request.
        /// </summary>
        /// <returns></returns>
        public IODataPathHandler PathHandler
        {
            get { return this.innerRequest.GetPathHandler(); }
        }

        /// <summary>
        /// Gets the OData query parameters from the query.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ODataQueryParameters
        {
            get
            {
                return this.innerRequest.Query
                    .Where(kvp => kvp.Key.StartsWith("$", StringComparison.Ordinal) ||
                        kvp.Key.StartsWith("@", StringComparison.Ordinal))
                    .SelectMany(kvp => kvp.Value, (kvp, value) => new KeyValuePair<string, string>(kvp.Key, value))
                    .ToDictionary(p => p.Key, p => p.Value);
            }
        }

        /// <summary>
        /// Gets the reader settings associated with the request.
        /// </summary>
        /// <returns></returns>
        public ODataMessageReaderSettings ReaderSettings
        {
            get { return this.innerRequest.GetReaderSettings(); }
        }

        /// <summary>
        /// Gets the writer settings associated with the request.
        /// </summary>
        /// <returns></returns>
        public ODataMessageWriterSettings WriterSettings
        {
            get { return this.innerRequest.GetWriterSettings(); }
        }
    }
}
