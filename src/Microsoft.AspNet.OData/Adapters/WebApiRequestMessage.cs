// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
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
            this.Headers = new WebApiRequestHeaders(request.Headers);

            HttpRequestMessageProperties context = request.ODataProperties();
            if (context != null)
            {
                this.Context = new WebApiContext(context);
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
        /// WebAPI headers associated with the request
        /// </summary>
        public IWebApiHeaders Headers { get; private set; }

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
        /// <returns></returns>
        public bool IsCountRequest()
        {
            return ODataCountMediaTypeMapping.IsCountRequest(this.innerRequest.ODataProperties().Path);
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
        /// <param name="instance">The instance based on which the skiptoken value is generated</param>
        /// <param name="objToSkipTokenValue">Function that takes in the last object and returns the skiptoken value string.</param>
        /// <returns>Returns the uri for the nextlink</returns>
        public Uri GetNextPageLink(int pageSize, object instance = null, Func<object, string> objToSkipTokenValue = null)
        {
            return this.innerRequest.GetNextPageLink(pageSize, instance, objToSkipTokenValue);
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

            EntityTagHeaderValue etag = configuration.GetETagHandler().CreateETag(properties);
            return etag != null ? etag.ToString() : null;
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
        /// Gets the query parameters from the query with duplicated key ignored.
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> QueryParameters
        {
            get
            {
                IDictionary<string, string> result = new Dictionary<string, string>();
                foreach (var pair in this.innerRequest.GetQueryNameValuePairs())
                {
                    if (!result.ContainsKey(pair.Key))
                    {
                        result.Add(pair.Key, pair.Value);
                    }
                }
                return result;
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
