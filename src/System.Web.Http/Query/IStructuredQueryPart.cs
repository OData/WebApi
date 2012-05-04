// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;

namespace System.Web.Http.Query
{
    /// <summary>
    /// Represents a query option like $filter, $top etc.
    /// </summary>
    public interface IStructuredQueryPart
    {
        /// <summary>
        /// The query operator that this query parameter is for.
        /// </summary>
        string QueryOperator { get; }

        /// <summary>
        /// The value part of the query parameter for this query part.
        /// </summary>
        string QueryExpression { get; }

        /// <summary>
        /// Applies this <see cref="IStructuredQueryPart"/> on to an <see cref="IQueryable"/>
        /// returning the resultant <see cref="IQueryable"/>
        /// </summary>
        /// <param name="source">The source <see cref="IQueryable"/></param>
        /// <returns>The resultant <see cref="IQueryable"/></returns>
        IQueryable ApplyTo(IQueryable source);
    }
}
