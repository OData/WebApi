// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IRouteConstraint"/> that only matches a specific OData protocol 
    /// version. This constraint won't match incoming requests that contain any of the previous OData version
    /// headers (for OData versions 1.0 to 3.0) regardless of the version in the current version headers.
    /// </summary>
    public partial class ODataVersionConstraint : IRouteConstraint
    {
        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (httpContext == null)
            {
                throw Error.ArgumentNull("httpContext");
            }

            if (routeDirection == RouteDirection.UrlGeneration)
            {
                return true;
            }

            IDictionary<string, IEnumerable<string>> headers = httpContext.Request.Headers
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList() as IEnumerable<string>);

            ODataVersion? serviceVersion = httpContext.Request.ODataServiceVersion();
            ODataVersion? maxServiceVersion = httpContext.Request.ODataMaxServiceVersion();

            return IsVersionMatch(headers, serviceVersion, maxServiceVersion);
        }
    }
}
