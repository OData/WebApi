// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation sources
    /// (entity sets or singletons)
    /// </summary>
    public abstract class NavigationSourceRoutingConvention : IODataRoutingConvention
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
            if (odataPath == null)
            {
                throw Error.ArgumentNull("odataPath");
            }

            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            // entity set
            EntitySetPathSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetPathSegment;
            if (entitySetSegment != null)
            {
                return entitySetSegment.EntitySetName;
            }

            // singleton
            SingletonPathSegment singletonSegment = odataPath.Segments.FirstOrDefault() as SingletonPathSegment;
            if (singletonSegment != null)
            {
                return singletonSegment.SingletonName;
            }

            return null;
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
    }
}
