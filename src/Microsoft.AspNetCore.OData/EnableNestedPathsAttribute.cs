// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to allow handling
    /// of arbitrarily nested paths. The result of the action should be an IQueryable
    /// or SingleResult. The sequence of property and key accesses in the path will be
    /// applied to the result of the action through query transformations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class EnableNestedPathsAttribute : ActionFilterAttribute
    {
        /// <inherit/>
        public EnableNestedPathsAttribute(): base()
        {
            // ensures this filter is executed before [EnableQuery] which has default order 0
            // since the OnActionExecuted method is executed after the action,
            // a filter with a lower order will be executed after a filter with a higher order
            Order = 1;
        }

        /// <summary>
        /// Transforms the result of the action based on the sequence of property accesses in the odata path
        /// after the action is executed.
        /// It first tries to retrieve the IQueryable from the
        /// returning response message. It then uses the <see cref="ODataPathQueryBuilder"/> to transform
        /// the query and sets the result back to the response message.
        /// </summary>
        /// <param name="actionExecutedContext">The context related to this action, including the response message,
        /// request message and HttpConfiguration etc.</param>
        public override void OnActionExecuted(ActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            HttpRequest request = actionExecutedContext.HttpContext.Request;
            if (request == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionExecutedContextMustHaveRequest);
            }

            ControllerActionDescriptor actionDescriptor = actionExecutedContext.ActionDescriptor as ControllerActionDescriptor;
            if (actionDescriptor == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionContextMustHaveDescriptor);
            }

            HttpResponse response = actionExecutedContext.HttpContext.Response;

            ObjectResult responseContent = actionExecutedContext.Result as ObjectResult;
            IQueryable result = responseContent.Value as IQueryable;
            SingleResult singleResult = responseContent.Value as SingleResult;
            if (singleResult != null)
            {
                // This could be a SingleResult, which has the property Queryable.
                // But it could be a SingleResult() or SingleResult<T>. Sort by number of parameters
                // on the property and get the one with the most parameters.
                PropertyInfo propInfo = singleResult.GetType().GetProperties()
                    .Where(p => p.Name.Equals("Queryable"))
                    .OrderByDescending(p => p.GetIndexParameters().Count())
                    .FirstOrDefault();

                result = propInfo.GetValue(singleResult) as IQueryable;
            }

            if (result != null)
            {
                ODataPath path = actionExecutedContext.HttpContext.ODataFeature().Path;
                IEdmModel model = actionExecutedContext.HttpContext.Request.GetModel();

                var queryBuilder = new ODataPathQueryBuilder(result, path);
                ODataPathQueryResult transformedResult = queryBuilder.BuildQuery();

                if (transformedResult == null)
                {
                    actionExecutedContext.Result = new NotFoundObjectResult(null);
                }
                else if (path.EdmType.TypeKind == EdmTypeKind.Collection || transformedResult.HasCountSegment)
                {
                    responseContent.Value = transformedResult.Result;
                }
                else
                {
                    Type elementType = transformedResult.Result.ElementType;

                    // If we return the IQueryable as the result, then the response will be returned as a collection
                    // so we have to return a single result. We can either return the materialized single result using SingleOrDefault,
                    // which will run the query against the underlying data source. Or we can wrap the IQueryable result with
                    // a SingleResult<T>.
                    // If the action has [EnableQuery], then we return a SingleResult<T> which allows query options to be
                    // applied to the IQueryable. If we returned the materialized object, then query options like $expand
                    // would return null because navigation properties are not fetched from relational databases by default.
                    // If the action does not have [EnableQuery], then we run the query using SingleOrDefault and return the
                    // actual object. If we returned a SingleResult<T> in this case, we'd get an error because the serializer
                    // does not recognize it.
                    if (actionDescriptor.MethodInfo.GetCustomAttributes<EnableQueryAttribute>().Any())
                    {
                        // calls SingleResult.Create<T>(IQueryable<T>)
                        responseContent.Value = ExpressionHelpers.CreateSingleResult(transformedResult.Result, transformedResult.Result.ElementType);
                    }
                    else
                    {
                        try
                        {
                            object singleValue = QueryHelpers.SingleOrDefault(transformedResult.Result, new WebApiActionDescriptor(actionDescriptor));

                            if (singleValue == null)
                            {
                                actionExecutedContext.Result = new NotFoundObjectResult(null);
                            }
                            else
                            {
                                responseContent.Value = singleValue;
                            }
                        }
                        catch (NullReferenceException)
                        {
                            actionExecutedContext.Result = new NotFoundObjectResult(null);
                        }
                    }
                }
            }
        }
    }
}
