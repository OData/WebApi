// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.ObjectModel;

namespace System.Web.Http.OData.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for creating routing conventions.
    /// </summary>
    public static class ODataRoutingConventions
    {
        /// <summary>
        /// Creates a mutable collection of the default OData routing conventions.
        /// </summary>
        /// <returns>A mutable collection of the default OData routing conventions.</returns>
        public static Collection<IODataRoutingConvention> CreateDefault()
        {
            return new Collection<IODataRoutingConvention>()
            {
                new MetadataRoutingConvention(),
                new EntitySetRoutingConvention(),
                new EntityRoutingConvention(),
                new NavigationRoutingConvention(),
                new LinksRoutingConvention(),
                new ActionRoutingConvention(),
                new UnmappedRequestRoutingConvention()
            };
        }
    }
}