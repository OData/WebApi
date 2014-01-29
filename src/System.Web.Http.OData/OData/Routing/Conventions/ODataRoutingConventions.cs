// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for creating routing conventions.
    /// </summary>
    public static class ODataRoutingConventions
    {
        /// <summary>
        /// Creates a mutable list of the default OData routing conventions.
        /// </summary>
        /// <returns>A mutable list of the default OData routing conventions.</returns>
        public static IList<IODataRoutingConvention> CreateDefault()
        {
            return new List<IODataRoutingConvention>()
            {
                new MetadataRoutingConvention(),
                new EntitySetRoutingConvention(),
                new EntityRoutingConvention(),
                new NavigationRoutingConvention(),
                new PropertyRoutingConvention(),
                new LinksRoutingConvention(),
                new ActionRoutingConvention(),
                new UnmappedRequestRoutingConvention()
            };
        }
    }
}