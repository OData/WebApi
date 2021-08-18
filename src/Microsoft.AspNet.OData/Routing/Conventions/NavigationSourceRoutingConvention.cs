//-----------------------------------------------------------------------------
// <copyright file="NavigationSourceRoutingConvention.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http.Controllers;
using Microsoft.AspNet.OData.Common;

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
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public virtual string SelectController(ODataPath odataPath, HttpRequestMessage request)
        {
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            SelectControllerResult controllerResult = SelectControllerImpl(odataPath);

            if (controllerResult != null)
            {
                request.Properties["AttributeRouteData"] = controllerResult.Values;
            }

            return controllerResult != null ? controllerResult.ControllerName : null;
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
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public abstract string SelectAction(ODataPath odataPath, HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap);

        /// <summary>
        /// Validate the parameters passed to SelectAction.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        internal static void ValidateSelectActionParameters(ODataPath odataPath, HttpControllerContext controllerContext,
            ILookup<string, HttpActionDescriptor> actionMap)
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
        }

        /// <summary>
        /// Get the controller result used to call the shared version of SelectAction()
        /// </summary>
        /// <param name="controllerContext">The controller context.</param>
        internal static SelectControllerResult GetControllerResult(HttpControllerContext controllerContext)
        {
            string controllerName = null;
            object value = null;

            if (controllerContext != null)
            {
                if (controllerContext.Request != null)
                {
                    controllerContext.Request.Properties.TryGetValue("AttributeRouteData", out value);
                }

                if (controllerContext.ControllerDescriptor != null)
                {
                    controllerName = controllerContext.ControllerDescriptor.ControllerName;
                }
            }

            return new SelectControllerResult(controllerName, value as IDictionary<string, object>);
        }
    }
}
