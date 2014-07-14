// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests with $count.
    /// </summary>
    public class ODataCountMediaTypeMapping : MediaTypeMapping
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataCountMediaTypeMapping"/> class.
        /// </summary>
        public ODataCountMediaTypeMapping()
            : base("text/plain")
        {
        }

        /// <inheritdoc/>
        public override double TryMatchMediaType(HttpRequestMessage request)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            return IsCountRequest(request) ? 1 : 0;
        }

        internal static bool IsCountRequest(HttpRequestMessage request)
        {
            ODataPath path = request.ODataProperties().Path;
            return path != null && path.Segments.LastOrDefault() is CountPathSegment;
        }
    }
}
