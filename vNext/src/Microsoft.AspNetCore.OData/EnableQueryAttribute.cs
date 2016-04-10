using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;

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

			var response = context.HttpContext.Response;
			if (!response.IsSuccessStatusCode())
			{
				return;
			}

			var request = context.HttpContext.Request;

			if (request.HasQueryOptions() ||
				ODataCountMediaTypeMapping.IsCountRequest(request) ||
				context.ActionDescriptor.HasQueryOption())
			{
				var result = context.Result as ObjectResult;
				if (result == null)
				{
					throw Error.Argument("context", SRResources.QueryingRequiresObjectContent, context.Result.GetType().FullName);
				}

				if (result.Value != null)
				{
					result.Value = ApplyQueryOptions(result.Value, request, context.ActionDescriptor, context.Controller.GetType().GetTypeInfo().Assembly.FullName);
				}
			}
		}

		public virtual object ApplyQueryOptions(object value, HttpRequest request, ActionDescriptor actionDescriptor, string assemblyName)
		{
			var elementClrType = value is IEnumerable
				? TypeHelper.GetImplementedIEnumerableType(value.GetType())
				: value.GetType();

			var model = request.ODataProperties().Model;
			if (model == null)
			{
				throw Error.InvalidOperation(SRResources.QueryGetModelMustNotReturnNull);
			}

			var queryContext = new ODataQueryContext(
				model,
				elementClrType,
				assemblyName,
				request.ODataProperties().Path
				);

			var queryOptions = new ODataQueryOptions(queryContext, request, assemblyName);

			var enumerable = value as IEnumerable;
			if (enumerable == null)
			{
				// response is single entity.
				return value;
			}

			// response is a collection.
			var query = (value as IQueryable) ?? enumerable.AsQueryable();

			query = queryOptions.ApplyTo(query,
				new ODataQuerySettings
				{
					// TODO: If we are using SQL, set this to false
					// otherwise if it is entities in code then
					// set it to true
					HandleNullPropagation = 
					//HandleNullPropagationOption.True
					HandleNullPropagationOptionHelper.GetDefaultHandleNullPropagationOption(query)
					,
					PageSize = actionDescriptor.PageSize(),
					SearchDerivedTypeWhenAutoExpand = true
				},
				AllowedQueryOptions.None);
			// Determine if this result should be a single entity
			
			if (ShouldBeSingleEntity(queryOptions))
			{
				return SingleOrDefault(query, actionDescriptor);
			}
			if (ODataCountMediaTypeMapping.IsCountRequest(request))
			{
				long? count = request.ODataProperties().TotalCount;

				if (count.HasValue)
				{
					// Return the count value if it is a $count request.
					return count.Value;
				}
			}
			return query;
		}

		private bool ShouldBeSingleEntity(ODataQueryOptions queryOptions)
		{
			return queryOptions.Context.Path.PathTemplate == "~/entityset/key";
		}

		internal static object SingleOrDefault(IQueryable queryable, ActionDescriptor actionDescriptor)
		{
			var enumerator = queryable.GetEnumerator();
			try
			{
				var result = enumerator.MoveNext() ? enumerator.Current : null;

				if (enumerator.MoveNext())
				{
					throw new InvalidOperationException(Error.Format(
						SRResources.SingleResultHasMoreThanOneEntity));
				}

				return result;
			}
			finally
			{
				// Fix for Issue #2097
				// Ensure any active/open database objects that were created
				// iterating over the IQueryable object are properly closed.
				var disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}