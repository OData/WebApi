//-----------------------------------------------------------------------------
// <copyright file="AttributeRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Represents a routing convention that looks for <see cref="ODataRouteAttribute"/>s to match an <see cref="ODataPath"/>
    /// to a controller and an action.
    /// </summary>
    public partial class AttributeRoutingConvention : IODataRoutingConvention
    {
        private static readonly DefaultODataPathHandler _defaultPathHandler = new DefaultODataPathHandler();

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The <see cref="System.Web.Http.HttpConfiguration"/> to use for figuring out all the controllers to
        /// look for a match.</param>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public AttributeRoutingConvention(string routeName, HttpConfiguration configuration)
            : this(routeName, configuration, _defaultPathHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use for figuring out all the controllers to
        /// look for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public AttributeRoutingConvention(string routeName, HttpConfiguration configuration,
            IODataPathTemplateHandler pathTemplateHandler)
            : this(routeName)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (pathTemplateHandler == null)
            {
                throw Error.ArgumentNull("pathTemplateHandler");
            }

            ODataPathTemplateHandler = pathTemplateHandler;

            // if settings is not on local, use the global configuration settings.
            IODataPathHandler pathHandler = pathTemplateHandler as IODataPathHandler;
            if (pathHandler != null && pathHandler.UrlKeyDelimiter == null)
            {
                ODataUrlKeyDelimiter urlKeyDelimiter = configuration.GetUrlKeyDelimiter();
                pathHandler.UrlKeyDelimiter = urlKeyDelimiter;
            }

            Action<HttpConfiguration> oldInitializer = configuration.Initializer;
            bool initialized = false;
            configuration.Initializer = (config) =>
            {
                if (!initialized)
                {
                    initialized = true;
                    oldInitializer(config);
                    IHttpControllerSelector controllerSelector = config.Services.GetHttpControllerSelector();
                    _attributeMappings = BuildAttributeMappings(controllerSelector.GetControllerMapping().Values);
                }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        /// <remarks>This signature uses types that are AspNet-specific and is only used for unit tests.</remarks>
        public AttributeRoutingConvention(string routeName,
            IEnumerable<HttpControllerDescriptor> controllers)
            : this(routeName, controllers, _defaultPathHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        /// <remarks>This signature uses types that are AspNet-specific and is only used for unit tests.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "See note on <see cref=\"ShouldMapController()\"> method.")]
        public AttributeRoutingConvention(string routeName,
            IEnumerable<HttpControllerDescriptor> controllers,
            IODataPathTemplateHandler pathTemplateHandler)
            : this(routeName)
        {
            if (controllers == null)
            {
                throw Error.ArgumentNull("controllers");
            }

            if (pathTemplateHandler == null)
            {
                throw Error.ArgumentNull("pathTemplateHandler");
            }

            ODataPathTemplateHandler = pathTemplateHandler;

            _attributeMappings = BuildAttributeMappings(controllers);
        }

        /// <summary>
        /// Gets the attribute mappings.
        /// </summary>
        internal IDictionary<ODataPathTemplate, IWebApiActionDescriptor> AttributeMappings
        {
            get
            {
                if (_attributeMappings == null)
                {
                    // Will throw an InvalidOperationException if this class is constructed with an HttpConfiguration
                    // but EnsureInitialized() hasn't been called yet.
                    throw Error.InvalidOperation(SRResources.Object_NotYetInitialized);
                }

                return _attributeMappings;
            }
        }

        /// <summary>
        /// Specifies whether OData route attributes on this controller should be mapped.
        /// This method will execute before the derived type's instance constructor executes. Derived types must
        /// be aware of this and should plan accordingly. For example, the logic in ShouldMapController() should be simple
        /// enough so as not to depend on the "this" pointer referencing a fully constructed object.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns><c>true</c> if this controller should be included in the map; <c>false</c> otherwise.</returns>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public virtual bool ShouldMapController(HttpControllerDescriptor controller)
        {
            return true;
        }

        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            SelectControllerResult controllerResult = SelectControllerImpl(
                odataPath,
                new WebApiRequestMessage(request),
                this.AttributeMappings);

            if (controllerResult != null)
            {
                request.Properties["AttributeRouteData"] = controllerResult.Values;
            }

            return controllerResult != null ? controllerResult.ControllerName : null;
        }

        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext, ILookup<string, HttpActionDescriptor> actionMap)
        {
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (controllerContext == null)
            {
                throw Error.ArgumentNull("controllerContext");
            }

            if (actionMap == null)
            {
                throw Error.ArgumentNull("actionMap");
            }

            object value = null;
            controllerContext.Request.Properties.TryGetValue("AttributeRouteData", out value);

            SelectControllerResult controllerResult = new SelectControllerResult(
                controllerContext.ControllerDescriptor.ControllerName,
                value as IDictionary<string, object>);

            return SelectActionImpl(new WebApiControllerContext(controllerContext, controllerResult));
        }

        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        private IDictionary<ODataPathTemplate, IWebApiActionDescriptor> BuildAttributeMappings(IEnumerable<HttpControllerDescriptor> controllers)
        {
            Dictionary<ODataPathTemplate, IWebApiActionDescriptor> attributeMappings =
                new Dictionary<ODataPathTemplate, IWebApiActionDescriptor>();

            foreach (HttpControllerDescriptor controller in controllers)
            {
                if (IsODataController(controller) && ShouldMapController(controller))
                {
                    IHttpActionSelector actionSelector = controller.Configuration.Services.GetActionSelector();
                    ILookup<string, HttpActionDescriptor> actionMapping = actionSelector.GetActionMapping(controller);
                    HttpActionDescriptor[] actions = actionMapping.SelectMany(a => a).ToArray();

                    foreach (string prefix in GetODataRoutePrefixes(controller))
                    {
                        foreach (HttpActionDescriptor action in actions)
                        {
                            IEnumerable<ODataPathTemplate> pathTemplates = GetODataPathTemplates(prefix, action);
                            foreach (ODataPathTemplate pathTemplate in pathTemplates)
                            {
                                attributeMappings.Add(pathTemplate, new WebApiActionDescriptor(action));
                            }
                        }
                    }
                }
            }

            return attributeMappings;
        }

        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        private static bool IsODataController(HttpControllerDescriptor controller)
        {
            return typeof(ODataController).IsAssignableFrom(controller.ControllerType);
        }

        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        private static IEnumerable<string> GetODataRoutePrefixes(HttpControllerDescriptor controllerDescriptor)
        {
            Contract.Assert(controllerDescriptor != null);

            var prefixAttributes = controllerDescriptor.GetCustomAttributes<ODataRoutePrefixAttribute>(inherit: false);

            return GetODataRoutePrefixes(prefixAttributes, controllerDescriptor.ControllerType.FullName);
        }

        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        private IEnumerable<ODataPathTemplate> GetODataPathTemplates(string prefix, HttpActionDescriptor action)
        {
            Contract.Assert(action != null);

            IEnumerable<ODataRouteAttribute> routeAttributes =
                action.GetCustomAttributes<ODataRouteAttribute>(inherit: false);

            IServiceProvider requestContainer = action.Configuration.GetODataRootContainer(_routeName);

            string controllerName = action.ControllerDescriptor.ControllerName;
            string actionName = action.ActionName;

            return
                routeAttributes
                    .Where(route => String.IsNullOrEmpty(route.RouteName) || route.RouteName == _routeName)
                    .Select(route => GetODataPathTemplate(prefix, route.PathTemplate, requestContainer, controllerName, actionName))
                    .Where(template => template != null);
        }
    }
}
