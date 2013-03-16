// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;

namespace System.Web.Http.Routing.Constraints
{
    public abstract class HttpRouteConstraintTestBase
    {
        protected bool TestValue(IHttpRouteConstraint constraint, object value)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
            HttpRoute httpRoute = new HttpRoute();
            const string parameterName = "fake";
            HttpRouteValueDictionary values = new HttpRouteValueDictionary { { parameterName, value } };
            const HttpRouteDirection httpRouteDirection = HttpRouteDirection.UriResolution;

            return constraint.Match(httpRequestMessage, httpRoute, parameterName, values, httpRouteDirection);
        }        
    }
}