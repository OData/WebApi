// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Reads and parses the content of a <see cref="T:System.IO.Stream" /> 
    /// into a query options part of an OData URL. 
    /// The query options are passed in the request body as plain text.
    /// </summary>
    /// <remarks>This class derives from a platform-specific class.</remarks>
    [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", Justification = "Spelling correct in this context")]
    public partial class PlainTextODataQueryOptionsParser : IODataQueryOptionsParser
    {
        /// <inheritdoc/>
        public bool CanParse(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("httpRequest");
            }

            MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;

            return contentType?.MediaType?.StartsWith(SupportedMediaType.MediaType, StringComparison.Ordinal) == true ? true : false;
        }
    }
}
