// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.OData;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Query
{
    public partial class PlainTextODataQueryOptionsParser
    {
        /// <inheritdoc/>
        public string Parse(Stream requestStream)
        {
            string queryString = string.Empty;

            MemoryStream memoryStream = new MemoryStream();
            TextReader reader;

            try
            {
                // Reset request stream position if possible
                if (requestStream.CanSeek)
                {
                    requestStream.Position = 0;
                }

                requestStream.CopyToAsync(memoryStream);
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

                    // For consideration: support multiline
                    // - Would it be a single query option per line?
                    // - Would the parser be responsible for adding the & separator?
                    // - Would the parser try to detect the & separator and add where necessary?
                    string result = reader.ReadToEndAsync().Result;

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
