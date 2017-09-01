// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Adapters;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation sources
    /// (entity sets or singletons)
    /// </summary>
    public abstract partial class NavigationSourceRoutingConvention : IODataRoutingConvention
    {
        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="request">The request.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller
        /// </returns>
        public virtual string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            SelectControllerResult controllerResult = SelectControllerImpl(
                odataPath,
                new WebApiRequestMessage(request));

            if (controllerResult != null)
            {
                request.Properties["AttributeRouteData"] = controllerResult.Values;
            }

            return controllerResult?.ControllerName;
        }

        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
        /// </returns>
        public abstract string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap);

        /// <summary>
        /// Get the controller result used to call the shared version of SelectAction()
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        internal SelectControllerResult GetControllerResult(HttpControllerContext controllerContext)
        {
            object value = null;
            controllerContext?.Request?.Properties.TryGetValue("AttributeRouteData", out value);

            return new SelectControllerResult(
                controllerContext?.ControllerDescriptor?.ControllerName,
                value as IDictionary<string, object>);
        }
    }
}
