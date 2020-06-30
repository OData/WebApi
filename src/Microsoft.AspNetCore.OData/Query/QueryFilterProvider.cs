// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// An implementation of <see cref="IFilterProvider" /> that applies an action filter to
    /// any action with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return type
    /// that doesn't bind a parameter of type <see cref="IODataQueryOptions" />.
    /// </summary>
    public class QueryFilterProvider : IFilterProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryFilterProvider" /> class.
        /// </summary>
        /// <param name="queryFilter">The action filter that executes the query.</param>
        public QueryFilterProvider(IActionFilter queryFilter)
        {
            if (queryFilter == null)
            {
                throw Error.ArgumentNull("queryFilter");
            }

            QueryFilter = queryFilter;
        }

        /// <summary>
        /// Gets the action filter that executes the query.
        /// </summary>
        public IActionFilter QueryFilter { get; private set; }

        /// <summary>
        /// Gets the order value for determining the order of execution of providers. Providers
        /// execute in ascending numeric value of the Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.Order
        /// property.
        /// </summary>
        public int Order
        {
            get
            {
                // Providers are executed in an ordering determined by an ascending sort of the
                // Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.Order property. A provider with
                // a lower numeric value of Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.Order
                // will have its Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.OnProvidersExecuting(Microsoft.AspNetCore.Mvc.Filters.FilterProviderContext)
                // called before that of a provider with a higher numeric value of Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.Order.
                // The Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.OnProvidersExecuted(Microsoft.AspNetCore.Mvc.Filters.FilterProviderContext)
                // method is called in the reverse ordering after all calls to Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.OnProvidersExecuting(Microsoft.AspNetCore.Mvc.Filters.FilterProviderContext).
                // A provider with a lower numeric value of Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.Order
                // will have its Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.OnProvidersExecuted(Microsoft.AspNetCore.Mvc.Filters.FilterProviderContext)
                // method called after that of a provider with a higher numeric value of Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.Order.
                // If two providers have the same numeric value of Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.Order,
                // then their relative execution order is undefined.
                return 0;
            }
        }

        /// <summary>
        /// Provides filters to apply to the specified action.
        /// </summary>
        /// <param name="context">The filter context.</param>
        public void OnProvidersExecuting(FilterProviderContext context)
        {
            // Actions with a bound parameter of type ODataQueryOptions do not support the query filter
            // The assumption is that the action will handle the querying within the action implementation
            ControllerActionDescriptor controllerActionDescriptor = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                Type returnType = controllerActionDescriptor.MethodInfo.ReturnType;
                if (ShouldAddFilter(context, returnType, controllerActionDescriptor))
                {
                    var filterDesc = new FilterDescriptor(QueryFilter, FilterScope.Global);
                    context.Results.Add(new FilterItem(filterDesc, QueryFilter));
                }
            }
        }

        /// <summary>
        /// Summary:
        /// Called in decreasing Microsoft.AspNetCore.Mvc.Filters.IFilterProvider.Order,
        /// after all Microsoft.AspNetCore.Mvc.Filters.IFilterProviders have executed once.
        /// </summary>
        /// <param name="context">The Microsoft.AspNetCore.Mvc.Filters.FilterProviderContext.</param>
        public void OnProvidersExecuted(FilterProviderContext context)
        {
        }

        private bool ShouldAddFilter(FilterProviderContext context, Type returnType, ControllerActionDescriptor controllerActionDescriptor)
        {
            // Get the inner return type if type is a task.
            Type innerReturnType = returnType;
            if (TypeHelper.IsGenericType(returnType) && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                innerReturnType = returnType.GetGenericArguments().First();
            }

            // See if this type is a SingleResult or is derived from SingleResult.
            bool isSingleResult = false;
            if (innerReturnType.IsGenericType)
            {
                Type genericType = innerReturnType.GetGenericTypeDefinition();
                Type baseType = TypeHelper.GetBaseType(innerReturnType);
                isSingleResult = (genericType == typeof(SingleResult<>) || baseType == typeof(SingleResult));
            }

            // Don't apply the filter if the result is not IQueryable() or SingleReult().
            if (!TypeHelper.IsIQueryable(innerReturnType) && !isSingleResult)
            {
                return false;
            }

            // If the controller takes a ODataQueryOptions, don't apply the filter.
            if (controllerActionDescriptor.Parameters
                .Any(parameter => TypeHelper.IsTypeAssignableFrom(typeof(ODataQueryOptions), parameter.ParameterType)))
            {
                return false;
            }

            // Don't apply a global filter if one of the same type exists.
            if (context.Results.Where(f => f.Filter?.GetType() == QueryFilter.GetType()).Any())
            {
                return false;
            }

            return true;
        }
}
}
