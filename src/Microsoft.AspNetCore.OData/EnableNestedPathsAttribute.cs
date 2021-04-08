using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// TODO: add summary
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
       Justification = "We want to be able to subclass this type.")]
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
        /// 
        /// </summary>
        /// <param name="actionExecutedContext"></param>
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

            ActionDescriptor actionDescriptor = actionExecutedContext.ActionDescriptor;
            if (actionDescriptor == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionContextMustHaveDescriptor);
            }



            //base.OnActionExecuted(context);
            HttpResponse response = actionExecutedContext.HttpContext.Response;

            ObjectResult responseContent = actionExecutedContext.Result as ObjectResult;
            var result = responseContent.Value as IQueryable;
            SingleResult singleResult = responseContent.Value as SingleResult;
            if (singleResult != null)
            {
                // This could be a SingleResult, which has the property Queryable.
                // But it could be a SingleResult() or SingleResult<T>. Sort by number of parameters
                // on the property and get the one with the most parameters.
                PropertyInfo propInfo = responseContent.Value.GetType().GetProperties()
                    .OrderBy(p => p.GetIndexParameters().Count())
                    .Where(p => p.Name.Equals("Queryable"))
                    .LastOrDefault();

                result = propInfo.GetValue(singleResult) as IQueryable;
            }

            
            if (result != null)
            {
                ODataPath path = actionExecutedContext.HttpContext.ODataFeature().Path;
                IEdmModel model = actionExecutedContext.HttpContext.Request.GetModel();

                var queryBuilder = new ODataPathQueryBuilder(result, model, path);
                ODataPathQueryResult transformedResult = queryBuilder.BuildQuery();

                if (transformedResult == null)
                {
                    // TODO: is this the best way to return 404
                    actionExecutedContext.Result = new NotFoundObjectResult(null);
                }
                else if (path.EdmType.TypeKind == EdmTypeKind.Collection || transformedResult.HasCountSegment)
                {
                    responseContent.Value = transformedResult.Result;
                }
                else
                {
                    Type elementType = transformedResult.Result.ElementType;
                    object singleValue = EnableQueryAttribute.SingleOrDefault(transformedResult.Result, new WebApiActionDescriptor(actionDescriptor as ControllerActionDescriptor));
             
                    if (singleValue == null)
                    {
                        // TODO: is this the best way to return 404
                        actionExecutedContext.Result = new NotFoundObjectResult(null);
                    }
                    else
                    {
                        responseContent.Value = singleValue;
                    }
                }
            }
        }
    }
}
