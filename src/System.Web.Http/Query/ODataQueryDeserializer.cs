// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Http.Internal;
using System.Web.Http.Properties;

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

            ServiceQuery serviceQuery = GetServiceQuery(uri);

            return Deserialize(query, serviceQuery.QueryParts, null);
        }

        /// <summary>
        /// Deserializes the query operations in the specified Uri and returns an IQueryable
        /// with a manufactured query root with those operations applied.
        /// </summary>
        /// <typeparam name="T">The element type of the query</typeparam>
        /// <param name="uri">The request Uri containing the query operations.</param>
        /// <returns>The resulting IQueryable with the deserialized query composed over it.</returns>
        public static IQueryable<T> Deserialize<T>(Uri uri)
        {
            if (uri == null)
            {
                throw Error.ArgumentNull("uri");
            }

            return (IQueryable<T>)Deserialize(typeof(T), uri);
        }

        /// <summary>
        /// Deserializes the query operations in the specified Uri and returns an IQueryable
        /// with a manufactured query root with those operations applied.
        /// </summary>
        /// <param name="elementType">The element type of the query</param>
        /// <param name="uri">The request Uri containing the query operations.</param>
        /// <returns>The resulting IQueryable with the deserialized query composed over it.</returns>
        public static IQueryable Deserialize(Type elementType, Uri uri)
        {
            if (elementType == null)
            {
                throw Error.ArgumentNull("elementType");
            }

            if (uri == null)
            {
                throw Error.ArgumentNull("uri");
            }

            ServiceQuery serviceQuery = GetServiceQuery(uri);

            Array array = Array.CreateInstance(elementType, 0);
            IQueryable baseQuery = ((IEnumerable)array).AsQueryable();

            return Deserialize(baseQuery, serviceQuery.QueryParts, null);
        }

        internal static IQueryable Deserialize(IQueryable query, IEnumerable<ServiceQueryPart> queryParts)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (queryParts == null)
            {
                throw Error.ArgumentNull("queryParts");
            }

            return Deserialize(query, queryParts, null);
        }

        internal static IQueryable Deserialize(IQueryable query, IEnumerable<ServiceQueryPart> queryParts, QueryResolver queryResolver)
        {
            if (query == null)
            {
                throw Error.ArgumentNull("query");
            }

            if (queryParts == null)
            {
                throw Error.ArgumentNull("queryParts");
            }

            foreach (ServiceQueryPart part in queryParts)
            {
                switch (part.QueryOperator)
                {
                    case "filter":
                        try
                        {
                            query = DynamicQueryable.Where(query, part.Expression, queryResolver);
                        }
                        catch (ParseException e)
                        {
                            throw new ParseException(
                                Error.Format(SRResources.ParseErrorInClause, "$filter", e.Message));
                        }
                        break;
                    case "orderby":
                        try
                        {
                            query = DynamicQueryable.OrderBy(query, part.Expression, queryResolver);
                        }
                        catch (ParseException e)
                        {
                            throw new ParseException(
                                Error.Format(SRResources.ParseErrorInClause, "$orderby", e.Message));
                        }
                        break;
                    case "skip":
                        try
                        {
                            int skipCount = Convert.ToInt32(part.Expression, System.Globalization.CultureInfo.InvariantCulture);
                            if (skipCount < 0)
                            {
                                throw new ParseException(
                                        Error.Format(SRResources.PositiveIntegerExpectedForODataQueryParameter, "$skip", part.Expression));
                            }

                            query = DynamicQueryable.Skip(query, skipCount);
                        }
                        catch (FormatException e)
                        {
                            throw new ParseException(
                                Error.Format(SRResources.ParseErrorInClause, "$skip", e.Message));
                        }
                        break;
                    case "top":
                        try
                        {
                            int topCount = Convert.ToInt32(part.Expression, System.Globalization.CultureInfo.InvariantCulture);
                            if (topCount < 0)
                            {
                                throw new ParseException(
                                    Error.Format(SRResources.PositiveIntegerExpectedForODataQueryParameter, "$top", part.Expression));
                            }

                            query = DynamicQueryable.Take(query, topCount);
                        }
                        catch (FormatException e)
                        {
                            throw new ParseException(
                                Error.Format(SRResources.ParseErrorInClause, "$top", e.Message));
                        }
                        break;
                }
            }

            return query;
        }

        internal static ServiceQuery GetServiceQuery(Uri uri)
        {
            if (uri == null)
            {
                throw Error.ArgumentNull("uri");
            }

            NameValueCollection queryPartCollection = UriQueryUtility.ParseQueryString(uri.Query);

            List<ServiceQueryPart> serviceQueryParts = new List<ServiceQueryPart>();
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
                    if (!ServiceQuery.IsSupportedQueryOperator(queryOperator))
                    {
                        // skip any operators we don't support
                        continue;
                    }

                    ServiceQueryPart serviceQueryPart = new ServiceQueryPart(queryOperator, value);
                    serviceQueryParts.Add(serviceQueryPart);
                }
            }

            // Query parts for OData need to be ordered $filter, $orderby, $skip, $top. For this
            // set of query operators, they are already in alphabetical order, so it suffices to
            // order by operator name. In the future if we support other operators, this may need
            // to be reexamined.
            serviceQueryParts = serviceQueryParts.OrderBy(p => p.QueryOperator).ToList();

            ServiceQuery serviceQuery = new ServiceQuery()
            {
                QueryParts = serviceQueryParts,
            };

            return serviceQuery;
        }
    }
}
