// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Reads and parses the content of a <see cref="T:System.IO.Stream" /> 
    /// into a query options part of an OData URL. 
    /// The query options are passed in the request body as plain text.
    /// </summary>
    /// <remarks>This class derives from a platform-specific class.</remarks>
    public partial class PlainTextODataQueryOptionsParser : IODataQueryOptionsParser
    {
        /// <inheritdoc/>
        public bool CanParse(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("httpRequest");
            }

            return request.ContentType?.StartsWith(supportedMediaType.MediaType, StringComparison.Ordinal) == true ? true : false;
        }
    }
}
