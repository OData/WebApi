// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for creating routing conventions.
    /// </summary>
    public static class ODataRoutingConventions
    {
        /// <summary>
        /// Creates a mutable list of the default OData routing conventions with attribute routing enabled.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="configuration">The server configuration.</param>
        /// <returns>A mutable list of the default OData routing conventions.</returns>
        public static IList<IODataRoutingConvention> CreateDefaultWithAttributeRouting(
            string routeName,
            HttpConfiguration configuration)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            IList<IODataRoutingConvention> routingConventions = CreateDefault();
            AttributeRoutingConvention routingConvention = new AttributeRoutingConvention(routeName, configuration);
            routingConventions.Insert(0, routingConvention);
            return routingConventions;
        }

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
                new SingletonRoutingConvention(),
                new EntityRoutingConvention(),
                new OperationImportRoutingConvention(),
                new NavigationRoutingConvention(),
                new PropertyRoutingConvention(),
                new DynamicPropertyRoutingConvention(),
                new RefRoutingConvention(),
                new ActionRoutingConvention(),
                new FunctionRoutingConvention(),
                new UnmappedRequestRoutingConvention()
            };
        }
    }
}