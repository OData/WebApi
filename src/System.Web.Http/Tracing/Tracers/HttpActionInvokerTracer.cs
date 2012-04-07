// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer for <see cref="IHttpActionInvoker"/>.
    /// </summary>
    internal class HttpActionInvokerTracer : IHttpActionInvoker
    {
        private const string InvokeActionAsyncMethodName = "InvokeActionAsync";

        private readonly IHttpActionInvoker _innerInvoker;
        private readonly ITraceWriter _traceWriter;

        public HttpActionInvokerTracer(IHttpActionInvoker innerInvoker, ITraceWriter traceWriter)
        {
            _innerInvoker = innerInvoker;
            _traceWriter = traceWriter;
        }

        Task<HttpResponseMessage> IHttpActionInvoker.InvokeActionAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            return _traceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                actionContext.ControllerContext.Request,
                TraceCategories.ActionCategory,
                TraceLevel.Info,
                _innerInvoker.GetType().Name,
                InvokeActionAsyncMethodName,

                beginTrace: (tr) =>
                {
                    tr.Message = Error.Format(
                        SRResources.TraceActionInvokeMessage,
                        FormattingUtilities.ActionInvokeToString(actionContext));
                },

                execute: () => (Task<HttpResponseMessage>)_innerInvoker.InvokeActionAsync(actionContext, cancellationToken),

                endTrace: (tr, result) =>
                {
                    HttpResponseMessage response = result;
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },

                errorTrace: null);
        }
    }
}
