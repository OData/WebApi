// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.OData.Query
{
    /// <summary>
    /// Represents the result of a $select and $expand query operation.
    /// </summary>
    public interface ISelectExpandWrapper
    {
        /// <summary>
        /// Projects the result of a $select and $expand query to a <see cref="IDictionary{TKey,TValue}" />.
        /// </summary>
        /// <returns>An <see cref="IDictionary{TKey,TValue}"/> representing the $select and $expand result.</returns>
        IDictionary<string, object> ToDictionary();
    }
}
