// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    /// <summary>
    /// Place on an action to expose it directly via a route. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class RouteAttribute : Attribute, IHttpRouteInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAttribute" /> class.
        /// </summary>
        public RouteAttribute()
        {
            RouteTemplate = String.Empty;
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template describing the URI pattern to match against.</param>
        public RouteAttribute(string routeTemplate)
        {
            if (routeTemplate == null)
            {
                throw Error.ArgumentNull("routeTemplate");
            }
            RouteTemplate = routeTemplate;
        }

        /// <inheritdoc />
        public string RouteName { get; set; }

        /// <inheritdoc />
        public int RouteOrder { get; set; }

        /// <inheritdoc />
        public string RouteTemplate { get; private set; }
    }
}
