//-----------------------------------------------------------------------------
// <copyright file="ICountOptionCollection.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections;

namespace Microsoft.AspNet.OData.Query
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
