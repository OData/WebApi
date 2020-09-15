// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
#if !NETCORE
using System.Net.Http;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Exposes the ability to read and parse the content of a <see cref="T:System.IO.Stream" /> 
    /// into a query options part of an OData URL. Query options may be passed 
    /// in the request body to a resource path ending in /$query.
    /// </summary>
    public interface IODataQueryOptionsParser
    {
        /// <summary>
        /// Determines whether this <see cref="IODataQueryOptionsParser"/> can parse the http request.
        /// </summary>
        /// <param name="request">The http request.</param>
        /// <returns>true if this <see cref="IODataQueryOptionsParser"/> can parse the http request; false otherwise.</returns>
#if !NETCORE
        bool CanParse(HttpRequestMessage request);
#else
        bool CanParse(HttpRequest request);
#endif

        /// <summary>
        /// Reads and parses the content of a <see cref="T:System.IO.Stream" /> 
        /// into a query options part of an OData URL.
        /// </summary>
        /// <param name="requestStream">A <see cref="T:System.IO.Stream" /> containing the query options.</param>
        /// <returns>A string representing the query options part of an OData URL.</returns>
        Task<string> ParseAsync(Stream requestStream);
    }
}
