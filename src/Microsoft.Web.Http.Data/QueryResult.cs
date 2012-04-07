// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Runtime.Serialization;

namespace Microsoft.Web.Http.Data
{
    /// <summary>
    /// Represents the results of a query request along with its total count if requested.
    /// </summary>
    [DataContract]
    public sealed class QueryResult
    {
        public QueryResult(IEnumerable results, int totalCount)
        {
            Results = results;
            TotalCount = totalCount;
        }

        /// <summary>
        /// The results of the query.
        /// </summary>
        [DataMember]
        public IEnumerable Results { get; set; }

        /// <summary>
        /// The total count of the query, without any paging options applied.
        /// A TotalCount equal to -1 indicates that the count is equal to the
        /// result count.
        /// </summary>
        [DataMember]
        public int TotalCount { get; set; }
    }
}
