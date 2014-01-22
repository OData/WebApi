// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.OData.Properties;

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s from query strings.
    /// </summary>
    public class QueryStringMediaTypeMapping : MediaTypeMapping
    {
        private static readonly Type _queryStringMediaTypeMappingType = typeof(QueryStringMediaTypeMapping);

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

        /// <summary>
        /// Gets the query string parameter name.
        /// </summary>
        public string QueryStringParameterName { get; private set; }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="QueryStringMediaTypeMapping"/>
        /// instance can return a <see cref="MediaTypeHeaderValue"/> from <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to check.</param>
        /// <returns>If this instance can produce a <see cref="MediaTypeHeaderValue"/> from <paramref name="request"/>
        /// it returns <c>1.0</c> otherwise <c>0.0</c>.</returns>
        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            NameValueCollection queryString = GetQueryString(request.RequestUri);
            return DoesQueryStringMatch(queryString) ? 1 : 0;
        }

        private static NameValueCollection GetQueryString(Uri uri)
        {
            if (uri == null)
            {
                throw Error.InvalidOperation(
                    SRResources.NonNullUriRequiredForMediaTypeMapping,
                    _queryStringMediaTypeMappingType.Name);
            }

            return new FormDataCollection(uri).ReadAsNameValueCollection();
        }

        private bool DoesQueryStringMatch(NameValueCollection queryString)
        {
            if (queryString != null)
            {
                string queryValue = queryString[QueryStringParameterName];
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
