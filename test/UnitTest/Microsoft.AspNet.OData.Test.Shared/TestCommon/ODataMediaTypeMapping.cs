//-----------------------------------------------------------------------------
// <copyright file="ODataMediaTypeMapping.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETCORE
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
#endif

namespace Microsoft.AspNet.OData.Test.Common
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
#if NETCORE
        public ODataMediaTypeMapping(MediaTypeHeaderValue mediaType)
            : base(mediaType.ToString())
        {
        }
#else
        public ODataMediaTypeMapping(MediaTypeHeaderValue mediaType)
            : base(mediaType)
        {
        }
#endif

        /// <summary>
        /// Returns a value indicating the quality of the media type match for the current <see cref="ODataMediaTypeMapping"/>
        /// instance for <paramref name="request"/>.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/> to check.</param>
        /// <returns>If this instance can produce a <see cref="MediaTypeHeaderValue"/> from <paramref name="request"/>
        /// it returns <c>1.0</c> otherwise <c>false</c>.</returns>
#if NETCORE
        public override sealed double TryMatchMediaType(HttpRequest request)
        {
            IDictionary<string, string> queryString = QueryHelpers.ParseNullableQuery(request.QueryString.Value)
                .Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.FirstOrDefault()))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            MediaTypeHeaderValue responseMediaType = request.GetTypedHeaders().Accept.FirstOrDefault();
#else
        public override sealed double TryMatchMediaType(HttpRequestMessage request)
        {
            FormDataCollection queryString = new FormDataCollection(request.RequestUri);
            MediaTypeWithQualityHeaderValue responseMediaType = request.Headers.Accept.FirstOrDefault();
#endif
            if (responseMediaType == null)
            {
                return 0.0;
            }

            double quality = responseMediaType.Quality.HasValue ? responseMediaType.Quality.Value : 1.0;

            return String.Equals(responseMediaType.ToString(), MediaType.ToString(), StringComparison.OrdinalIgnoreCase) && DoesQueryStringMatch(queryString)
                        ? quality
                        : 0.0;
        }

        private bool DoesQueryStringMatch(IEnumerable<KeyValuePair<string, string>> queryString)
        {
            if (queryString != null)
            {
                string queryValue = queryString.Where(kvp => kvp.Key == ODataMediaTypeMapping.QueryStringFormatParameter)
                      .FirstOrDefault()
                      .Value;

                if (queryValue != null)
                {
                    return queryValue == ODataMediaTypeMapping.QueryFormatODataValue;
                }
            }

            return false;
        }
    }
}
