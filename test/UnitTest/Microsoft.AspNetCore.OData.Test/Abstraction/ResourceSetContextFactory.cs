//-----------------------------------------------------------------------------
// <copyright file="ResourceSetContextFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A class to create ResourceSetContextFactory.
    /// </summary>
    public class ResourceSetContextFactory
    {
        /// <summary>
        /// Initializes a new instance of ResourceSetContext.
        /// </summary>
        /// <returns>A new instance of ResourceSetContext.</returns>
        public static ResourceSetContext Create(IEdmEntitySet entitySetBase, HttpRequest request)
        {
            return new ResourceSetContext { EntitySetBase = entitySetBase, Request = request };
        }
    }
}
