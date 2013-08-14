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
    /// Annotates a controller with a route template that applies to any actions within the controller 
    /// that don't already have a route template.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class DefaultRouteAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRouteAttribute" /> class.
        /// </summary>
        public DefaultRouteAttribute()
            : this(String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRouteAttribute" /> class.
        /// </summary>
        /// <param name="routeTemplate">The route template for the controller.</param>
        public DefaultRouteAttribute(string routeTemplate)
        {
            if (routeTemplate == null)
            {
                throw Error.ArgumentNull("routeTemplate");
            }

            RouteTemplate = routeTemplate;
        }

        /// <summary>
        /// Gets the route template describing the URI pattern to match against.
        /// </summary>
        public string RouteTemplate { get; private set; }
    }
}
