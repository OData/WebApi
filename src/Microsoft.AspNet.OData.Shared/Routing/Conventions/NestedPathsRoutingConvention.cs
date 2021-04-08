// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE

// TODO: [EnableNestedPaths] feature has not yet been ported to AspNet classic

using System.Linq;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.UriParser;


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


            // unsupported path segments
            if (odataPath.PathTemplate.EndsWith("$ref"))
            {
                return null;
            }

            ODataPathSegment firstSegment = odataPath.Segments.FirstOrDefault();

            string sourceName;
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
            IWebApiActionDescriptor descriptor = actionMap.GetActionDescriptor($"Get{sourceName}") ?? actionMap.GetActionDescriptor("Get");
            if (descriptor == null)
            {
                return null;
            }

            if (!descriptor.GetCustomAttributes<EnableNestedPathsAttribute>(/* inherit */ true).Any())
            {
                return null;
            }

            return descriptor.ActionName;
        }
    }
}
#endif