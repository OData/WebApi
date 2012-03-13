using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Microsoft.Web.Http.Data
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    internal sealed class QueryFilterAttribute : ActionFilterAttribute
    {
        internal static readonly string TotalCountKey = "MS_InlineCountKey";
        private static readonly MethodInfo _getTotalCountMethod = typeof(QueryFilterAttribute).GetMethod("GetTotalCount", BindingFlags.NonPublic | BindingFlags.Static);

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            bool inlineCount = false;
            HttpRequestMessage request = context.Request;
            if (request != null && request.RequestUri != null &&
                !String.IsNullOrWhiteSpace(request.RequestUri.Query))
            {
                // search the URI for an inline count request
                var parsedQuery = request.RequestUri.ParseQueryString();
                var inlineCountPart = parsedQuery["$inlinecount"];
                if (inlineCountPart == "allpages")
                {
                    inlineCount = true;
                }
            }

            HttpResponseMessage response = context.Result;
            if (!inlineCount || response == null)
            {
                return;
            }

            IQueryable results;
            ObjectContent objectContent = response.Content as ObjectContent;
            if (objectContent != null && (results = objectContent.Value as IQueryable) != null)
            {
                // Compute the total count and add the result as a request property. Later after all
                // filters have run, DataController will transform the final result into a QueryResult
                // which includes this value.
                // TODO : use a compiled/cached deletate?
                int totalCount = (int)_getTotalCountMethod.MakeGenericMethod(results.ElementType).Invoke(null, new object[] { results, context.ActionContext });
                request.Properties.Add(QueryFilterAttribute.TotalCountKey, totalCount);
            }
        }

        /// <summary>
        /// Determine the total count for the specified query.
        /// </summary>
        private static int GetTotalCount<T>(IQueryable<T> results, HttpActionContext context)
        {
            // A total count of -1 indicates that the count is the result count. This
            // is the default, unless we discover that skip/top operations will be
            // performed, in which case we'll form and execute a count query.
            int totalCount = -1;

            IQueryable totalCountQuery = null;
            if (TryRemovePaging(results, out totalCountQuery))
            {
                totalCount = ((IQueryable<T>)totalCountQuery).Count();
            }
            else if (context.ActionDescriptor.GetFilterPipeline().Any(p => p.Instance is ResultLimitAttribute))
            {
                // The client query didn't specify any skip/top paging operations.
                // However, this action has a ResultLimitFilter applied.
                // Therefore, we need to take the count now before that limit is applied.
                totalCount = results.Count();
            }

            return totalCount;
        }

        /// <summary>
        /// Inspects the specified query and if the query has any paging operators
        /// at the end of it (either a single Take or a Skip/Take) the underlying
        /// query w/o the Skip/Take is returned.
        /// </summary>
        /// <param name="query">The query to inspect.</param>
        /// <param name="countQuery">The resulting count query. Null if there is no paging.</param>
        /// <returns>True if a count query is returned, false otherwise.</returns>
        internal static bool TryRemovePaging(IQueryable query, out IQueryable countQuery)
        {
            MethodCallExpression mce = query.Expression as MethodCallExpression;
            Expression countExpr = null;

            if (IsSequenceOperator("take", mce))
            {
                // strip off the Take operator
                countExpr = mce.Arguments[0];

                mce = countExpr as MethodCallExpression;
                if (IsSequenceOperator("skip", mce))
                {
                    // If there's a skip then we need to exclude that too. No skip means we're 
                    // on the first page.
                    countExpr = mce.Arguments[0];
                }
            }

            countQuery = null;
            if (countExpr != null)
            {
                countQuery = query.Provider.CreateQuery(countExpr);
                return true;
            }

            return false;
        }

        private static bool IsSequenceOperator(string operatorName, MethodCallExpression mce)
        {
            if (mce != null && mce.Method.DeclaringType == typeof(Queryable) &&
                mce.Method.Name.Equals(operatorName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}
