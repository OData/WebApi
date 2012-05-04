// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;

namespace System.Web.Http.Query
{
    /// <summary>
    /// Used to deserialize a set of string based query operations into expressions and
    /// compose them over a specified query.
    /// </summary>
    internal static class ODataQueryDeserializer
    {
        /// <summary>
        /// Deserializes the query operations in the specified Uri and applies them
        /// to the specified IQueryable.
        /// </summary>
        /// <param name="query">The root query to compose the deserialized query over.</param>
        /// <param name="uri">The request Uri containing the query operations.</param>
        /// <returns>The resulting IQueryable with the deserialized query composed over it.</returns>
        public static IQueryable Deserialize(IQueryable query, Uri uri)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (uri == null)
            {
                throw Error.ArgumentNull("uri");
            }

            StructuredQuery structuredQuery = GetStructuredQuery(uri);

            return Deserialize(query, structuredQuery.QueryParts);
        }

        internal static IQueryable Deserialize(IQueryable query, IEnumerable<IStructuredQueryPart> queryParts)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (queryParts == null)
            {
                throw Error.ArgumentNull("queryParts");
            }

            foreach (IStructuredQueryPart part in queryParts)
            {
                query = part.ApplyTo(query);
            }

            return query;
        }

        internal static StructuredQuery GetStructuredQuery(Uri uri)
        {
            if (uri == null)
            {
                throw Error.ArgumentNull("uri");
            }

            NameValueCollection queryPartCollection = uri.ParseQueryString();

            List<IStructuredQueryPart> structuredQueryParts = new List<IStructuredQueryPart>();
            foreach (string queryPart in queryPartCollection)
            {
                if (queryPart == null || !queryPart.StartsWith("$", StringComparison.Ordinal))
                {
                    // not a special query string
                    continue;
                }

                foreach (string value in queryPartCollection.GetValues(queryPart))
                {
                    string queryOperator = queryPart.Substring(1);
                    if (!StandardStructuredQueryPart.IsSupportedQueryOperator(queryOperator))
                    {
                        // skip any operators we don't support
                        continue;
                    }

                    StandardStructuredQueryPart structuredQueryPart = new StandardStructuredQueryPart(queryOperator, value);
                    structuredQueryParts.Add(structuredQueryPart);
                }
            }

            // Query parts for OData need to be ordered $filter, $orderby, $skip, $top. For this
            // set of query operators, they are already in alphabetical order, so it suffices to
            // order by operator name. In the future if we support other operators, this may need
            // to be reexamined.
            structuredQueryParts = structuredQueryParts.OrderBy(p => p.QueryOperator).ToList();

            StructuredQuery structuredQuery = new StructuredQuery()
            {
                QueryParts = structuredQueryParts,
            };

            return structuredQuery;
        }
    }
}
