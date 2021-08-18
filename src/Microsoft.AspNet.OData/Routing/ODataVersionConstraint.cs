//-----------------------------------------------------------------------------
// <copyright file="ODataVersionConstraint.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing
{
    /// <summary>
    /// An implementation of <see cref="IHttpRouteConstraint"/> that only matches a specific OData protocol 
    /// version. This constraint won't match incoming requests that contain any of the previous OData version
    /// headers (for OData versions 1.0 to 3.0) regardless of the version in the current version headers.
    /// </summary>
    public partial class ODataVersionConstraint : IHttpRouteConstraint
    {
        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName,
            IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (routeDirection == HttpRouteDirection.UriGeneration)
            {
                return true;
            }

            IDictionary<string, IEnumerable<string>> headers = request.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            ODataVersion? serviceVersion = request.ODataProperties().ODataServiceVersion;
            ODataVersion? maxServiceVersion = request.ODataProperties().ODataMaxServiceVersion;

            return IsVersionMatch(headers, serviceVersion, maxServiceVersion);
        }
    }
}
