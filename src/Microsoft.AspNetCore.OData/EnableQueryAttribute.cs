// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// This class defines an attribute that can be applied to an action to enable querying using the OData query
    /// syntax. To avoid processing unexpected or malicious queries, use the validation settings on
    /// <see cref="EnableQueryAttribute"/> to validate incoming queries. For more information, visit
    /// http://go.microsoft.com/fwlink/?LinkId=279712.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "We want to be able to subclass this type.")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public partial class EnableQueryAttribute : ActionFilterAttribute
    {
        // Maintain the Microsoft.AspNet.OData. prefix in any new properties to avoid conflicts with user properties
        // and those of the v3 assembly.  Concern is reduced here due to addition of user type name but prefix
        // also clearly ties the property to code in this assembly.
        private const string ModelKeyPrefix = "Microsoft.AspNet.OData.Model+";

        /// <summary>
        /// Performs query validations before action is executed.
        /// </summary>
        /// <param name="context">Action context.</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context == null)
            {
                throw Error.ArgumentNull(nameof(context));
            }

            base.OnActionExecuting(context);

            HttpRequest request = context.HttpContext.Request;
            ODataPath path = request.ODataFeature().Path;

            IEdmType edmType = path.EdmType;
            IEdmType elementType = edmType.AsElementType();
            ODataQueryContext queryContext = new ODataQueryContext(request.GetModel(), elementType);

            // Create and validate the query options.
            ODataQueryOptions queryOptions = new ODataQueryOptions(queryContext, request);

            ValidateQuery(request, queryOptions);
        }

        /// <summary>
        /// Performs the query composition after action is executed. It first tries to retrieve the IQueryable from the
        /// returning response message. It then validates the query from uri based on the validation settings on
        /// <see cref="EnableQueryAttribute"/>. It finally applies the query appropriately, and reset it back on
        /// the response message.
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

            ActionDescriptor actionDescriptor = actionExecutedContext.ActionDescriptor;
            if (actionDescriptor == null)
            {
                throw Error.Argument("actionExecutedContext", SRResources.ActionContextMustHaveDescriptor);
            }

            HttpResponse response = actionExecutedContext.HttpContext.Response;

            // Check is the response is set and successful.
            if (response != null && IsSuccessStatusCode(response.StatusCode) && actionExecutedContext.Result != null)
            {
                // actionExecutedContext.Result might also indicate a status code that has not yet
                // been applied to the result; make sure it's also successful.
                StatusCodeResult statusCodeResult = actionExecutedContext.Result as StatusCodeResult;
                if (statusCodeResult == null || IsSuccessStatusCode(statusCodeResult.StatusCode))
                {
                    ObjectResult responseContent = actionExecutedContext.Result as ObjectResult;
                    if (responseContent != null)
                    {
                        //throw Error.Argument("actionExecutedContext", SRResources.QueryingRequiresObjectContent,
                        //    actionExecutedContext.Result.GetType().FullName);

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
                            new WebApiActionDescriptor(actionDescriptor as ControllerActionDescriptor),
                            new WebApiRequestMessage(request),
                            (elementClrType) => GetModel(elementClrType, request, actionDescriptor),
                            (queryContext) => CreateAndValidateQueryOptions(request, queryContext),
                            (statusCode) => actionExecutedContext.Result = new StatusCodeResult((int)statusCode),
                            (statusCode, message, exception) => actionExecutedContext.Result = CreateBadRequestResult(message, exception));

                        if (queryResult != null)
                        {
                            responseContent.Value = queryResult;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Create and validate a new instance of <see cref="ODataQueryOptions"/> from a query and context.
        /// </summary>
        /// <param name="request">The incoming request.</param>
        /// <param name="queryContext">The query context.</param>
        /// <returns></returns>
        private ODataQueryOptions CreateAndValidateQueryOptions(HttpRequest request, ODataQueryContext queryContext)
        {
            return new ODataQueryOptions(queryContext, request);
        }

        /// <summary>
        /// Determine if the status code indicates success.
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <returns>True if the response has a success status code; false otherwise.</returns>
        private static bool IsSuccessStatusCode(int statusCode)
        {
            return statusCode >= 200 && statusCode < 300;
        }

        /// <summary>
        /// Create an error response.
        /// </summary>
        /// <param name="message">The message of the error.</param>
        /// <param name="exception">The error exception if any.</param>
        /// <returns>A SerializableError.</returns>
        /// <remarks>This function is recursive.</remarks>
        public static SerializableError CreateErrorResponse(string message, Exception exception = null)
        {
            // The key values mimic the behavior of HttpError in AspNet. It's a fine format
            // and many of the test cases expect it.
            SerializableError error = new SerializableError();
            if (!String.IsNullOrEmpty(message))
            {
                error.Add(SerializableErrorKeys.MessageKey, message);
            }

            if (exception != null)
            {
                error.Add(SerializableErrorKeys.ExceptionMessageKey, exception.Message);
                error.Add(SerializableErrorKeys.ExceptionTypeKey, exception.GetType().FullName);
                error.Add(SerializableErrorKeys.StackTraceKey, exception.StackTrace);
                if (exception.InnerException != null)
                {
                    error.Add(SerializableErrorKeys.InnerExceptionKey, CreateErrorResponse(String.Empty, exception.InnerException));
                }
            }

            return error;
        }

        /// <summary>
        /// Create a BadRequestObjectResult.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="exception">The exception.</param>
        /// <returns>A BadRequestObjectResult.</returns>
        private static BadRequestObjectResult CreateBadRequestResult(string message, Exception exception)
        {
            SerializableError error = CreateErrorResponse(message, exception);
            return new BadRequestObjectResult(error);
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
        public virtual void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            if (queryOptions == null)
            {
                throw Error.ArgumentNull("queryOptions");
            }

            IEnumerable<KeyValuePair<string, StringValues>> queryParameters = request.Query;
            foreach (KeyValuePair<string, StringValues> kvp in queryParameters)
            {
                if (!queryOptions.IsSupportedQueryOption(kvp.Key) &&
                     kvp.Key.StartsWith("$", StringComparison.Ordinal))
                {
                    // we don't support any custom query options that start with $
                    // this should be caught be OnActionExecuted().
                    throw new ArgumentOutOfRangeException(kvp.Key);
                }
            }

            queryOptions.Validate(_validationSettings);
        }
        /// <summary>
        /// Gets the EDM model for the given type and request.Override this method to customize the EDM model used for
        /// querying.
        /// </summary>
        /// <param name = "elementClrType" > The CLR type to retrieve a model for.</param>
        /// <param name = "request" > The request message to retrieve a model for.</param>
        /// <param name = "actionDescriptor" > The action descriptor for the action being queried on.</param>
        /// <returns>The EDM model for the given type and request.</returns>
        public virtual IEdmModel GetModel(
            Type elementClrType,
            HttpRequest request,
            ActionDescriptor actionDescriptor)
        {
            // Get model for the request
            IEdmModel model = request.GetModel();

            if (model == EdmCoreModel.Instance || model.GetEdmType(elementClrType) == null)
            {
                // user has not configured anything or has registered a model without the element type
                // let's create one just for this type and cache it in the action descriptor
                model = actionDescriptor.GetEdmModel(request, elementClrType);
            }

            Contract.Assert(model != null);
            return model;
        }
    }
}