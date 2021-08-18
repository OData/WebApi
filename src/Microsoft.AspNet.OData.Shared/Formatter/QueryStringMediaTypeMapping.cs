//-----------------------------------------------------------------------------
// <copyright file="QueryStringMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s from query strings.
    /// </summary>
    public partial class QueryStringMediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringMediaTypeMapping"/> class.
        /// </summary>
        /// <param name="queryStringParameterName">The name of the query string parameter to match, if present.</param>
        /// <param name="mediaType">The media type to use if the query parameter specified by <paramref name="queryStringParameterName"/> is present
        /// and assigned the value specified by <paramref name="mediaType"/>.</param>
        public QueryStringMediaTypeMapping(string queryStringParameterName, string mediaType)
            : base(mediaType)
        {
            if (queryStringParameterName == null)
            {
                throw Error.ArgumentNull("queryStringParameterName");
            }

            QueryStringParameterName = queryStringParameterName;
        }

        /// <summary>
        /// Gets the query string parameter name.
        /// </summary>
        public string QueryStringParameterName { get; private set; }

        private bool DoesQueryStringMatch(IEnumerable<KeyValuePair<string, string>> queryString)
        {
            if (queryString != null)
            {
                string queryValue = queryString.Where(kvp => kvp.Key == QueryStringParameterName)
                    .FirstOrDefault()
                    .Value;

                if (queryValue != null)
                {
                    // construct a media type from the query value
                    MediaTypeHeaderValue parsedValue;
                    bool success = MediaTypeHeaderValue.TryParse(queryValue, out parsedValue);
                    if (success && MediaType.Equals(parsedValue))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
