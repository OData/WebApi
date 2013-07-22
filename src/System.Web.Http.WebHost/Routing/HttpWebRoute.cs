// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace System.Web.Http.WebHost.Routing
{
    /// <summary>
    /// Mimics the System.Web.Routing.Route class to work better for Web API scenarios. The only
    /// difference between the base class and this class is that this one will match only when
    /// a special "httproute" key is specified when generating URLs. There is no special behavior
    /// for incoming URLs.
    /// </summary>
    internal class HttpWebRoute : Route
    {
        /// <summary>
        /// Key used to signify that a route URL generation request should include HTTP routes (e.g. Web API).
        /// If this key is not specified then no HTTP routes will match.
        /// </summary>
        internal const string HttpRouteKey = "httproute";

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#", Justification = "Matches the base class's parameter names.")]
        public HttpWebRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler, IHttpRoute httpRoute)
            : base(url, defaults, constraints, dataTokens, routeHandler)
        {
            if (httpRoute == null)
            {
                throw Error.ArgumentNull("httpRoute");
            }

            HttpRoute = httpRoute;
        }

        /// <summary>
        /// Gets the <see cref="IHttpRoute"/> associated with this <see cref="HttpWebRoute"/>.
        /// </summary>
        public IHttpRoute HttpRoute { get; private set; }

        protected override bool ProcessConstraint(HttpContextBase httpContext, object constraint, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            IHttpRouteConstraint httpRouteConstraint = constraint as IHttpRouteConstraint;
            if (httpRouteConstraint != null)
            {
                HttpRequestMessage request = httpContext.GetOrCreateHttpRequestMessage();
                return httpRouteConstraint.Match(request, HttpRoute, parameterName, values, ConvertRouteDirection(routeDirection));
            }

            return base.ProcessConstraint(httpContext, constraint, parameterName, values, routeDirection);
        }

        [SuppressMessage("Microsoft.Web.FxCop", "MW1201:DoNotCallProblematicMethodsOnTask",
            Justification = "Usage is fine here as it handles error case and that task finishes synchronously in the default code path anyways.")]
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            try
            {
                if (HttpRoute is HostedHttpRoute)
                {
                    return base.GetRouteData(httpContext);
                }
                else
                {
                    // if user passed us a custom IHttpRoute, then we should invoke their function instead of the base
                    HttpRequestMessage request = httpContext.GetOrCreateHttpRequestMessage();
                    IHttpRouteData data = HttpRoute.GetRouteData(httpContext.Request.ApplicationPath, request);
                    return data == null ? null : data.ToRouteData();
                }
            }
            catch (HttpResponseException e)
            {
                // Task.Wait is fine here as ConvertResponse calls into MediaTypeFormatter.WriteToStreamAsync which happens
                // synchronously in the default case (our default formatters are synchronous).
                HttpControllerHandler.ConvertResponse(httpContext, e.Response, httpContext.GetOrCreateHttpRequestMessage()).Wait();
                httpContext.Response.End();
                return null;
            }
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            // Only perform URL generation if the "httproute" key was specified. This allows these
            // routes to be ignored when a regular MVC app tries to generate URLs. Without this special
            // key an HTTP route used for Web API would normally take over almost all the routes in a
            // typical app.
            if (!values.ContainsKey(HttpRouteKey))
            {
                return null;
            }
            // Remove the value from the collection so that it doesn't affect the generated URL
            RouteValueDictionary newValues = GetRouteDictionaryWithoutHttpRouteKey(values);

            if (HttpRoute is HostedHttpRoute)
            {
                return base.GetVirtualPath(requestContext, newValues);
            }
            else
            {
                // if user passed us a custom IHttpRoute, then we should invoke their function instead of the base
                HttpRequestMessage request = requestContext.HttpContext.GetOrCreateHttpRequestMessage();
                IHttpVirtualPathData virtualPathData = HttpRoute.GetVirtualPath(request, values);

                return virtualPathData == null ? null : new VirtualPathData(this, virtualPathData.VirtualPath);
            }
        }

        private static RouteValueDictionary GetRouteDictionaryWithoutHttpRouteKey(IDictionary<string, object> routeValues)
        {
            var newRouteValues = new RouteValueDictionary();
            foreach (var routeValue in routeValues)
            {
                if (!String.Equals(routeValue.Key, HttpRouteKey, StringComparison.OrdinalIgnoreCase))
                {
                    newRouteValues.Add(routeValue.Key, routeValue.Value);
                }
            }
            return newRouteValues;
        }

        private static HttpRouteDirection ConvertRouteDirection(RouteDirection routeDirection)
        {
            if (routeDirection == RouteDirection.IncomingRequest)
            {
                return HttpRouteDirection.UriResolution;
            }

            if (routeDirection == RouteDirection.UrlGeneration)
            {
                return HttpRouteDirection.UriGeneration;
            }

            throw Error.InvalidEnumArgument("routeDirection", (int)routeDirection, typeof(RouteDirection));
        }
    }
}
