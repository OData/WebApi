// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="AuthorizationFilterAttribute"/>
    /// </summary>
    internal sealed class AuthorizationFilterAttributeTracer : AuthorizationFilterAttribute
    {
        private const string OnAuthorizationMethodName = "OnAuthorization";

        private readonly AuthorizationFilterAttribute _innerFilter;
        private readonly ITraceWriter _traceStore;

        public AuthorizationFilterAttributeTracer(AuthorizationFilterAttribute innerFilter, ITraceWriter traceStore)
        {
            _innerFilter = innerFilter;
            _traceStore = traceStore;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            _traceStore.TraceBeginEnd(
                actionContext.ControllerContext.Request,
                TraceCategories.FiltersCategory,
                TraceLevel.Info,
                _innerFilter.GetType().Name,
                OnAuthorizationMethodName,
                beginTrace: (tr) =>
                {
                    HttpResponseMessage response = actionContext.Response;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                execute: () => { _innerFilter.OnAuthorization(actionContext);  },
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
