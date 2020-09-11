// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;

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
    }
}
