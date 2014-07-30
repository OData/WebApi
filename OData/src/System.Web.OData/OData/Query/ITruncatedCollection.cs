// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;

namespace System.Web.OData.Query
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
