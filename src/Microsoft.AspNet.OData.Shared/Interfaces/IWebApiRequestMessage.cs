// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// Represents a HTTP request message.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be exposed publicly; it used for the internal
    /// implementations of SelectControl(). Any design which makes this class public
    /// should find an alternative design.
    /// </remarks>
    internal interface IWebApiRequestMessage
    {
        /// <summary>
        /// Gets the contents of the HTTP message.
        /// </summary>
        IWebApiContext Context { get; }

        /// <summary>
        /// Gets a value indicating if this is a count request.
        /// </summary>
        /// <returns></returns>
        bool IsCountRequest();

        /// <summary>
        /// Gets the HTTP method used by the HTTP request message.
        /// </summary>
        ODataRequestMethod Method { get; }

        /// <summary>
        /// Get the options associated with the request.
        /// </summary>
        IWebApiOptions Options { get; }

        /// <summary>
        /// Get the options associated with the request.
        /// </summary>
        IWebApiHeaders Headers { get; }

        /// <summary>
        /// The request container associated with the request.
        /// </summary>
        IServiceProvider RequestContainer { get; }

        /// <summary>
        /// Gets the Uri used for the HTTP request.
        /// </summary>
        Uri RequestUri { get; }

        /// <summary>
        /// Gets the deserializer provider associated with the request.
        /// </summary>
        /// <returns></returns>
        ODataDeserializerProvider DeserializerProvider { get; }

        /// <summary>
        /// Creates an ETag from concurrency property names and values.
        /// </summary>
        /// <param name="properties">The input property names and values.</param>
        /// <returns>The generated ETag string.</returns>
        string CreateETag(IDictionary<string, object> properties);

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag.
        /// </summary>
        ETag GetETag(EntityTagHeaderValue etagHeaderValue);

        /// <summary>
        /// Gets the EntityTagHeaderValue ETag.
        /// </summary>
        ETag GetETag<TEntity>(EntityTagHeaderValue etagHeaderValue);

        /// <summary>
        /// Get the next page link for a given page size.
        /// </summary>
        /// <param name="pageSize">The page size.</param>
        /// <param name="instance">The instance based on which the skiptoken value is generated</param>
        /// <param name="objToSkipTokenValue">Function that takes in the last object and returns the skiptoken value string.</param>
        /// <param name="encodeUrl">Optional Parameter to determine whether the nextpace url should be encoded.</param>
        /// <returns></returns>
        Uri GetNextPageLink(int pageSize, object instance, Func<object, string> objToSkipTokenValue,bool encodeUrl);

        /// <summary>
        /// Get a list of content Id mappings associated with the request.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> ODataContentIdMapping { get; }

        /// <summary>
        /// Get the path handler associated with the request.
        /// </summary>
        /// <returns></returns>
        IODataPathHandler PathHandler { get; }

        /// <summary>
        /// Get the name value pairs from the query.
        /// </summary>
        /// <returns></returns>
        IDictionary<string, string> QueryParameters { get; }

        /// <summary>
        /// Get the reader settings associated with the request.
        /// </summary>
        /// <returns></returns>
        ODataMessageReaderSettings ReaderSettings { get; }

        /// <summary>
        /// Gets the writer settings associated with the request.
        /// </summary>
        /// <returns></returns>
        ODataMessageWriterSettings WriterSettings { get; }
    }
}
