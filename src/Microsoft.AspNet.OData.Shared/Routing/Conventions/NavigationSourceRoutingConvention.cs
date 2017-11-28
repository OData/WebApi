// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that handles navigation sources
    /// (entity sets or singletons)
    /// </summary>
    public abstract partial class NavigationSourceRoutingConvention
    {
        /// <summary>
        /// Selects the controller for OData requests.
        /// </summary>
        /// <param name="odataPath">The OData path.</param>
        /// <returns>
        ///   <c>null</c> if the request isn't handled by this convention; otherwise, the name of the selected controller
        /// </returns>
        internal static SelectControllerResult SelectControllerImpl(ODataPath odataPath)
        {
            // entity set
            EntitySetSegment entitySetSegment = odataPath.Segments.FirstOrDefault() as EntitySetSegment;
            if (entitySetSegment != null)
            {
                return new SelectControllerResult(entitySetSegment.EntitySet.Name, null);
            }

            // singleton
            SingletonSegment singletonSegment = odataPath.Segments.FirstOrDefault() as SingletonSegment;
            if (singletonSegment != null)
            {
                return new SelectControllerResult(singletonSegment.Singleton.Name, null);
            }

            return null;
        }
    }
}
