// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if !NETCORE
using System.Net.Http.Formatting;
#endif
using Microsoft.AspNet.OData.Formatter;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Reads and parses the content of a <see cref="T:System.IO.Stream" /> 
    /// into a query options part of an OData URL. 
    /// The query options are passed in the request body as plain text.
    /// </summary>
    /// <remarks>This class derives from a platform-specific class.</remarks>
    public partial class TextPlainODataQueryOptionsParser : IODataQueryOptionsParser
    {
        /// <inheritdoc/>
        public MediaTypeMapping MediaTypeMapping { get; } = new ContentTypeMediaTypeMapping("text/plain");
    }
}
