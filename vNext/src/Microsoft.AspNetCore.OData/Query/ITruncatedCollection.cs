// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Represents a collection that is truncated to a given page size.
    /// </summary>
    public interface ITruncatedCollection : IEnumerable
    {
        /// <summary>
        /// Gets the page size the collection is truncated to.
        /// </summary>
        int PageSize { get; }

        /// <summary>
        /// Gets a value representing if the collection is truncated or not.
        /// </summary>
        bool IsTruncated { get; }
    }
}
