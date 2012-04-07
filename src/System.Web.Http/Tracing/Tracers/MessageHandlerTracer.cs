// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.Tracing.Tracers
{
    /// <summary>
    /// Tracer to wrap a <see cref="DelegatingHandler"/>.
    /// </summary>
    internal class MessageHandlerTracer : DelegatingHandler
    {
        private const string SendAsyncMethodName = "SendAsync";

        private readonly DelegatingHandler _innerHandler;
        private readonly ITraceWriter _traceWriter;

        public MessageHandlerTracer(DelegatingHandler innerHandler, ITraceWriter traceWriter)
        {
            _innerHandler = innerHandler;
            _traceWriter = traceWriter;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.WebAPI", "CR4001:DoNotCallProblematicMethodsOnTask", Justification = "Tracing layer needs to observer all Task completion paths")]
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync<HttpResponseMessage>(
                request,
                TraceCategories.MessageHandlersCategory,
                TraceLevel.Info,
                _innerHandler.GetType().Name,
                SendAsyncMethodName,
                beginTrace: null,
                execute: () => base.SendAsync(request, cancellationToken),
                endTrace: (tr, response) =>
                {
                    if (response != null)
                    {
                        tr.Status = response.StatusCode;
                    }
                },
                errorTrace: null);
        }
    }
}
