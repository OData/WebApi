// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.AspNet.OData.Common;
using System.Net.Http;
using System.Net.Http.Headers;

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
        private static MediaTypeHeaderValue supportedMediaType = MediaTypeHeaderValue.Parse("text/plain");

        /// <inheritdoc/>
        public bool CanParse(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("httpRequest");
            }

            MediaTypeHeaderValue contentType = request.Content.Headers.ContentType;

            return contentType?.MediaType?.StartsWith(supportedMediaType.MediaType, StringComparison.Ordinal) == true ? true : false;
        }

        /// <inheritdoc/>
        public async Task<string> ParseAsync(Stream requestStream)
        {
            string queryString = string.Empty;

            MemoryStream memoryStream = new MemoryStream();
            TextReader reader;

            try
            {
                await requestStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                reader = new StreamReader(memoryStream);
            }
            catch
            {
                memoryStream.Dispose();

                throw new ODataException(SRResources.CannotParseQueryOptionsPayload);
            }

            using (reader)
            {
                try
                {
                    // Based on OData OASIS Standard, the request body is expected to contain the query options part of OData URL 
                    // and MUST use the same percent-encoding as in URLs (especially: no spaces, tabs, or line breaks allowed) 
                    // and MUST follow the expected syntax rules
                    string result = await reader.ReadToEndAsync();

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        // Query string is expected to start with ?
                        queryString = result[0] == '?' ? result : '?' + result;
                    }
                }
                catch
                {
                    throw new ODataException(SRResources.CannotParseQueryOptionsPayload);
                }
            }

            return queryString;
        }
    }
}
