//-----------------------------------------------------------------------------
// <copyright file="WebApiAssembliesResolver.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Adapters
{
    /// <summary>
    /// Adapter class to convert Asp.Net WebApi assembly resolver to OData WebApi.
    /// </summary>
    internal partial class WebApiAssembliesResolver : IWebApiAssembliesResolver
    {
        /// <summary>
        /// The inner resolver wrapped by this instance.
        /// </summary>
        private IAssembliesResolver innerResolver;

        /// <summary>
        /// Initializes a new instance of the WebApiAssembliesResolver class.
        /// </summary>
        public WebApiAssembliesResolver()
        {
            this.innerResolver = new DefaultAssembliesResolver();
        }

        /// <summary>
        /// Initializes a new instance of the WebApiAssembliesResolver class.
        /// </summary>
        /// <param name="resolver">The inner resolver.</param>
        public WebApiAssembliesResolver(IAssembliesResolver resolver)
        {
            if (resolver == null)
            {
                throw Error.ArgumentNull("resolver");
            }

            this.innerResolver = resolver;
        }

        /// <summary>
        /// Returns a list of assemblies available for the application.
        /// </summary>
        /// <returns>A list of assemblies available for the application.</returns>
        public IEnumerable<Assembly> Assemblies
        {
            get
            {
                return this.innerResolver.GetAssemblies();
            }
        }
    }
}
