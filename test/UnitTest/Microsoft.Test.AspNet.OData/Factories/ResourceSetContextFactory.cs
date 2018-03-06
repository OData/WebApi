// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

#if NETCORE
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;
#else
using System.Net.Http;
using Microsoft.AspNet.OData;
using Microsoft.OData.Edm;
#endif

namespace Microsoft.Test.AspNet.OData.Factories
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
#if NETCORE
        public static ResourceSetContext Create(IEdmEntitySet entitySetBase, HttpRequest request)
        {
            return new ResourceSetContext { EntitySetBase = entitySetBase, Request = request };
        }
#else
        public static ResourceSetContext Create(IEdmEntitySet entitySetBase, HttpRequestMessage request)
        {
            return new ResourceSetContext { EntitySetBase = entitySetBase, Request = request, Url = request.GetUrlHelper() };
        }
#endif
    }
}
