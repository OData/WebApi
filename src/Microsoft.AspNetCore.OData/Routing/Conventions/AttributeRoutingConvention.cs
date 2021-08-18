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
using System.Reflection;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Routing.Template;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Represents a routing convention that looks for <see cref="ODataRouteAttribute"/>s to match an <see cref="ODataPath"/>
    /// to a controller and an action.
    /// </summary>
    public partial class AttributeRoutingConvention : IODataRoutingConvention
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to use for figuring out all the controllers to
        /// look for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        /// <remarks>
        /// While this function does not use types that are AspNetCore-specific,
        /// the functionality is due to the way assembly resolution is done in AspNet vs AspnetCore.
        /// </remarks>
        public AttributeRoutingConvention(string routeName, IServiceProvider serviceProvider,
            IODataPathTemplateHandler pathTemplateHandler = null)
            : this(routeName)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull("serviceProvider");
            }

            _serviceProvider = serviceProvider;

            if (pathTemplateHandler != null)
            {
                ODataPathTemplateHandler = pathTemplateHandler;
            }
            else
            {
                IPerRouteContainer perRouteContainer = _serviceProvider.GetRequiredService<IPerRouteContainer>();
                if (perRouteContainer == null)
                {
                    throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
                }

                IServiceProvider rootContainer = perRouteContainer.GetODataRootContainer(routeName);
                ODataPathTemplateHandler = rootContainer.GetRequiredService<IODataPathTemplateHandler>();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        /// <remarks>This signature uses types that are AspNetCore-specific and is only used for unit tests.</remarks>
        internal AttributeRoutingConvention(string routeName,
            IEnumerable<ControllerActionDescriptor> controllers)
            : this(routeName, controllers, (IODataPathTemplateHandler)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeRoutingConvention"/> class.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="controllers">The collection of controllers to search for a match.</param>
        /// <param name="pathTemplateHandler">The path template handler to be used for parsing the path templates.</param>
        /// <remarks>This signature uses types that are AspNetCore-specific and is only used for unit tests.</remarks>
        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors",
            Justification = "See note on <see cref=\"ShouldMapController()\"> method.")]
        internal AttributeRoutingConvention(string routeName,
            IEnumerable<ControllerActionDescriptor> controllers,
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

            // Look for a service provider on the ControllerActionDescriptor, which the unit test will setup
            // so the BuildAttributeMappings can find the perRouteContainer.
            _serviceProvider = controllers.FirstOrDefault()?.Properties["serviceProvider"] as IServiceProvider;

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
                    // Get the IActionDescriptorCollectionProvider from the global services provider.
                    IActionDescriptorCollectionProvider actionDescriptorCollectionProvider =
                            _serviceProvider.GetRequiredService<IActionDescriptorCollectionProvider>();

                    IEnumerable<ControllerActionDescriptor> actionDescriptors =
                        actionDescriptorCollectionProvider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>();

                    _attributeMappings = BuildAttributeMappings(actionDescriptors);
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
        /// <param name="controllerAction">The controller action.</param>
        /// <returns><c>true</c> if this controller should be included in the map; <c>false</c> otherwise.</returns>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public virtual bool ShouldMapController(ControllerActionDescriptor controllerAction)
        {
            return true;
        }

        /// <inheritdoc/>
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public IEnumerable<ControllerActionDescriptor> SelectAction(RouteContext routeContext)
        {
            // Get a IActionDescriptorCollectionProvider from the global service provider.
            IActionDescriptorCollectionProvider actionCollectionProvider =
                routeContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();
            Contract.Assert(actionCollectionProvider != null);

            ODataPath odataPath = routeContext.HttpContext.ODataFeature().Path;
            HttpRequest request = routeContext.HttpContext.Request;

            SelectControllerResult controllerResult = SelectControllerImpl(
                odataPath,
                new WebApiRequestMessage(request),
                this.AttributeMappings);

            if (controllerResult != null)
            {
                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == controllerResult.ControllerName);

                    string actionName = SelectActionImpl(new WebApiControllerContext(routeContext, controllerResult));

                if (!String.IsNullOrEmpty(actionName))
                {
                    return actionDescriptors.Where(
                        c => String.Equals(c.ActionName, actionName, StringComparison.OrdinalIgnoreCase));
                }
            }

            return null;
        }

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        private IDictionary<ODataPathTemplate, IWebApiActionDescriptor> BuildAttributeMappings(IEnumerable<ControllerActionDescriptor> controllerActions)
        {
            Dictionary<ODataPathTemplate, IWebApiActionDescriptor> attributeMappings =
                new Dictionary<ODataPathTemplate, IWebApiActionDescriptor>();

            foreach (ControllerActionDescriptor controllerAction in controllerActions)
            {
                if (IsODataController(controllerAction) && ShouldMapController(controllerAction))
                {
                    foreach (string prefix in GetODataRoutePrefixes(controllerAction))
                    {
                        IEnumerable<ODataPathTemplate> pathTemplates = GetODataPathTemplates(prefix, controllerAction);
                        foreach (ODataPathTemplate pathTemplate in pathTemplates)
                        {
                            attributeMappings.Add(pathTemplate, new WebApiActionDescriptor(controllerAction));
                        }
                    }
                }
            }

            return attributeMappings;
        }

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        private static bool IsODataController(ControllerActionDescriptor controllerAction)
        {
            return typeof(ODataController).IsAssignableFrom(controllerAction.ControllerTypeInfo.AsType());
        }

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        private static IEnumerable<string> GetODataRoutePrefixes(ControllerActionDescriptor controllerAction)
        {
            Contract.Assert(controllerAction != null);

            IEnumerable<ODataRoutePrefixAttribute> prefixAttributes = controllerAction.ControllerTypeInfo.GetCustomAttributes<ODataRoutePrefixAttribute>(inherit: false);

            return GetODataRoutePrefixes(prefixAttributes, controllerAction.ControllerTypeInfo.FullName);
        }

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        private IEnumerable<ODataPathTemplate> GetODataPathTemplates(string prefix, ControllerActionDescriptor controllerAction)
        {
            Contract.Assert(controllerAction != null);

            IEnumerable<ODataRouteAttribute> routeAttributes =
                controllerAction.MethodInfo.GetCustomAttributes<ODataRouteAttribute>(inherit: false);

            IPerRouteContainer perRouteContainer = _serviceProvider.GetRequiredService<IPerRouteContainer>();
            if (perRouteContainer == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(IPerRouteContainer));
            }

            IServiceProvider requestContainer = perRouteContainer.GetODataRootContainer(_routeName);

            string controllerName = controllerAction.ControllerName;
            string actionName = controllerAction.ActionName;

            return
                routeAttributes
                    .Where(route => string.IsNullOrEmpty(route.RouteName) || route.RouteName == _routeName)
                    .Select(route => GetODataPathTemplate(prefix, route.PathTemplate, requestContainer, controllerName, actionName))
                    .Where(template => template != null);
        }
    }
}
