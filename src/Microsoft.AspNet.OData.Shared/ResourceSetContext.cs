// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Contains context information about the resource set currently being serialized.
    /// </summary>
    public partial class ResourceSetContext
    {
        /// <summary>
        /// Gets the <see cref="IEdmEntitySetBase"/> this instance belongs to.
        /// </summary>
        public IEdmEntitySetBase EntitySetBase { get; set; }

        /// <summary>
        /// Gets the value of this feed instance.
        /// </summary>
        public object ResourceSetInstance { get; set; }
    }
}
