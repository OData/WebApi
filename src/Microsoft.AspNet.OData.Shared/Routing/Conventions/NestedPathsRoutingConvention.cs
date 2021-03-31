// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles GET requests
    /// to arbitrarily nested paths. It requires a controller action with the [EnableNestedPaths] attribute.
    /// This should be the last convention in order not to override other routing conventions that
    /// rely on user-defined methods.
    /// </summary>
    public partial class NestedPathsRoutingConvention : NavigationSourceRoutingConvention
    {
        /// <summary>
        /// Selects the action for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionMap">The action map.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected action
        /// </returns>
        internal static string SelectActionImpl(ODataPath odataPath, IWebApiControllerContext controllerContext, IWebApiActionMap actionMap)
        {

            ODataRequestMethod method = controllerContext.Request.GetRequestMethodOrPreflightMethod();

            if (method != ODataRequestMethod.Get)
            {
                // [EnableNestedPaths] only supports GET requests
                return null;
            }

            if (odataPath.PathTemplate.EndsWith("$ref"))
            {
                // [EnableNestedPaths] currently does not support $ref requests
                return null;
            }

            string sourceName = null;
            ODataPathSegment firstSegment = odataPath.Segments.FirstOrDefault();
            if (firstSegment is EntitySetSegment entitySetSegment)
            {
                sourceName = entitySetSegment.EntitySet.Name;
            }
            else if (firstSegment is SingletonSegment singletonSegment)
            {
                sourceName = singletonSegment.Singleton.Name;
            }
            else
            {
                // this only supports paths starting with an entity set or singleton
                return null;
            }




            // if we did no find a matching action amongst the conventional user-defined methods
            // then let's check if the controller has a Get method with [EnableNestedPaths] attribute
            // which should be used to catch any any nested GET request

            // TODO figure out a better way of getting the action descriptors or controller methods
            // this hack will only work on .net core
            var actionCollectionProvider = (controllerContext as WebApiControllerContext)
                .RouteContext.HttpContext.RequestServices.GetRequiredService<IActionDescriptorCollectionProvider>();

            //IActionDescriptorCollectionProvider actionCollectionProvider =
            //       controllerContext.Request.RequestContainer.GetRequiredService<IActionDescriptorCollectionProvider>();

            // check if we have a Get() method in this controller with [EnableNestedPaths] attribute
            if (actionCollectionProvider != null)
            {
                var controllerResult = controllerContext.ControllerResult;
                IEnumerable<ControllerActionDescriptor> actionDescriptors = actionCollectionProvider
                    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                    .Where(c => c.ControllerName == controllerResult.ControllerName);

                var action =
                    actionDescriptors.Where(a =>
                        (a.MethodInfo.Name == "Get" || a.MethodInfo.Name == $"Get{sourceName}")
                        && a.MethodInfo.GetCustomAttributes(true).OfType<EnableNestedPathsAttribute>().Any())
                    .FirstOrDefault();

                if (action != null)
                {
                    return action.ActionName;
                }
            }


            return null;
        }
    }
}
