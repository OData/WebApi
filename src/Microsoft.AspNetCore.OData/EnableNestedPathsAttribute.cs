using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// 
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
       Justification = "We want to be able to subclass this type.")]
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class EnableNestedPathsAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            //base.OnActionExecuted(context);
            HttpResponse response = context.HttpContext.Response;
            ObjectResult responseContent = context.Result as ObjectResult;
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

            var feature = context.HttpContext.ODataFeature();
            var path = feature.Path;
            IEdmModel model = context.HttpContext.Request.GetModel();
            if (result != null)
            {
                var queryBuilder = new ODataPathQueryBuilder(result, model, path);
                object transformedResult = queryBuilder.BuildQuery();
                
                responseContent.Value = transformedResult;
            }
        }
    }
}
