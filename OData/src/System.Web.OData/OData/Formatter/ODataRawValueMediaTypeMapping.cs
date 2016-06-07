// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.OData.Extensions;
using Microsoft.OData.UriParser;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests for the raw value of properties.
    /// </summary>
    public abstract class ODataRawValueMediaTypeMapping : MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataRawValueMediaTypeMapping"/> class.
        /// </summary>
        protected ODataRawValueMediaTypeMapping(string mediaType)
            : base(mediaType)
        {
        }

        /// <inheritdoc/>
        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return (IsRawValueRequest(request) && IsMatch(GetProperty(request))) ? 1 : 0;
        }

        /// <summary>
        /// This method determines if the <see cref="HttpRequestMessage"/> is an OData Raw value request.
        /// </summary>
        /// <param name="propertySegment">The <see cref="PropertySegment"/> of the path.</param>
        /// <returns>True if the request is an OData raw value request.</returns>
        protected abstract bool IsMatch(PropertySegment propertySegment);

        internal static bool IsRawValueRequest(HttpRequestMessage request)
        {
            ODataPath path = request.ODataProperties().Path;
            return path != null && path.Segments.LastOrDefault() is ValueSegment;
        }

        private static PropertySegment GetProperty(HttpRequestMessage request)
        {
            ODataPath odataPath = request.ODataProperties().Path;
            if (odataPath == null || odataPath.Segments.Count < 2)
            {
                return null;
            }
            return odataPath.Segments[odataPath.Segments.Count - 2] as PropertySegment;
        }
    }
}
