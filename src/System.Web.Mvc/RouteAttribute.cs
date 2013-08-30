// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc.Routing;

namespace System.Web.Mvc
{
    /// <summary>
    /// Represents a route for an action method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class RouteAttribute : Attribute, IRouteInfoProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAttribute" /> class.
        /// </summary>
        public RouteAttribute() : this(String.Empty)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteAttribute" /> class.
        /// </summary>
        /// <param name="template">The route template describing the URI pattern to match against.</param>
        public RouteAttribute(string template)
        {
            if (template == null)
            {
                throw Error.ArgumentNull("template");
            }
            Template = template;
        }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public int Order { get; set; }

        /// <inheritdoc />
        public string Template { get; private set; }
    }
}