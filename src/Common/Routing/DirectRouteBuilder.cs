// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
#if !ASPNETWEBAPI
using System.Diagnostics.Contracts;
#endif
using System.Linq;

#if ASPNETWEBAPI
using TActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
using TRouteDictionary = System.Collections.Generic.IDictionary<string, object>;
using TRouteDictionaryConcrete = System.Web.Http.Routing.HttpRouteValueDictionary;
using TRouteHandler = System.Net.Http.HttpMessageHandler;
#else
using System.Web.Routing;
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
using TRouteDictionaryConcrete = System.Web.Routing.RouteValueDictionary;
using TRouteHandler = System.Web.Routing.IRouteHandler;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    /// <summary>Represents a builder that creates direct routes to actions (attribute routes).</summary>
    public class DirectRouteBuilder
    {
        private readonly TActionDescriptor[] _actions;

#if !ASPNETWEBAPI
        private readonly bool _targetIsAction;
#endif

        private string _template;

#if ASPNETWEBAPI
        /// <summary>Initializes a new instance of the <see cref="DirectRouteBuilder"/> class.</summary>
        /// <param name="actions">The action descriptors to which to create a route.</param>
        public DirectRouteBuilder(IReadOnlyCollection<TActionDescriptor> actions)
#else
        /// <summary>Initializes a new instance of the <see cref="DirectRouteBuilder"/> class.</summary>
        /// <param name="actions">The action descriptors to which to create a route.</param>
        /// <param name="targetIsAction">
        /// A value indicating whether the route is configured at the action or controller level.
        /// </param>
        public DirectRouteBuilder(IReadOnlyCollection<TActionDescriptor> actions, bool targetIsAction)
#endif
        {
            if (actions == null)
            {
                throw new ArgumentNullException("actions");
            }

            _actions = actions.ToArray();

#if !ASPNETWEBAPI
            _targetIsAction = targetIsAction;
#endif
        }

        /// <summary>Gets or sets the route name, if any; otherwise <see langword="null"/>.</summary>
        public string Name { get; set; }

        /// <summary>Gets or sets the route template.</summary>
        /// <remarks>This value is the remaining route template after resolving any inline constraints.</remarks>
        public string Template
        {
            get
            {
                return _template;
            }
            set
            {
#if ASPNETWEBAPI
                ParsedRoute = null;
#endif
                _template = value;
            }
        }

        /// <summary>Gets or sets the route defaults.</summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Null and empty values are legitimate, separate options when constructing a route.")]
        public TRouteDictionary Defaults { get; set; }

        /// <summary>Gets or sets the route constraints.</summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Null and empty values are legitimate, separate options when constructing a route.")]
        public TRouteDictionary Constraints { get; set; }

        /// <summary>Gets or sets the route data tokens.</summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
            Justification = "Null and empty values are legitimate, separate options when constructing a route.")]
        public TRouteDictionary DataTokens { get; set; }

        /// <summary>Gets or sets the route handler.</summary>
        public TRouteHandler Handler { get; set; }

#if ASPNETWEBAPI
        internal HttpParsedRoute ParsedRoute { get; set; }
#endif

        /// <summary>Gets or sets the route order.</summary>
        /// <remarks>
        /// The route order disambiguates multiple matching routes and overrides precedence.
        /// The intended use of order is for an explicitly provided precedence override value.
        /// </remarks>
        public int Order { get; set; }

        /// <summary>Gets or sets the route precedence.</summary>
        /// <remarks>
        /// The route order disambiguates multiple matching routes with the same order.
        /// The intended use of precedence is for deafult, automatically computed disambiguation based on inline
        /// constraint types.
        /// </remarks>
        public decimal Precedence { get; set; }

        /// <summary>Gets the action descriptors to which to create a route.</summary>
        public IReadOnlyCollection<TActionDescriptor> Actions
        {
            get { return _actions; }
        }

#if !ASPNETWEBAPI
        /// <summary>
        /// Gets a value indicating whether the route is configured at the action or controller level.
        /// </summary>
        /// <remarks>
        /// <see langword="true"/> when the route is configured at the action level; otherwise <see langword="false"/>
        /// (if the route is configured at the controller level).
        /// </remarks>
        public bool TargetIsAction
        {
            get { return _targetIsAction; }
        }
#endif

        /// <summary>Creates a route entry based on the current property values.</summary>
        /// <returns>The route entry created.</returns>
        public virtual RouteEntry Build()
        {
            TRouteDictionaryConcrete defaults;
#if ASPNETWEBAPI
            defaults = Copy(Defaults);
#else
            defaults = Copy(Defaults) ?? new RouteValueDictionary();
#endif
            TRouteDictionaryConcrete constraints = Copy(Constraints);
            TRouteDictionaryConcrete dataTokens = Copy(DataTokens) ?? new TRouteDictionaryConcrete();

            dataTokens[RouteDataTokenKeys.Actions] = _actions;

            int order = Order;

            if (order != default(int))
            {
                dataTokens[RouteDataTokenKeys.Order] = order;
            }

            decimal precedence = Precedence;

            if (precedence != default(decimal))
            {
                dataTokens[RouteDataTokenKeys.Precedence] = precedence;
            }

#if ASPNETWEBAPI
            IHttpRoute route = new HttpRoute(Template, defaults, constraints, dataTokens, Handler, ParsedRoute);
#else
            ControllerDescriptor controllerDescriptor = GetControllerDescriptor();

            if (controllerDescriptor != null)
            {
                defaults["controller"] = controllerDescriptor.ControllerName;
            }

            if (TargetIsAction && _actions.Length == 1)
            {
                ActionDescriptor actionDescriptor = _actions[0];
                defaults["action"] = actionDescriptor.ActionName;
            }

            RouteAreaAttribute area = controllerDescriptor.GetAreaFrom();
            string areaName = controllerDescriptor.GetAreaName(area);

            if (areaName != null)
            {
                dataTokens.Add(RouteDataTokenKeys.Area, areaName);
                dataTokens.Add(RouteDataTokenKeys.UseNamespaceFallback, value: false);

                Type controllerType = controllerDescriptor.ControllerType;
                if (controllerType != null)
                {
                    dataTokens.Add(RouteDataTokenKeys.Namespaces, new[] { controllerType.Namespace });
                }
            }

            Route route = new Route(Template, defaults, constraints, dataTokens, Handler ?? new MvcRouteHandler());
#endif

            return new RouteEntry(Name, route);
        }

        private static TRouteDictionaryConcrete Copy(TRouteDictionary routeDictionary)
        {
            if (routeDictionary == null)
            {
                return null;
            }

            return new TRouteDictionaryConcrete(routeDictionary);
        }

#if !ASPNETWEBAPI
        private ControllerDescriptor GetControllerDescriptor()
        {
            ControllerDescriptor controller = null;

            foreach (ActionDescriptor action in _actions)
            {
                if (controller == null)
                {
                    controller = action.ControllerDescriptor;
                }
                else if (action.ControllerDescriptor != controller)
                {
                    controller = null;
                    break;
                }
            }

            return controller;
        }
#endif
    }
}
