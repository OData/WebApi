// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="AuthorizationFilterAttribute"/>
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "internal type needs to override, tracer are not sealed")]
    internal class AuthorizationFilterAttributeTracer : AuthorizationFilterAttribute
    {
        private const string OnAuthorizationMethodName = "OnAuthorization";

        private readonly AuthorizationFilterAttribute _innerFilter;
        private readonly ITraceWriter _traceStore;

        public AuthorizationFilterAttributeTracer(AuthorizationFilterAttribute innerFilter, ITraceWriter traceWriter)
        {
            Contract.Assert(innerFilter != null);
            Contract.Assert(traceWriter != null);

            _innerFilter = innerFilter;
            _traceStore = traceWriter;
        }

        public override bool AllowMultiple
        {
            get
            {
                return _innerFilter.AllowMultiple;
            }
        }

        public override object TypeId
        {
            get
            {
                return _innerFilter.TypeId;
            }
        }

        public override bool Equals(object obj)
        {
            return _innerFilter.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _innerFilter.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return _innerFilter.IsDefaultAttribute();
        }

        public override bool Match(object obj)
        {
            return _innerFilter.Match(obj);
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
