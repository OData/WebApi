// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Class that provides <see cref="MediaTypeHeaderValue"/>s for OData from query strings.
    /// </summary>
    internal class ODataMediaTypeMapping : MediaTypeMapping
    {
        internal const string QueryStringFormatParameter = "format";
        internal const string QueryFormatODataValue = "odata";
        private static readonly Type typeODataMediaTypeMapping = typeof(ODataMediaTypeMapping);

        /// <summary>
        /// ODataMediaTypeMapping constructor.
        /// </summary>
        /// <param name="mediaType">The media type to use if the query parameter for OData is present </param>
        public ODataMediaTypeMapping(string mediaType)
            : base(mediaType)
        {
        }

        /// <summary>
        /// ODataMediaTypeMapping constructor.
        /// </summary>
        /// <param name="mediaType">The media type to use if the query parameter for OData is present </param>
        public ODataMediaTypeMapping(MediaTypeHeaderValue mediaType)
            : base(mediaType)
        {
        }

        /// <summary>
        /// Returns a value indicating the quality of the media type match for the current <see cref="ODataMediaTypeMapping"/>
        /// instance for <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to check.</param>
        /// <returns>If this instance can produce a <see cref="MediaTypeHeaderValue"/> from <paramref name="request"/>
        /// it returns <c>1.0</c> otherwise <c>false</c>.</returns>
        public override sealed double TryMatchMediaType(HttpRequestMessage request)
        {
            NameValueCollection queryString = GetQueryString(request.RequestUri);
            MediaTypeWithQualityHeaderValue responseMediaType = request.Headers.Accept.FirstOrDefault();
            if (responseMediaType == null)
            {
                return 0.0;
            }

            double quality = responseMediaType.Quality.HasValue ? responseMediaType.Quality.Value : 1.0;

            return String.Equals(responseMediaType.ToString(), MediaType.ToString(), StringComparison.OrdinalIgnoreCase) && DoesQueryStringMatch(queryString)
                        ? quality
                        : 0.0;
        }

        private static NameValueCollection GetQueryString(Uri uri)
        {
            if (uri == null)
            {
                throw new InvalidOperationException(String.Format("Uri cannot be null for {0}", typeODataMediaTypeMapping.Name));
            }

            return HttpUtility.ParseQueryString(uri.Query);
        }

        private bool DoesQueryStringMatch(NameValueCollection queryString)
        {
            if (queryString != null)
            {
                return queryString[ODataMediaTypeMapping.QueryStringFormatParameter] == ODataMediaTypeMapping.QueryFormatODataValue;
            }

            return false;
        }
    }
}
