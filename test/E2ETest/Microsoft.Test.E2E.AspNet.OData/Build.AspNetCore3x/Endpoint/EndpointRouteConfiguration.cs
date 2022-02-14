//-----------------------------------------------------------------------------
// <copyright file="EndpointRouteConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.Test.AspNet.OData.TestCommon;

namespace Microsoft.Test.E2E.AspNet.OData.Endpoint
{
    /// <summary>
    /// Add configuration allow callers to configure <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    public sealed class EndpointRouteConfiguration : IEndpointRouteBuilder
    {
        private IEndpointRouteBuilder routeBuilder;
        private ApplicationPartManager _scopedPartManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndpointRouteConfiguration"/> class
        /// </summary>
        public EndpointRouteConfiguration(IEndpointRouteBuilder routeBuilder)
        {
            this.routeBuilder = routeBuilder ?? throw new ArgumentNullException(nameof(routeBuilder));
        }

        /// <summary>
        /// Add a list of controllers to be discovered by the application.
        /// </summary>
        /// <param name="controllers"></param>
        public void AddControllers(params Type[] controllers)
        {
            // Strip out all the IApplicationPartTypeProvider parts.
            _scopedPartManager = routeBuilder.ServiceProvider.GetRequiredService<ApplicationPartManager>();
            IList<ApplicationPart> parts = _scopedPartManager.ApplicationParts;
            IList<ApplicationPart> nonAssemblyParts = parts.Where(p => p.GetType() != typeof(IApplicationPartTypeProvider)).ToList();
            _scopedPartManager.ApplicationParts.Clear();
            _scopedPartManager.ApplicationParts.Concat(nonAssemblyParts);

            // Add a new AssemblyPart with the controllers.
            AssemblyPart part = new AssemblyPart(new TestAssembly(controllers));
            _scopedPartManager.ApplicationParts.Add(part);
        }

        public ICollection<EndpointDataSource> DataSources => routeBuilder.DataSources;

        public IServiceProvider ServiceProvider => routeBuilder.ServiceProvider;

        public IApplicationBuilder CreateApplicationBuilder() => routeBuilder.CreateApplicationBuilder();
    }
}
