using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.OData
{
    // TODO: Replace with full version in the future.
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class EnableQueryAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull("context");
            }

            var response = context.Result as StatusCodeResult;
            if (response != null && !response.IsSuccessStatusCode())
            {
                return;
            }

            var request = context.HttpContext.Request;
            if (request.HasQueryOptions())
            {
                var result = context.Result as ObjectResult;
                if (result == null)
                {
                    throw Error.Argument("context", SRResources.QueryingRequiresObjectContent, context.Result.GetType().FullName);
                }
                var value = result.Value;
                if (value != null)
                {
                    var elementClrType = TypeHelper.GetImplementedIEnumerableType(value.GetType()) ?? value.GetType();
                    var model = request.ODataFeature().Model;
                    if (model == null)
                    {
                        throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
                    }
                    var queryContext = new ODataQueryContext(
                        model,
                        elementClrType,
                        request.ODataFeature().Path);

                    var queryOptions = new ODataQueryOptions(queryContext, request);

                    long? count = null;
                    var items = ApplyQueryOptions(result.Value, queryOptions, context.ActionDescriptor) as IEnumerable<object>;
                    if (queryOptions.Count)
                    {
                        count = Count(result.Value, queryOptions, context.ActionDescriptor);
                    }
                    // We might be getting a single result, so no paging involved
                    if (items != null)
                    {
                        var pageResult = new PageResult<object>(items, null, count);
                        result.Value = pageResult;
                    }
                }
            }
        }

        public virtual object ApplyQueryOptions(object value, ODataQueryOptions options, ActionDescriptor descriptor)
        {


            var enumerable = value as IEnumerable;
            if (enumerable == null)
            {
                // response is single entity.
                return value;
            }

            // response is a collection.
            var query = (value as IQueryable) ?? enumerable.AsQueryable();
            return options.ApplyTo(query,
                new ODataQuerySettings
                {
                    HandleNullPropagation = HandleNullPropagationOption.True
                });
        }

        public virtual long Count(object value, ODataQueryOptions options, ActionDescriptor descriptor)
        {
            var enumerable = value as IEnumerable;
            if (enumerable == null)
            {
                // response is single entity.
                return 1;
            }

            // response is a collection.
            var query = (value as IQueryable) ?? enumerable.AsQueryable();
            var settings = new ODataQuerySettings
            {
                HandleNullPropagation = HandleNullPropagationOption.True
            };
            var forCount = options.ApplyForCount(query, settings);
            var count = forCount.Cast<object>().LongCount();
            return count;
        }
    }
}