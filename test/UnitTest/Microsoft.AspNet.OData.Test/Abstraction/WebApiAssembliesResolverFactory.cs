//-----------------------------------------------------------------------------
// <copyright file="WebApiAssembliesResolverFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Reflection;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Test.Common;
using Moq;

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
            IAssembliesResolver resolver = null;
            if (assembly != null)
            {
                resolver = new TestAssemblyResolver(assembly);
            }
            else
            {
                Mock<IAssembliesResolver> mockAssembliesResolver = new Mock<IAssembliesResolver>();
                mockAssembliesResolver
                    .Setup(r => r.GetAssemblies())
                    .Returns(new Assembly[0]);

                resolver = mockAssembliesResolver.Object;
            }

            return new WebApiAssembliesResolver(resolver);
        }
    }
}
