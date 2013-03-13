// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The accessor is exposed as an Collection<HttpMethod>.")]
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class AcceptVerbsAttribute : Attribute, IHttpRouteProvider
    {
        private readonly Collection<HttpMethod> _httpMethods;

        public AcceptVerbsAttribute(params string[] methods)
        {
            _httpMethods = methods != null
                       ? new Collection<HttpMethod>(methods.Select(method => HttpMethodHelper.GetHttpMethod(method)).ToArray())
                       : new Collection<HttpMethod>(new HttpMethod[0]);
        }

        internal AcceptVerbsAttribute(params HttpMethod[] methods)
        {
            _httpMethods = new Collection<HttpMethod>(methods);
        }

        public Collection<HttpMethod> HttpMethods
        {
            get
            {
                return _httpMethods;
            }
        }

        public string RouteName { get; set; }

        public string RouteTemplate { get; set; }
    }
}
