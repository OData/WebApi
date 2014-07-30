// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing.Conventions
{
    /// <summary>
    /// Provides helper methods for creating routing conventions.
    /// </summary>
    public static class ODataRoutingConventions
    {
        /// <summary>
        /// Creates a mutable list of the default OData routing conventions with attribute routing enabled.
        /// </summary>
        /// <param name="configuration">The server configuration.</param>
        /// <param name="model">The EDM model to use for parsing OData paths.</param>
        /// <returns>A mutable list of the default OData routing conventions.</returns>
        public static IList<IODataRoutingConvention> CreateDefaultWithAttributeRouting(
            HttpConfiguration configuration,
            IEdmModel model)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            IList<IODataRoutingConvention> routingConventions = CreateDefault();
            AttributeRoutingConvention routingConvention = new AttributeRoutingConvention(model, configuration);
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
                new NavigationRoutingConvention(),
                new PropertyRoutingConvention(),
                new RefRoutingConvention(),
                new ActionRoutingConvention(),
                new FunctionRoutingConvention(),
                new UnmappedRequestRoutingConvention()
            };
        }
    }
}