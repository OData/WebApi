using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.Filters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.OData
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

            var response = context.HttpContext.Response;
            if (!response.IsSuccessStatusCode())
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
                    var model = request.ODataProperties().Model;
                    if (model == null)
                    {
                        throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
                    }
                    var queryContext = new ODataQueryContext(
                        model,
                        elementClrType,
                        request.ODataProperties().Path);

                    var queryOptions = new ODataQueryOptions(queryContext, request);

                    long? count = null;
                    var items = ApplyQueryOptions(result.Value, queryOptions, context.ActionDescriptor) as IEnumerable<object>;
                    if (queryOptions.Count)
                    {
                        count = Count(result.Value, queryOptions, context.ActionDescriptor);
                    }
                    var pageResult = new PageResult<object>(items, null, count);
                    result.Value = pageResult;
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