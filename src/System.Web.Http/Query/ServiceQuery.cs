using System.Collections.Generic;

namespace System.Web.Http.Query
{
    /// <summary>
    /// Represents an <see cref="System.Linq.IQueryable"/>.
    /// </summary>
    internal class ServiceQuery
    {
        /// <summary>
        /// Gets or sets a list of query parts.
        /// </summary>
        public List<ServiceQueryPart> QueryParts { get; set; }

        public static bool IsSupportedQueryOperator(string queryOperator)
        {
            return queryOperator == "filter" || queryOperator == "orderby" ||
                   queryOperator == "skip" || queryOperator == "top";
        }
    }
}
