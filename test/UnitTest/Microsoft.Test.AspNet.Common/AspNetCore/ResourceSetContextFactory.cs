// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Http;
using Microsoft.OData.Edm;

namespace Microsoft.Test.AspNet.OData
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
