// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Specifies what HTTP methods an action supports.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The accessor is exposed as an Collection<HttpMethod>.")]
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class AcceptVerbsAttribute : Attribute, IActionHttpMethodProvider, IHttpRouteInfoProvider
    {
        private readonly Collection<HttpMethod> _httpMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="AcceptVerbsAttribute" /> class.
        /// </summary>
        /// <param name="methods">The HTTP methods the action supports.</param>
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

        /// <summary>
        /// Gets the HTTP methods the action supports.
        /// </summary>
        public Collection<HttpMethod> HttpMethods
        {
            get
            {
                return _httpMethods;
            }
        }

        /// <inheritdoc />
        public string RouteName { get; set; }

        /// <inheritdoc />
        public int RouteOrder { get; set; }

        /// <inheritdoc />
        public string RouteTemplate { get; set; }

        IEnumerable<HttpMethod> IHttpRouteInfoProvider.HttpMethods
        {
            get
            {
                return _httpMethods;
            }
        }
    }
}
