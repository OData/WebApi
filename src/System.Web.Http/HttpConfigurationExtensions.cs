// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ModelBinding;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Routing;

namespace System.Web.Http
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class HttpConfigurationExtensions
    {
        /// <summary>
        /// Register that the given parameter type on an Action is to be bound using the model binder.
        /// </summary>
        /// <param name="configuration">configuration to be updated.</param>
        /// <param name="type">parameter type that binder is applied to</param>
        /// <param name="binder">a model binder</param>
        public static void BindParameter(this HttpConfiguration configuration, Type type, IModelBinder binder)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            }

            // Add a provider so that we can use this type recursively
            // Be sure to insert at position 0 to preempt any eager binders (eg, MutableObjectBinder) that 
            // may eagerly claim all types.
            configuration.Services.Insert(typeof(ModelBinderProvider), 0, new SimpleModelBinderProvider(type, binder));

            // Add the binder to the list of rules. 
            // This ensures that the parameter binding will actually use model binding instead of Formatters.            
            // Without this, the parameter binding system may see the parameter type is complex and choose
            // to use formatters instead, in which case it would ignore the registered model binders. 
            configuration.ParameterBindingRules.Insert(0, type, param => param.BindWithModelBinding(binder));
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        public static void MapHttpAttributeRoutes(this HttpConfiguration configuration)
        {
            configuration.MapHttpAttributeRoutes(new HttpRouteBuilder());
        }

        /// <summary>
        /// Maps the attribute-defined routes for the application.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="routeBuilder">The <see cref="HttpRouteBuilder"/> to use for generating attribute routes.</param>
        public static void MapHttpAttributeRoutes(this HttpConfiguration configuration, HttpRouteBuilder routeBuilder)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (routeBuilder == null)
            {
                throw Error.ArgumentNull("routeBuilder");
            }

            IHttpControllerSelector controllerSelector = configuration.Services.GetHttpControllerSelector();
            IHttpActionSelector actionSelector = configuration.Services.GetActionSelector();
            foreach (HttpControllerDescriptor controllerDescriptor in controllerSelector.GetControllerMapping().Values)
            {
                foreach (IGrouping<string, HttpActionDescriptor> actionGrouping in actionSelector.GetActionMapping(controllerDescriptor))
                {
                    MapHttpAttributeRoutes(configuration.Routes, controllerDescriptor, actionGrouping, routeBuilder);
                }
            }
        }

        private static void MapHttpAttributeRoutes(HttpRouteCollection routes, HttpControllerDescriptor controllerDescriptor,
            IGrouping<string, HttpActionDescriptor> actionGrouping, HttpRouteBuilder routeBuilder)
        {
            List<IHttpRoute> namelessAttributeRoutes = new List<IHttpRoute>();
            string controllerName = controllerDescriptor.ControllerName;
            string actionName = actionGrouping.Key;

            foreach (HttpActionDescriptor actionDescriptor in actionGrouping)
            {
                foreach (IHttpRouteInfoProvider routeProvider in actionDescriptor.GetCustomAttributes<IHttpRouteInfoProvider>(inherit: false))
                {
                    if (routeProvider.RouteTemplate != null)
                    {
                        IHttpRoute route = routeBuilder.BuildHttpRoute(routeProvider, controllerName, actionName);
                        if (routeProvider.RouteName == null)
                        {
                            namelessAttributeRoutes.Add(route);
                        }
                        else
                        {
                            routes.Add(routeProvider.RouteName, route);
                        }
                    }
                }
            }

            // Only use a route suffix to disambiguate between multiple routes without a specified route name
            if (namelessAttributeRoutes.Count == 1)
            {
                routes.Add(controllerName + "." + actionName, namelessAttributeRoutes[0]);
            }
            else if (namelessAttributeRoutes.Count > 1)
            {
                int routeSuffix = 1;
                foreach (IHttpRoute namelessAttributeRoute in namelessAttributeRoutes)
                {
                    routes.Add(controllerName + "." + actionName + routeSuffix, namelessAttributeRoute);
                    routeSuffix++;
                }
            }
        }
    }
}
