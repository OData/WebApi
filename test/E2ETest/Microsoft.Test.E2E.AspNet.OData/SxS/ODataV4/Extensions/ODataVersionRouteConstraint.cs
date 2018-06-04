// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

namespace Microsoft.Test.E2E.AspNet.OData.SxS.ODataV4.Extensions
{
    /// <summary>
    /// Route constraint to allow constraint odata route by query string or headers
    /// For example, you may set query string constraint to v=1 and the route that 
    /// matching it will be considered as a v1 request and corresponding model will
    /// be used to server it
    /// </summary>
    public class ODataVersionRouteConstraint : IHttpRouteConstraint
    {
        public ODataVersionRouteConstraint(object queryConstraints)
        {
            QueryStringConstraints = new HttpRouteValueDictionary(queryConstraints);
        }

        public Dictionary<string, object> QueryStringConstraints { get; set; }

        bool IHttpRouteConstraint.Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            var queries = request.GetQueryNameValuePairs().ToDictionary(p => p.Key, p => p.Value);
            foreach (var key in QueryStringConstraints.Keys)
            {
                if (!queries.ContainsKey(key)
                    || string.Compare(queries[key], QueryStringConstraints[key].ToString(), true, CultureInfo.CurrentCulture) != 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}