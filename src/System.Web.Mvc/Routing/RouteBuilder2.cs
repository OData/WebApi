// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Routing;

namespace System.Web.Mvc.Routing
{
    /// <summary>
    /// Builds <see cref="Route"/> instances based on route information.
    /// </summary>
    internal class RouteBuilder2
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RouteBuilder" /> class using the default inline constraint resolver.
        /// </summary>
        public RouteBuilder2()
            : this(new DefaultInlineConstraintResolver())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RouteBuilder" /> class.
        /// </summary>
        /// <param name="constraintResolver">The <see cref="IInlineConstraintResolver"/> to use for resolving inline constraints.</param>
        public RouteBuilder2(IInlineConstraintResolver constraintResolver)
        {
            if (constraintResolver == null)
            {
                throw Error.ArgumentNull("constraintResolver");
            }

            ConstraintResolver = constraintResolver;
        }

        public IInlineConstraintResolver ConstraintResolver { get; private set; }

        /// <summary>
        /// Builds an <see cref="Route"/> for a particular action.
        /// </summary>
        /// <param name="routeTemplate">The tokenized route template for the route.</param>
        /// <param name="routeInfoProvider">The info provider for the route.</param>
        /// <param name="controllerDescriptor">The controller the route attribute has been applied on.</param>
        /// <param name="actionDescriptor">The action reachable by this route.</param>
        /// <returns>The generated <see cref="Route"/>.</returns>
        public Route BuildDirectRoute(
            string routeTemplate,
            IRouteInfoProvider routeInfoProvider,
            ControllerDescriptor controllerDescriptor,
            ActionDescriptor actionDescriptor)
        {
            return BuildDirectRoute(routeTemplate, routeInfoProvider, controllerDescriptor, new ActionDescriptor[] { actionDescriptor }, isActionDirectRoute: true);
        }

        /// <summary>
        /// Builds an <see cref="Route"/> for a particular controller.
        /// </summary>
        /// <param name="routeTemplate">The tokenized route template for the route.</param>
        /// <param name="routeInfoProvider">The info provider for the route.</param>
        /// <param name="controllerDescriptor">The controller the route attribute has been applied on.</param>
        /// <param name="actionDescriptors">The actions reachable by this route.</param>
        /// <returns>The generated <see cref="Route"/>.</returns>
        public Route BuildDirectRoute(
            string routeTemplate, 
            IRouteInfoProvider routeInfoProvider, 
            ControllerDescriptor controllerDescriptor, 
            IEnumerable<ActionDescriptor> actionDescriptors)
        {
            return BuildDirectRoute(routeTemplate, routeInfoProvider, controllerDescriptor, actionDescriptors, isActionDirectRoute: false);
        }

        private Route BuildDirectRoute(
            string routeTemplate,
            IRouteInfoProvider routeInfoProvider,
            ControllerDescriptor controllerDescriptor,
            IEnumerable<ActionDescriptor> actionDescriptors,
            bool isActionDirectRoute)
        {
            if (routeTemplate == null)
            {
                throw Error.ArgumentNull("routeTemplate");
            }

            if (routeInfoProvider == null)
            {
                throw Error.ArgumentNull("routeInfoProvider");
            }

            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            if (actionDescriptors == null || !actionDescriptors.Any())
            {
                throw Error.ParameterCannotBeNullOrEmpty("actionDescriptors");
            }

            string controllerName = controllerDescriptor.ControllerName;

            RouteAreaAttribute area = controllerDescriptor.GetAreaFrom();
            string areaName = controllerDescriptor.GetAreaName(area);

            RouteValueDictionary defaults = new RouteValueDictionary
            {
                { "controller", controllerName }
            };

            if (isActionDirectRoute)
            {
                ActionDescriptor actionDescriptor = actionDescriptors.Single();
                defaults.Add("action", actionDescriptor.ActionName);
            }

            RouteValueDictionary constraints = new RouteValueDictionary();

            string detokenizedRouteTemplate = InlineRouteTemplateParser.ParseRouteTemplate(routeTemplate, defaults, constraints, ConstraintResolver);
            ParsedRoute parsedRoute = RouteParser.Parse(detokenizedRouteTemplate);

            RouteValueDictionary dataTokens = new RouteValueDictionary();
            dataTokens[RouteDataTokenKeys.DirectRoutePrecedence] = RouteEntry.GetPrecedence(parsedRoute, constraints);
            dataTokens[RouteDataTokenKeys.DirectRouteController] = controllerDescriptor;
            dataTokens[RouteDataTokenKeys.DirectRouteActions] = actionDescriptors;

            int order = 0;
            IOrderedRouteInfoProvider orderedAttribute = routeInfoProvider as IOrderedRouteInfoProvider;
            if (orderedAttribute != null)
            {
                order = orderedAttribute.Order;
            }

            dataTokens[RouteDataTokenKeys.DirectRouteOrder] = order;

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

            Route route = new Route(detokenizedRouteTemplate, new MvcRouteHandler())
            {
                Defaults = defaults,
                Constraints = constraints,
                DataTokens = dataTokens
            };

            return route;
        }
    }
}