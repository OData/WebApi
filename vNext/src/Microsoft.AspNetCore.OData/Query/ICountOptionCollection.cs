// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;

namespace Microsoft.AspNetCore.OData.Query
{
    /// <summary>
    /// Represents a collection that has total count.
    /// </summary>
    internal interface ICountOptionCollection : IEnumerable
    {
        /// <summary>
        /// Gets a value representing the total count of the collection.
        /// </summary>
        long? TotalCount { get; }
    }
}
