// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="ActionFilterAttribute"/>.
    /// </summary>
    internal sealed class ActionFilterAttributeTracer : ActionFilterAttribute
    {
        private const string ActionExecutedMethodName = "ActionExecuted";
        private const string ActionExecutingMethodName = "ActionExecuting";

        private readonly ActionFilterAttribute _innerFilter;
        private readonly ITraceWriter _traceWriter;

        public ActionFilterAttributeTracer(ActionFilterAttribute innerFilter, ITraceWriter traceWriter)
        {
            _innerFilter = innerFilter;
            _traceWriter = traceWriter;
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            _traceWriter.TraceBeginEnd(
                actionExecutedContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                _innerFilter.GetType().Name,
                ActionExecutedMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                        SRResources.TraceActionFilterMessage,
                                        FormattingUtilities.ActionDescriptorToString(
                                            actionExecutedContext.ActionContext.ActionDescriptor));
                    tr.Exception = actionExecutedContext.Exception;
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                execute: () =>
                {
                    _innerFilter.OnActionExecuted(actionExecutedContext);
                },
                endTrace: (tr) =>
                {
                    tr.Exception = actionExecutedContext.Exception;
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: (tr) =>
                {
                    HttpResponseMessage response = actionExecutedContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                });
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            _traceWriter.TraceBeginEnd(
                actionContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                _innerFilter.GetType().Name,
                ActionExecutingMethodName,
                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                                    SRResources.TraceActionFilterMessage,
                                    FormattingUtilities.ActionDescriptorToString(
                                        actionContext.ActionDescriptor));

                    HttpResponseMessage response = actionContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                execute: () =>
                {
                    _innerFilter.OnActionExecuting(actionContext);
                },
                endTrace: (tr) =>
                {
                    HttpResponseMessage response = actionContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: (tr) =>
                {
                    HttpResponseMessage response = actionContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                });
        }
    }
}
