// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
#if !NETCORE
using System.Net.Http.Formatting;
#else
using Microsoft.AspNet.OData.Formatter;
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
        /// Gets the media type supported by the parser.
        /// </summary>
        MediaTypeMapping MediaTypeMapping { get; }

        /// <summary>
        /// Reads and parses the content of a <see cref="T:System.IO.Stream" /> 
        /// into a query options part of an OData URL.
        /// </summary>
        /// <param name="requestStream">A <see cref="T:System.IO.Stream" /> containing the query options.</param>
        /// <returns>A string representing the query options part of an OData URL.</returns>
        string Parse(Stream requestStream);
    }
}
