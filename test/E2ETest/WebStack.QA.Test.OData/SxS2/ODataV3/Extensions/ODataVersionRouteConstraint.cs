﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Routing;

namespace WebStack.QA.Test.OData.SxS2.ODataV3.Extensions
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

        public bool Match(
            HttpRequestMessage request,
            IHttpRoute route,
            string parameterName,
            IDictionary<string, object> values,
            HttpRouteDirection routeDirection)
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