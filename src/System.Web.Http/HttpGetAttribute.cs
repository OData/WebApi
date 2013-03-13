// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class HttpGetAttribute : Attribute, IHttpRouteProvider
    {
        private static readonly Collection<HttpMethod> _supportedMethods = new Collection<HttpMethod>(new HttpMethod[] { HttpMethod.Get });

        public HttpGetAttribute()
        {
        }

        public HttpGetAttribute(string routeTemplate)
        {
            RouteTemplate = routeTemplate;
        }

        public Collection<HttpMethod> HttpMethods
        {
            get
            {
                return _supportedMethods;
            }
        }

        public string RouteName { get; set; }

        public string RouteTemplate { get; private set; }
    }
}
