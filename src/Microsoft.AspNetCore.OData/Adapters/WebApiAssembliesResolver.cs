//-----------------------------------------------------------------------------
// <copyright file="WebApiAssembliesResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi assembly resolver to OData WebApi.
    /// </summary>
    internal partial class WebApiAssembliesResolver : IWebApiAssembliesResolver
    {
        /// <summary>
        /// The inner manager wrapped by this instance.
        /// </summary>
        private ApplicationPartManager innerManager;

        /// <summary>
        /// Initializes a new instance of the WebApiAssembliesResolver class.
        /// </summary>
        public WebApiAssembliesResolver()
        {
            this.innerManager = null;
        }

        /// <summary>
        /// Initializes a new instance of the WebApiAssembliesResolver class.
        /// </summary>
        /// <param name="applicationPartManager">The inner manager.</param>
        public WebApiAssembliesResolver(ApplicationPartManager applicationPartManager)
        {
            this.innerManager = applicationPartManager;
        }

        /// <summary>
        /// Returns a list of assemblies available for the application.
        /// </summary>
        /// <returns>A list of assemblies available for the application.</returns>
        public IEnumerable<Assembly> Assemblies
        {
            get
            {
                if (this.innerManager != null)
                {
                    IList<ApplicationPart> parts = this.innerManager.ApplicationParts;
                    IEnumerable<Assembly> assemblies = parts
                        .OfType<AssemblyPart>()
                        .Select(p => (p as AssemblyPart).Assembly)
                        .Distinct();

                    if (assemblies.Any())
                    {
                        return assemblies;
                    }
                }

                // Without an ApplicationPartManager, fall back to a list of assemblies for the entire process.
                // We cannot provide one on a per-route basis because that would require a request container,
                // which would already have a part manager. The AppDomain provides a list of all assemblies
                // in the domain; .NET Core 1.x does not support AppDomain but .NET Core 2.x does.
                return AppDomain.CurrentDomain.GetAssemblies().Distinct();
            }
        }
    }
}
