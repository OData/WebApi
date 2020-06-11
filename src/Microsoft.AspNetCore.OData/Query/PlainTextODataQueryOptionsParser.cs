// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Query
{
    public partial class PlainTextODataQueryOptionsParser
    {
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
                    // Based on OData OASIS Standard, the request body is expected to contain the query portion of the URL 
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
