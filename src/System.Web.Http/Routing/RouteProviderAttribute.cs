// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace System.Web.Http.Routing
{
    /// <summary>Represents an attribute route that may contain custom constraints.</summary>
    public abstract class RouteProviderAttribute : Attribute, IDirectRouteProvider
    {
        private readonly string _template;

        /// <summary>Initializes a new instance of the <see cref="RouteProviderAttribute"/> class.</summary>
        /// <param name="template">The route template.</param>
        protected RouteProviderAttribute(string template)
        {
            _template = template;
        }

        /// <summary>When implemented in a derived class, gets the route template.</summary>
        public string Template
        {
            get { return _template; }
        }

        /// <summary>Gets or sets the route name, if any; otherwise <see langword="null"/>.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the route order.</summary>
        public int Order { get; set; }

        /// <summary>Gets the route constraints, if any; otherwise <see langword="null"/>.</summary>
        public virtual HttpRouteValueDictionary Constraints
        {
            get { return null; }
        }

        internal HttpRouteEntry CreateRoute(DirectRouteProviderContext context)
        {
            return ((IDirectRouteProvider)this).CreateRoute(context);
        }

        HttpRouteEntry IDirectRouteProvider.CreateRoute(DirectRouteProviderContext context)
        {
            Contract.Assert(context != null);
            DirectRouteBuilder builder = context.CreateBuilder(Template);
            Contract.Assert(builder != null);
            builder.Name = Name;
            builder.Order = Order;

            HttpRouteValueDictionary builderConstraints = builder.Constraints;

            if (builderConstraints == null)
            {
                builder.Constraints = Constraints;
            }
            else
            {
                HttpRouteValueDictionary constraints = Constraints;

                if (constraints != null)
                {
                    foreach (KeyValuePair<string, object> constraint in constraints)
                    {
                        builderConstraints.Add(constraint.Key, constraint.Value);
                    }
                }
            }

            return builder.Build();
        }
    }
}
