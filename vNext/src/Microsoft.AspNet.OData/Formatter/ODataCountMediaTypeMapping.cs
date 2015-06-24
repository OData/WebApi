// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Media type mapping that associates requests with $count.
    /// </summary>
    public class ODataCountMediaTypeMapping
    {
        ///// <summary>
        ///// Initializes a new instance of the <see cref="ODataCountMediaTypeMapping"/> class.
        ///// </summary>
        //public ODataCountMediaTypeMapping()
        //    : base("text/plain")
        //{
        //}

        ///// <inheritdoc/>
        //public override double TryMatchMediaType(HttpRequestMessage request)
        //{
        //    if (request == null)
        //    {
        //        throw Error.ArgumentNull("request");
        //    }

        //    return IsCountRequest(request) ? 1 : 0;
        //}

        internal static bool IsCountRequest(HttpRequest request)
        {
            ODataPath path = request.ODataProperties().Path;
            return path != null && path.Segments.LastOrDefault() is CountPathSegment;
        }
    }
}
