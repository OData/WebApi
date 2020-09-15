// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Query
{
    public partial class PlainTextODataQueryOptionsParser
    {
        private static MediaTypeHeaderValue supportedMediaType = MediaTypeHeaderValue.Parse("text/plain");

        /// <inheritdoc/>
        public async Task<string> ParseAsync(Stream requestStream)
        {
            try
            {
                using (var reader = new StreamReader(
                    requestStream,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true))
                {
                    // Based on OData OASIS Standard, the request body is expected to contain the query portion of the URL
                    // and MUST use the same percent-encoding as in URLs (especially: no spaces, tabs, or line breaks allowed)
                    // and MUST follow the expected syntax rules
                    return await reader.ReadToEndAsync();
                }
            }
            catch
            {
                throw new ODataException(SRResources.CannotParseQueryOptionsPayload);
            }
        }
    }
}
