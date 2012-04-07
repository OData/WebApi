// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Tracing.Tracers
{
    internal class HttpActionBindingTracer : HttpActionBinding
    {
        private const string ExecuteBindingAsyncMethodName = "ExecuteBindingAsync";

        private readonly HttpActionBinding _innerBinding;
        private readonly ITraceWriter _traceWriter;

        public HttpActionBindingTracer(HttpActionBinding innerBinding, ITraceWriter traceWriter)
        {
            _innerBinding = innerBinding;
            _traceWriter = traceWriter;

            // Properties that cannot be delegated to the inner must be replicated.
            // They must also avoid an ArgumentNullException for null values.
            if (_innerBinding.ParameterBindings != null)
            {
                ParameterBindings = _innerBinding.ParameterBindings;
            }

            if (_innerBinding.ActionDescriptor != null)
            {
                ActionDescriptor = _innerBinding.ActionDescriptor;
            }
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
