using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    public class HttpActionBindingTracer : HttpActionBinding
    {
        private const string ExecuteBindingAsyncMethodName = "ExecuteBindingAsync";

        private readonly HttpActionBinding _innerBinding;
        private readonly ITraceWriter _traceWriter;

        public HttpActionBindingTracer(HttpActionBinding innerBinding, ITraceWriter traceWriter)
        {
            _innerBinding = innerBinding;
            _traceWriter = traceWriter;
        }

        public override Task ExecuteBindingAsync(Controllers.HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return _traceWriter.TraceBeginEndAsync(
                actionContext.ControllerContext.Request,
                TraceCategories.ModelBindingCategory,
                TraceLevel.Info,
                _innerBinding.GetType().Name,
                ExecuteBindingAsyncMethodName,
                beginTrace: null,
                execute: () => _innerBinding.ExecuteBindingAsync(actionContext, cancellationToken),
                endTrace: (tr) =>
                {
                    if (!actionContext.ModelState.IsValid)
                    {
                        tr.Message = Error.Format(SRResources.TraceModelStateInvalidMessage,
                                                  FormattingUtilities.ModelStateToString(
                                                        actionContext.ModelState));
                    }
                    else
                    {
                        if (actionContext.ActionDescriptor.GetParameters().Count > 0)
                        {
                            tr.Message = Error.Format(SRResources.TraceValidModelState,
                                                      FormattingUtilities.ActionArgumentsToString(
                                                            actionContext.ActionArguments));
                        }
                    }
                },
                errorTrace: null);
        }
    }
}
