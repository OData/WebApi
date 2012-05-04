// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.Http.Query
{
    /// <summary>
    /// Represents an <see cref="System.Linq.IQueryable"/>.
    /// </summary>
    public sealed class StructuredQuery
    {
        public StructuredQuery()
        {
            QueryParts = new List<IStructuredQueryPart>();
        }

        /// <summary>
        /// Gets or sets a list of query parts.
        /// </summary>
        public ICollection<IStructuredQueryPart> QueryParts { get; internal set; }
    }
}
