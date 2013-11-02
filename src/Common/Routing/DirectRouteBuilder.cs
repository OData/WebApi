// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
#if !ASPNETWEBAPI
using System.Diagnostics.Contracts;
#endif
using System.Linq;

#if ASPNETWEBAPI
using TActionDescriptor = System.Web.Http.Controllers.HttpActionDescriptor;
using TRouteDictionary = System.Web.Http.Routing.HttpRouteValueDictionary;
using TRouteHandler = System.Net.Http.HttpMessageHandler;
#else
using System.Web.Routing;
using TActionDescriptor = System.Web.Mvc.ActionDescriptor;
using TRouteDictionary = System.Web.Routing.RouteValueDictionary;
using TRouteHandler = System.Web.Routing.IRouteHandler;
#endif

#if ASPNETWEBAPI
namespace System.Web.Http.Routing
#else
namespace System.Web.Mvc.Routing
#endif
{
    internal class DirectRouteBuilder
    {
        private readonly TActionDescriptor[] _actions;

#if !ASPNETWEBAPI
        private readonly bool _targetIsAction;
#endif

        private string _template;

#if ASPNETWEBAPI
        public DirectRouteBuilder(IEnumerable<TActionDescriptor> actions)
#else
        public DirectRouteBuilder(IEnumerable<TActionDescriptor> actions, bool targetIsAction)
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

        public string Name { get; set; }

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

        public TRouteDictionary Defaults { get; set; }

        public TRouteDictionary Constraints { get; set; }

        public TRouteDictionary DataTokens { get; set; }

        public TRouteHandler Handler { get; set; }

#if ASPNETWEBAPI
        internal HttpParsedRoute ParsedRoute { get; set; }
#endif

        public int Order { get; set; }

        public decimal Precedence { get; set; }

        public IEnumerable<TActionDescriptor> Actions
        {
            get { return _actions; }
        }

#if !ASPNETWEBAPI
        public bool TargetIsAction
        {
            get { return _targetIsAction; }
        }
#endif

        public virtual RouteEntry Build()
        {
            TRouteDictionary dataTokens = DataTokens;

            if (dataTokens == null)
            {
                dataTokens = new TRouteDictionary();
            }

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
            IHttpRoute route = new HttpRoute(Template, Defaults, Constraints, dataTokens, Handler, ParsedRoute);
#else
            TRouteDictionary defaults = Defaults;

            if (defaults == null)
            {
                defaults = new RouteValueDictionary();
            }

            ControllerDescriptor controllerDescriptor = GetControllerDescriptor();
            defaults["controller"] = controllerDescriptor.ControllerName;

            if (TargetIsAction)
            {
                ActionDescriptor actionDescriptor = _actions.Single();
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

            Route route = new Route(Template, defaults, Constraints, dataTokens, Handler ?? new MvcRouteHandler());
#endif

            return new RouteEntry(Name, route);
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

            Contract.Assert(controller != null);
            return controller;
        }
#endif
    }
}
