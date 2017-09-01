// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;

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
        internal HttpRequestMessage innerRequest;

        /// <summary>
        /// Initializes a new instance of the WebApiRequestMessage class.
        /// </summary>
        /// <param name="request">The inner request.</param>
        public WebApiRequestMessage(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            this.innerRequest = request;

            HttpRequestMessageProperties context = request.ODataProperties();
            if (context != null)
            {
                this.Context = new WebApiContext(context);
            }

            UrlHelper uriHelper = request.GetUrlHelper();
            if (uriHelper != null)
            {
                this.UrlHelper = new WebApiUrlHelper(uriHelper);
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration != null)
            {
                this.Options = new WebApiOptions(configuration);
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
            return ODataCountMediaTypeMapping.IsCountRequest(this.innerRequest);
        }

        /// <summary>
        /// Gets the HTTP method used by the HTTP request message.
        /// </summary>
        public ODataRequestMethod Method
        {
            get
            {
                bool ignoreCase = true;
                ODataRequestMethod methodEnum = ODataRequestMethod.Unknown;
                if (Enum.TryParse<ODataRequestMethod>(this.innerRequest.Method.ToString(), ignoreCase, out methodEnum))
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
            get { return this.innerRequest.RequestUri; }
        }

        /// <summary>
        /// Gets or sets the <see cref="IWebApiUrlHelper"/> to use for generating OData links.
        /// </summary>
        public IWebApiUrlHelper UrlHelper { get; set; }

        /// <summary>
        /// Gets the deserializer provider associated with the request.
        /// </summary>
        /// <returns></returns>
        public ODataDeserializerProvider DeserializerProvider
        {
            get { return this.innerRequest.GetDeserializerProvider(); }
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
            HttpConfiguration configuration = this.innerRequest.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            return configuration.GetETagHandler().CreateETag(properties)?.ToString();
        }

        /// <summary>
        /// Gets a list of content Id mappings associated with the request.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ODataContentIdMapping
        {
            get { return this.innerRequest.GetODataContentIdMapping(); }
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
                return this.innerRequest.GetQueryNameValuePairs()
                    .Where(p => p.Key.StartsWith("$", StringComparison.Ordinal) ||
                    p.Key.StartsWith("@", StringComparison.Ordinal))
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
        /// Gets the route data for the given request or null if not available.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, object> RouteData
        {
            get { return this.innerRequest.GetRouteData().Values; }
        }
    }
}
