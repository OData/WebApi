// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNetCore.OData.Routing.ODataPath;

namespace Microsoft.AspNetCore.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests for the raw value of properties.
    /// </summary>
    public abstract class ODataRawValueMediaTypeMapping
    {
        ///// <summary>
        ///// Initializes a new instance of the <see cref="ODataRawValueMediaTypeMapping"/> class.
        ///// </summary>
        //protected ODataRawValueMediaTypeMapping(string mediaType)
        //    : base(mediaType)
        //{
        //}

        ///// <inheritdoc/>
        //public override double TryMatchMediaType(HttpRequestMessage request)
        //{
        //    if (request == null)
        //    {
        //        throw Error.ArgumentNull("request");
        //    }

        //    return (IsRawValueRequest(request) && IsMatch(GetProperty(request))) ? 1 : 0;
        //}

        /// <summary>
        /// This method determines if the <see cref="HttpRequest"/> is an OData Raw value request.
        /// </summary>
        /// <param name="propertySegment">The <see cref="PropertyAccessPathSegment"/> of the path.</param>
        /// <returns>True if the request is an OData raw value request.</returns>
        protected abstract bool IsMatch(PropertySegment propertySegment);

        internal static bool IsRawValueRequest(HttpContext context)
        {
            ODataPath path = context.ODataFeature().Path;
            return path != null && path.Segments.LastOrDefault() is ValueSegment;
        }

        //private static PropertyAccessPathSegment GetProperty(HttpRequestMessage request)
        //{
        //    ODataPath odataPath = request.ODataProperties().Path;
        //    if (odataPath == null || odataPath.Segments.Count < 2)
        //    {
        //        return null;
        //    }
        //    return odataPath.Segments[odataPath.Segments.Count - 2] as PropertyAccessPathSegment;
        //}
    }
}
