//-----------------------------------------------------------------------------
// <copyright file="QueryStringMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s from query strings.
    /// </summary>
    public partial class QueryStringMediaTypeMapping : MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryStringMediaTypeMapping"/> class.
        /// </summary>
        /// <param name="queryStringParameterName">The name of the query string parameter to match, if present.</param>
        /// <param name="mediaType">The <see cref="MediaTypeHeaderValue"/> to use if the query parameter specified by <paramref name="queryStringParameterName"/> is present
        /// and assigned the value specified by <paramref name="mediaType"/>.</param>
        public QueryStringMediaTypeMapping(string queryStringParameterName, MediaTypeHeaderValue mediaType)
            : base(mediaType)
        {
            if (queryStringParameterName == null)
            {
                throw Error.ArgumentNull("queryStringParameterName");
            }

            QueryStringParameterName = queryStringParameterName;
        }

        /// <inheritdocs />
        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            FormDataCollection queryString = GetQueryString(request.RequestUri);
            return DoesQueryStringMatch(queryString) ? 1 : 0;
        }

        private static FormDataCollection GetQueryString(Uri uri)
        {
            if (uri == null)
            {
                throw Error.InvalidOperation(
                    SRResources.NonNullUriRequiredForMediaTypeMapping,
                    typeof(QueryStringMediaTypeMapping).Name);
            }

            return new FormDataCollection(uri);
        }
    }
}
