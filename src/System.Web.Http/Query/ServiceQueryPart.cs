// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Properties;

namespace System.Web.Http.Query
{
    /// <summary>
    /// Represents a single query operator to be applied to a query
    /// </summary>
    internal class ServiceQueryPart
    {
        /// <summary>
        /// Public constructor
        /// </summary>
        public ServiceQueryPart()
        {
        }

        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="queryOperator">The query operator</param>
        /// <param name="expression">The query expression</param>
        public ServiceQueryPart(string queryOperator, string expression)
        {
            if (queryOperator == null)
            {
                throw Error.ArgumentNull("queryOperator");
            }
            if (expression == null)
            {
                throw Error.ArgumentNull("expression");
            }

            if (!ServiceQuery.IsSupportedQueryOperator(queryOperator))
            {
                throw Error.Argument("queryOperator", SRResources.InvalidQueryOperator, queryOperator);
            }

            QueryOperator = queryOperator;
            Expression = expression;
        }

        /// <summary>
        /// Gets or sets the query operator. Must be one of the supported operators : "where", "orderby", "skip", or "take".
        /// </summary>
        public string QueryOperator { get; set; }

        /// <summary>
        /// Gets or sets the query expression.
        /// </summary>
        public string Expression { get; set; }

        /// <summary>
        /// Returns a string representation of this <see cref="ServiceQueryPart"/>
        /// </summary>
        /// <returns>The string representation of this <see cref="ServiceQueryPart"/></returns>
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}={1}", QueryOperator, Expression);
        }
    }
}
