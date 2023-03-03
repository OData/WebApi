//-----------------------------------------------------------------------------
// <copyright file="EnableQueryAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Results;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable querying using the OData query
    /// syntax. To avoid processing unexpected or malicious queries, use the validation settings on
    /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
    /// http://go.microsoft.com/fwlink/?LinkId=279712.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "The majority of types referenced by this method result from HttpActionExecutedContext")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "We want to be able to subclass this type.")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public partial class EnableQueryAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Performs the query composition after action is executed. It first tries to retrieve the IQueryable from the
        /// returning response message. It then validates the query from uri based on the validation settings on
        /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
        /// the response message.
        /// </summary>
        /// <param name="actionExecutedContext">The context related to this action, including the response message,
        /// request message and HttpConfiguration etc.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling",
            Justification = "The majority of types referenced by this method result from HttpActionExecutedContext")]
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            if (actionExecutedContext == null)
            {
                throw Error.ArgumentNull("actionExecutedContext");
            }

            HttpRequestMessage request = actionExecutedContext.Request;
            if (request == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionExecutedContextMustHaveRequest);
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.RequestMustContainConfiguration);
            }

            if (actionExecutedContext.ActionContext == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionExecutedContextMustHaveActionContext);
            }

            HttpActionDescriptor actionDescriptor = actionExecutedContext.ActionContext.ActionDescriptor;
            if (actionDescriptor == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionContextMustHaveDescriptor);
            }

            HttpResponseMessage response = actionExecutedContext.Response;

            if (response != null && response.IsSuccessStatusCode && response.Content != null)
            {
                ObjectContent responseContent = response.Content as ObjectContent;

                //if (responseContent == null)
                //{
                //    throw Error.Argument("actionExecutedContext", SRResources.QueryingRequiresObjectContent,
                //        response.Content.GetType().FullName);
                //}

                if (responseContent == null)
                {
                    response.TryGetContentValue(out object value);

                    Type createdODataResultType = value.GetType().GetGenericArguments().Count() > 0 ?
                        typeof(CreatedODataResult<>).MakeGenericType(value.GetType().GetGenericArguments()[0]) : null;

                    Type actionResultType = value.GetType();

                    // Get the entity object from CreatedODataResult<T> via reflection.
                    // Use the entity object to create an instance of ObjectResult.
                    if (createdODataResultType != null &&
                        (actionResultType == createdODataResultType || createdODataResultType.IsAssignableFrom(actionResultType)))
                    {
                        object entity = ((PropertyInfo)createdODataResultType.GetProperty("Entity")).GetValue(value);
                        responseContent = new ObjectContent(actionResultType,entity,null);
                    }
                }

                // Get collection from SingleResult.
                IQueryable singleResultCollection = null;
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

                    singleResultCollection = propInfo.GetValue(singleResult) as IQueryable;
                }

                // Execution the action.
                object queryResult = OnActionExecuted(
                    responseContent.Value,
                    singleResultCollection,
                    new WebApiActionDescriptor(actionDescriptor),
                    new WebApiRequestMessage(request),
                    (elementClrType) => GetModel(elementClrType, request, actionDescriptor),
                    (queryContext) => CreateAndValidateQueryOptions(request, queryContext),
                    (statusCode) => actionExecutedContext.Response = request.CreateResponse(statusCode),
                    (statusCode, message, exception) => actionExecutedContext.Response = request.CreateErrorResponse(statusCode, message, exception));

                if (queryResult != null)
                {
                    responseContent.Value = queryResult;
                }
            }
        }

        /// <summary>
        /// Create and validate a new instance of <see cref="ODataQueryOptions"/> from a query and context.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="queryContext">The query context.</param>
        /// <returns></returns>
        private ODataQueryOptions CreateAndValidateQueryOptions(HttpRequestMessage request, ODataQueryContext queryContext)
        {
            ODataQueryOptions queryOptions = new ODataQueryOptions(queryContext, request);
            ValidateQuery(request, queryOptions);

            return queryOptions;
        }

        /// <summary>
        /// Validates the OData query in the incoming request. By default, the implementation throws an exception if
        /// the query contains unsupported query parameters. Override this method to perform additional validation of
        /// the query.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="queryOptions">
        /// The <see cref="ODataQueryOptions"/> instance constructed based on the incoming request.
        /// </param>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
            Justification = "Response disposed after being sent.")]
        public virtual void ValidateQuery(HttpRequestMessage request, ODataQueryOptions queryOptions)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }

            IEnumerable<KeyValuePair<string, string>> queryParameters = request.GetQueryNameValuePairs();
            foreach (KeyValuePair<string, string> kvp in queryParameters)
            {
                if (!queryOptions.IsSupportedQueryOption(kvp.Key) &&
                     kvp.Key.StartsWith("$", StringComparison.Ordinal))
                {
                    // we don't support any custom query options that start with $
                    throw new HttpResponseException(request.CreateErrorResponse(HttpStatusCode.BadRequest,
                        Error.Format(SRResources.QueryParameterNotSupported, kvp.Key)));
                }
            }

            queryOptions.Validate(_validationSettings);
        }

        /// <summary>
        /// Gets the EDM model for the given type and request. Override this method to customize the EDM model used for
        /// querying.
        /// </summary>
        /// <param name="elementClrType">The CLR type to retrieve a model for.</param>
        /// <param name="request">The request message to retrieve a model for.</param>
        /// <param name="actionDescriptor">The action descriptor for the action being queried on.</param>
        /// <returns>The EDM model for the given type and request.</returns>
        public virtual IEdmModel GetModel(Type elementClrType, HttpRequestMessage request,
            HttpActionDescriptor actionDescriptor)
        {
            // Get model for the request
            IEdmModel model = request.GetModel();

            if (model == EdmCoreModel.Instance || model.GetTypeMappingCache().GetEdmType(elementClrType, model) == null)
            {
                // user has not configured anything or has registered a model without the element type
                // let's create one just for this type and cache it in the action descriptor
                model = actionDescriptor.GetEdmModel(elementClrType);
            }

            Contract.Assert(model != null);
            return model;
        }
    }
}
