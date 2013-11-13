// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;

#if ASPNETWEBAPI
using TRouteDictionary = System.Collections.Generic.IDictionary<string, object>;
#else
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Represents an attribute route that may contain custom constraints.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public abstract class RouteProviderAttribute : Attribute, IDirectRouteProvider
    {
        private readonly string _template;

        /// <summary>Initializes a new instance of the <see cref="RouteProviderAttribute"/> class.</summary>
        /// <param name="template">The route template.</param>
        protected RouteProviderAttribute(string template)
        {
            _template = template;
        }

        /// <summary>Gets the route template.</summary>
        public string Template
        {
            get { return _template; }
        }

        /// <summary>Gets or sets the route name, if any; otherwise <see langword="null"/>.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the route order.</summary>
        public int Order { get; set; }

        /// <summary>Gets the route constraints, if any; otherwise <see langword="null"/>.</summary>
        public virtual TRouteDictionary Constraints
        {
            get { return null; }
        }

        /// <inheritdoc />
        public RouteEntry CreateRoute(DirectRouteProviderContext context)
        {
            Contract.Assert(context != null);
            DirectRouteBuilder builder = context.CreateBuilder(Template);
            Contract.Assert(builder != null);
            builder.Name = Name;
            builder.Order = Order;

            TRouteDictionary builderConstraints = builder.Constraints;

            if (builderConstraints == null)
            {
                builder.Constraints = Constraints;
            }
            else
            {
                TRouteDictionary constraints = Constraints;

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
