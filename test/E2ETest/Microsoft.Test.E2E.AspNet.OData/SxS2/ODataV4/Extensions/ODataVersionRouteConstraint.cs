//-----------------------------------------------------------------------------
// <copyright file="ODataVersionRouteConstraint.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

namespace Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV4.Extensions
{
    /// <summary>
    /// Route constraint to allow constraint odata route by query string or headers
    /// For example, you may set query string constraint to v=1 and the route that 
    /// matching it will be considered as a v1 request and corresponding model will
    /// be used to server it
    /// </summary>
    public class ODataVersionRouteConstraint : IHttpRouteConstraint
    {
        public ODataVersionRouteConstraint(List<string> invalidVersionSignature)
        {
            HeaderStringConstraints = invalidVersionSignature;
        }

        public List<string> HeaderStringConstraints { get; set; }

        bool IHttpRouteConstraint.Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (HeaderStringConstraints.Count > 0)
            {
                var queries = request.Headers.ToDictionary(h => h.Key, h => h.Value);
                foreach (var key in HeaderStringConstraints)
                {
                    if (queries.ContainsKey(key))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
