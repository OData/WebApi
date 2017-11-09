// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// An implementation of <see cref="IFilterProvider" /> that applies an action filter to
    /// any action with an <see cref="IQueryable" /> or <see cref="IQueryable{T}" /> return type
    /// that doesn't bind a parameter of type <see cref="ODataQueryOptions" />.
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
        /// <param name="configuration">The server configuration.</param>
        /// <param name="actionDescriptor">The action descriptor for the action to provide filters for.</param>
        /// <returns>
        /// The filters to apply to the specified action.
        /// </returns>
        public void OnProvidersExecuting(FilterProviderContext context)
        {
            // Actions with a bound parameter of type ODataQueryOptions do not support the query filter
            // The assumption is that the action will handle the querying within the action implementation
            ControllerActionDescriptor controllerActionDescriptor = context.ActionContext.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {
                Type returnType = controllerActionDescriptor.MethodInfo.ReturnType;

                if ((IsIQueryable(returnType) /* TODO: || TypeHelper.IsTypeAssignableFrom(typeof(SingleResult), returnType)*/) &&
                    !controllerActionDescriptor.Parameters.Any(parameter => TypeHelper.IsTypeAssignableFrom(typeof(ODataQueryOptions), parameter.ParameterType)))
                {
                    context.Results.Add(new FilterItem(new FilterDescriptor(QueryFilter, FilterScope.Global)));
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

        internal static bool IsIQueryable(Type type)
        {
            return type == typeof(IQueryable) ||
                (type != null && TypeHelper.IsGenericType (type) && type.GetGenericTypeDefinition() == typeof(IQueryable<>));
        }
    }
}
