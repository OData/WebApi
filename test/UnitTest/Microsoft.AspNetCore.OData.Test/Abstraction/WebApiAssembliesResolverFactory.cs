//-----------------------------------------------------------------------------
// <copyright file="WebApiAssembliesResolverFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Test.Common;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create WebApiAssembliesResolver.
    /// </summary>
    public class WebApiAssembliesResolverFactory
    {
        /// <summary>
        /// Initializes a new instance of the routing configuration class.
        /// </summary>
        /// <returns>A new instance of the routing configuration class.</returns>
        internal static IWebApiAssembliesResolver Create(MockAssembly assembly = null)
        {
            IRouteBuilder builder = RoutingConfigurationFactory.Create();

            ApplicationPartManager applicationPartManager = builder.ApplicationBuilder.ApplicationServices.GetRequiredService<ApplicationPartManager>();
            applicationPartManager.ApplicationParts.Clear();

            if (assembly != null)
            {
                AssemblyPart part = new AssemblyPart(assembly);
                applicationPartManager.ApplicationParts.Add(part);
            }

            return new WebApiAssembliesResolver(applicationPartManager);
        }
    }
}
