// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
    internal class WebApiAssembliesResolver : IWebApiAssembliesResolver
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
                    return parts.Where(p => p is AssemblyPart).Select(p => (p as AssemblyPart).Assembly);
                }

                // Cannot get the list of assemblies without an innerManager.
                throw new NotImplementedException();
            }
        }
    }
}
