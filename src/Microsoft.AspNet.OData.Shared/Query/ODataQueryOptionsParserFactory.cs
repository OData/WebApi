// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
#if !NETCORE
using System.Net.Http;
#else
using Microsoft.AspNetCore.Http;
#endif
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Factory for <see cref="IODataQueryOptionsParser"/> classes to handle parsing of OData query options in the request body.
    /// </summary>
    public static class ODataQueryOptionsParserFactory
    {
        /// <summary>
        /// Creates a list of <see cref="IODataQueryOptionsParser"/>s to handle parsing of OData query options in the request body.
        /// </summary>
        /// <returns>A list of <see cref="IODataQueryOptionsParser"/>s to handle parsing of OData query options in the request body.</returns>
        public static IList<IODataQueryOptionsParser> Create()
        {
            IList<IODataQueryOptionsParser> parsers = new List<IODataQueryOptionsParser>();

            parsers.Add(new PlainTextODataQueryOptionsParser());

            return parsers;
        }

        /// <summary>
        /// Gets the parser capable of parsing the query options in the request body.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>
        /// A parser capable of parsing the query options in the request body.
        /// An <see cref="ODataException"/> is thrown if no capable parser is found.
        /// </returns>
#if !NETCORE
        public static IODataQueryOptionsParser GetQueryOptionsParser(HttpRequestMessage request)
        {
            string contentType = request.Content.Headers.ContentType?.MediaType;
#else
        public static IODataQueryOptionsParser GetQueryOptionsParser(HttpRequest request)
        {
            string contentType = request.ContentType;
#endif

            IServiceProvider requestContainer = request.GetRequestContainer();
            Contract.Assert(requestContainer != null);

            // Fetch parsers available in the request container for parsing the query options in the request body
            IEnumerable<IODataQueryOptionsParser> parsers = requestContainer.GetRequiredService<IEnumerable<IODataQueryOptionsParser>>();
            IODataQueryOptionsParser parser = parsers.FirstOrDefault(d => d.CanParse(request));

            if (parser == null)
            {
                throw new ODataException(string.Format(
                    CultureInfo.InvariantCulture,
                    SRResources.CannotFindParserForRequestMediaType,
                    contentType));
            }

            return parser;
        }
    }
}
