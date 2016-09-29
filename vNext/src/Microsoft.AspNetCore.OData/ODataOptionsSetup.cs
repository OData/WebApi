// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Sets up default options for <see cref="ODataOptions"/>.
    /// </summary>
    public class ODataOptionsSetup : IConfigureOptions<ODataOptions>
    {
        public void Configure(ODataOptions options)
        {
            // Set up the default routing conventions
            options.RoutingConventions.Add(new AttributeRoutingConvention());
            options.RoutingConventions.Add(new MetadataRoutingConvention());
            options.RoutingConventions.Add(new EntitySetRoutingConvention());
            options.RoutingConventions.Add(new SingletonRoutingConvention());
            options.RoutingConventions.Add(new EntityRoutingConvention());
            options.RoutingConventions.Add(new NavigationRoutingConvention());
            options.RoutingConventions.Add(new PropertyRoutingConvention());
            options.RoutingConventions.Add(new DynamicPropertyRoutingConvention());
            options.RoutingConventions.Add(new RefRoutingConvention());
            options.RoutingConventions.Add(new ActionRoutingConvention());
            options.RoutingConventions.Add(new FunctionRoutingConvention());
            options.RoutingConventions.Add(new UnmappedRequestRoutingConvention());

            // TODO: add more default configuration here
        }
    }
}
