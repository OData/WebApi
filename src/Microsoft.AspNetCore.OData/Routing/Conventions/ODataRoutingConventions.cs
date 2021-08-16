//-----------------------------------------------------------------------------
// <copyright file="ODataRoutingConventions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Routing;

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
        /// <param name="builder">The <see cref="IRouteBuilder"/> to add the route to.</param>
        /// <returns>A mutable list of the default OData routing conventions.</returns>
        public static IList<IODataRoutingConvention> CreateDefaultWithAttributeRouting(
            string routeName,
            IRouteBuilder builder)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull(nameof(builder));
            }

            return CreateDefaultWithAttributeRouting(routeName, builder.ServiceProvider);
        }

        /// <summary>
        /// Creates a mutable list of the default OData routing conventions with attribute routing enabled.
        /// For Endpoint routing, please use this version.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A mutable list of the default OData routing conventions.</returns>
        public static IList<IODataRoutingConvention> CreateDefaultWithAttributeRouting(
            string routeName,
            IServiceProvider serviceProvider)
        {
            if (routeName == null)
            {
                throw Error.ArgumentNull("routeName");
            }

            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            IList<IODataRoutingConvention> routingConventions = CreateDefault();
            AttributeRoutingConvention routingConvention = new AttributeRoutingConvention(routeName, serviceProvider);
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
                new OperationImportRoutingConvention(),
                new EntityRoutingConvention(),
                new NavigationRoutingConvention(),
                new PropertyRoutingConvention(),
                new DynamicPropertyRoutingConvention(),
                new RefRoutingConvention(),
                new ActionRoutingConvention(),
                new FunctionRoutingConvention(),
                new NestedPathsRoutingConvention(),
                new UnmappedRequestRoutingConvention()
            };
        }
    }
}
