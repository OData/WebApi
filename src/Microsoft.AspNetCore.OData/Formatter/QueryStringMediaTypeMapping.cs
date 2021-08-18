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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s from query strings.
    /// </summary>
    /// <remarks>This class derives from a platform-specific class.</remarks>
    public partial class QueryStringMediaTypeMapping : MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringMediaTypeMapping"/> class.
        /// </summary>
        /// <param name="queryStringParameterName">The name of the query string parameter to match, if present.</param>
        /// <param name="queryStringParameterValue">The value of the query string parameter to match, if present.</param>
        /// <param name="mediaType">The media type to use if the query parameter specified by <paramref name="queryStringParameterName"/> is present
        /// and assigned the value specified by <paramref name="mediaType"/>.</param>
        public QueryStringMediaTypeMapping(string queryStringParameterName, string queryStringParameterValue, string mediaType)
            : base(mediaType)
        {
            if (queryStringParameterName == null)
            {
                throw Error.ArgumentNull("queryStringParameterName");
            }

            QueryStringParameterName = queryStringParameterName;
            QueryStringParameterValue = queryStringParameterValue;
        }

        /// <summary>
        /// Gets the query string parameter value.
        /// </summary>
        public string QueryStringParameterValue { get; private set; }

        /// <inheritdocs />
        public override double TryMatchMediaType(HttpRequest request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            double quality = 0;

            QueryString queryString = request.QueryString;
            if (queryString.HasValue)
            {
                Dictionary<string, StringValues> parsedQuery = QueryHelpers.ParseNullableQuery(queryString.Value);
                if (parsedQuery != null)
                {
                    IDictionary<string, string> queryValues =parsedQuery
                        .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.FirstOrDefault()))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                    quality = DoesQueryStringMatch(queryValues) ? 1 : 0;
                    if (quality < 1)
                    {
                        string queryValue = queryValues.Where(kvp => kvp.Key == QueryStringParameterName)
                            .FirstOrDefault()
                            .Value;

                        quality = (!string.IsNullOrEmpty(queryValue) && (queryValue == QueryStringParameterValue)) ? 1 : 0;
                    }
                }
            }

            return quality;
        }
    }
}
